using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using AccessMigrationApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccessMigrationApp.Reports;

public class GearListDocument : IDocument
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _locationId;
    private readonly string _locationName;
    private readonly string _berth;
    private readonly int _finished;
    private List<InventoryOnsiteViewModel>? _gearItems;

    public bool HasData => _gearItems != null && _gearItems.Any();

    public GearListDocument(
        IServiceProvider serviceProvider,
        int locationId,
        string locationName,
        string berth,
        int finished)
    {
        _serviceProvider = serviceProvider;
        _locationId = locationId;
        _locationName = locationName;
        _berth = berth;
        _finished = finished;
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public async Task PrepareAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();

        var query = dbContext.InventoryOnsites
            .FromSqlRaw("SELECT * FROM dbo.inventory_onsite")
            .Where(il => il.LocationId == _locationId);

        // Different location selected (_finished = 2): filter out materials and zero quantities
        if (_finished == 2)
        {
            query = query.Where(il => 
                il.OnHand != 0 && 
                il.Inventory!.InvType != "MATRL");
        }        // Current location, not finished (_finished = 0): filter zero quantities
        else if (_finished == 0)
        {
            query = query.Where(il => il.OnHand != 0);
        }
        // Current location, finished (_finished = 1): no additional filters

        _gearItems = await query
            .Select(il => new InventoryOnsiteViewModel
            {
                InvlocId = il.Id,
                ItemId = il.ItemId!,
                ItemName = il.ItemName!,
                NewItemName = il.ItemName!,
                Description = il.Description ?? "",
                NewDescription = il.Description ?? "",
                LocationId = il.LocationId ?? 0,
                LocationName = il.Location!.Name!,
                Berth = il.Location.Berth!,
                OnHand = il.OnHand ?? 0,
                NewOnHand = il.OnHand ?? 0,
                IsBillable = il.Inventory!.Billable == 1,
                NewIsBillable = il.Inventory!.Billable == 1,
                IsModified = false
            })
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.Legal);
            page.Margin(1, Unit.Centimetre);
            page.DefaultTextStyle(x => x
                .FontSize(10)
                .FontFamily("Arial"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Company Info
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("SSA Marine Canada").FontSize(8);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8));
                    text.Span($"{DateTime.Now:MM/dd/yy h:mm tt}");
                    text.Span("  Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });

            column.Item().Text("locker: gearlist").FontSize(8);

            // Title Info
            column.Item().PaddingVertical(5).Row(row =>
            {
                row.RelativeItem(2).Text(text =>
                {
                    text.Span("For the  ").FontSize(10);
                    text.Span(_locationName).Bold().FontSize(10);
                });

                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.Span("By:  ").FontSize(10);
                    text.Span(_berth).Bold().FontSize(10);
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Starting  ").FontSize(10);
                    text.Span(DateTime.Now.AddDays(30).ToString("MM/dd/yyyy")).Bold().FontSize(10);
                });
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_gearItems == null || !_gearItems.Any())
        {
            container.Height(200) // Set a minimum height to center vertically
                   .AlignCenter()
                   .AlignMiddle()
                   .Text(text =>
                   {
                       text.Span("NO GEAR ITEMS FOUND").Bold().FontSize(16);
                       text.EmptyLine();
                       text.Span("There are no gear items to display for the selected location.").FontSize(12);
                   });
            return;
        }

        container.Column(column =>
        {
            // Table section
            column.Item().Table(table =>
            {
                // Set table properties
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);    // OUT
                    columns.ConstantColumn(30);    // Quantity
                    columns.RelativeColumn();      // Item Name
                    columns.ConstantColumn(40);    // IN
                });

                // Table header
                table.Header(header =>
                {
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("OUT").Bold();
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("").Bold();
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("").Bold();
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("IN").Bold();
                });

                // Table content
                foreach (var item in _gearItems)
                {
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("");
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignRight().PaddingRight(5).Text(item.OnHand.ToString("0"));
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).PaddingLeft(5).Text(item.ItemName);
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("");
                }
            });

            // Inspection box at the end of content
            column.Item().PaddingTop(20).Border(0.5f).BorderColor(Colors.Black).Padding(10).Column(innerColumn =>
            {
                innerColumn.Item().Text("Returned Gear/Equipment Inspected By:").Bold();
                
                innerColumn.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                });

                innerColumn.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignRight().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(8));
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }
} 