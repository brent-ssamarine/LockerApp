using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using AccessMigrationApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public class MaterialListDocument : BaseDocument
{
    private readonly int? _locationId;
    private string _locationName = "";
    private string _berth = "";
    private DateTime _startDate = DateTime.Today;
    private List<MaterialListViewModel>? _materialItems;

    public MaterialListDocument(
        IServiceProvider serviceProvider,
        int? locationId) : base(serviceProvider)
    {
        _locationId = locationId;
    }

    public async Task PrepareAsync()
    {
        await LoadData();
    }

    public override void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.Letter);
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
            // First header line - Company info and date/page
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("SSA Marine Canada").FontSize(8);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8));
                    text.Span($"{DateTime.Now:MM/dd/yy h:mm tt}");
                    text.Line("");
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });

            // Second header line
            column.Item().Text("locker:materiallist").FontSize(8);

            // Title section with vessel, berth, and starting date
            column.Item().PaddingVertical(5).Row(row =>
            {
                row.RelativeItem().Text(_locationName).Bold().FontSize(12).AlignCenter();
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Berth  ").FontSize(10);
                    text.Span(_berth).Bold().FontSize(10);
                });
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Started  ").FontSize(10);
                    text.Span(_startDate.ToString("M/d/yyyy")).Bold().FontSize(10);
                });
            });
        });
    }    private async Task LoadData()
    {
        if (_materialItems != null) return;

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();

        // Load material list data based on location ID and convert to ViewModel
        var query = dbContext.MaterialLists.AsQueryable();

        if (_locationId.HasValue)
        {
            query = query.Where(m => m.LocationId == _locationId.Value);
        }        var materialList = await query
            .Where(m => m.ToLocationName == "LOCKER") // Filter for items going to LOCKER
            .OrderBy(m => m.ItemName)
            .ToListAsync();

        // Convert to MaterialListViewModel and extract the specific fields
        _materialItems = materialList.Select(m => new MaterialListViewModel
        {
            LocationId = m.LocationId,
            FromLocationName = m.FromLocationName,
            Berth = m.Berth,
            StartDate = m.StartDate,
            ItemName = m.ItemName,
            Description = m.Description,
            Quantity = m.Quantity,
            InvType = m.InvType
        }).ToList();

        // Extract header information - group by loc_id to get header data
        if (_materialItems.Any())
        {
            var headerGroup = _materialItems
                .GroupBy(m => m.LocationId)
                .FirstOrDefault();
                
            if (headerGroup != null)
            {
                var headerItem = headerGroup.First();
                _locationName = headerItem.FromLocationName ?? "";
                _berth = headerItem.Berth ?? "";
                _startDate = headerItem.StartDate ?? DateTime.Today;
            }
        }
    }

    private void ComposeContent(IContainer container)
    {
        LoadData().Wait();

        if (_materialItems == null || !_materialItems.Any())
        {
            container.Text("No materials found.");
            return;
        }

        container.Column(column =>
        {
            // Main table
            column.Item().Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);     // Item Description  
                    columns.ConstantColumn(60);    // Quantity
                    columns.ConstantColumn(80);    // InvType
                });

                // Table header
                table.Header(header =>
                {
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("ITEM DESCRIPTION").Bold();
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("QTY").Bold();
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("TYPE").Bold();
                });                // Table content - show individual records without grouping
                foreach (var item in _materialItems)
                {
                    // Apply VBA logic - only show items with quantity > 0
                    if (item.Quantity > 0)
                    {
                        var qtyDisplay = item.Quantity > 0 ? item.Quantity?.ToString("0") ?? "" : "";
                        var itemDisplay = !string.IsNullOrWhiteSpace(item.Description) 
                            ? $"{item.ItemName?.Trim()} - {item.Description.Trim()}"
                            : item.ItemName?.Trim() ?? "";

                        table.Cell().Border(0.5f).BorderColor(Colors.Black).PaddingVertical(2).PaddingLeft(5).Text(itemDisplay);
                        table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignRight().PaddingVertical(2).PaddingRight(5).Text(qtyDisplay);
                        table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text(item.InvType ?? "");
                    }
                }
            });

            // Inspection section
            column.Item().PaddingTop(30).Border(0.5f).BorderColor(Colors.Black).Padding(10).Column(innerColumn =>
            {
                innerColumn.Item().Text("Returned Gear/Equipment Inspected By:").Bold().FontSize(10);
                
                innerColumn.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                });

                innerColumn.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        // Footer is empty in this report format
    }
}
