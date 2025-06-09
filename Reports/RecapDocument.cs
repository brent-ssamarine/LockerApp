using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using AccessMigrationApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public class RecapDocument : BaseDocument
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly string _inspectedBy;
    private List<RecapViewModel>? _recapItems;

    public RecapDocument(
        IServiceProvider serviceProvider,
        DateTime startDate,
        DateTime endDate,
        string inspectedBy) : base(serviceProvider)
    {
        _startDate = startDate;
        _endDate = endDate;
        _inspectedBy = inspectedBy;
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
                page.Margin(20);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Inventory Transfer Recap Report")
                    .Style(TitleStyle);

                column.Item().Text($"Period: {_startDate:MM/dd/yyyy} - {_endDate:MM/dd/yyyy}")
                    .Style(SubtitleStyle);

                if (!string.IsNullOrEmpty(_inspectedBy))
                {
                    column.Item().Text($"Inspected By: {_inspectedBy}")
                        .Style(SubtitleStyle);
                }
            });

            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Generated: {DateTime.Now:MM/dd/yyyy HH:mm}")
                    .Style(DefaultTextStyle)
                    .AlignRight();
            });
        });
    }

    private async Task LoadData()
    {
        if (_recapItems != null) return;

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();

        var query = dbContext.RptRecaps
            .Where(r => r.TransferDate >= _startDate && r.TransferDate <= _endDate);        if (!string.IsNullOrEmpty(_inspectedBy))
        {
            query = query.Where(r => r.InspectedBy == _inspectedBy);
        }

        _recapItems = await query
            .Select(r => new RecapViewModel
            {
                InvLocId = r.InvLocId ?? 0,
                Location = r.Location != null ? r.Location.ToString() : "",
                ItemId = r.Item ?? "",
                ItemName = r.ItemName ?? "",
                OnHand = r.OnHand ?? 0,
                Description = r.Description ?? "",
                LocationType = r.LocType ?? "",
                TransferDate = r.TransferDate,
                Quantity = r.Quantity ?? 0,
                Consumed = r.Consumed.HasValue ? (r.Consumed.Value == 1 ? 1 : 0) : 0,
                InspectedBy = r.InspectedBy ?? ""
            })
            .OrderBy(r => r.TransferDate)
            .ThenBy(r => r.Location)
            .ThenBy(r => r.ItemName)
            .ToListAsync();
    }

    private void ComposeContent(IContainer container)
    {
        LoadData().Wait();

        container.PaddingVertical(10).Column(column =>
        {
            // Table header
            column.Item().Row(row =>
            {
                row.RelativeItem(2).Text("Date").Style(HeaderStyle);
                row.RelativeItem(1).Text("Location").Style(HeaderStyle);
                row.RelativeItem(3).Text("Item").Style(HeaderStyle);
                row.RelativeItem(2).Text("Description").Style(HeaderStyle);
                row.RelativeItem(1).Text("Quantity").Style(HeaderStyle).AlignRight();
                row.RelativeItem(1).Text("On Hand").Style(HeaderStyle).AlignRight();
                row.RelativeItem(2).Text("Inspector").Style(HeaderStyle);
            });

            // Table content
            if (_recapItems != null && _recapItems.Any())
            {                DateTime? currentDate = null;
                foreach (var item in _recapItems)
                {
                    // Add date separator if it's a new date
                    if (item.TransferDate?.Date != currentDate?.Date)
                    {
                        column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium).Row(row =>
                        {
                            row.RelativeItem().Text(item.FormattedTransferDate).Style(HeaderStyle);
                        });
                        currentDate = item.TransferDate;
                    }

                    column.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Row(row =>
                    {
                        row.RelativeItem(2).Text(item.FormattedTransferDate).Style(DefaultTextStyle);
                        row.RelativeItem(1).Text(item.Location).Style(DefaultTextStyle);
                        row.RelativeItem(3).Text(item.ItemName ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(item.Description ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(1).AlignRight().Text(item.FormattedQuantity).Style(DefaultTextStyle);
                        row.RelativeItem(1).AlignRight().Text(item.FormattedOnHand).Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(item.InspectedBy ?? "").Style(DefaultTextStyle);
                    });
                }
            }
            else
            {
                column.Item().PaddingTop(20).Text("No records found for the specified criteria.")
                    .Style(DefaultTextStyle);
            }

            // Summary section
            if (_recapItems != null && _recapItems.Any())
            {
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>                    {
                        c.Item().Text("Summary by Location").Style(HeaderStyle);
                        
                        var locationSummary = _recapItems
                            .GroupBy(r => r.Location)
                            .Select(g => new { Location = g.Key ?? "Unknown", Count = g.Count() })
                            .OrderBy(s => s.Location);

                        foreach (var item in locationSummary)
                        {
                            c.Item().Text($"Location {item.Location}: {item.Count} transfers").Style(DefaultTextStyle);
                        }
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Summary by Inspector").Style(HeaderStyle);
                        
                        var inspectorSummary = _recapItems
                            .GroupBy(r => r.InspectedBy)
                            .Select(g => new { Inspector = g.Key ?? "Unknown", Count = g.Count() })
                            .OrderBy(s => s.Inspector);

                        foreach (var item in inspectorSummary)
                        {
                            c.Item().Text($"{item.Inspector}: {item.Count} transfers").Style(DefaultTextStyle);
                        }                    });
                });

                // Total quantity summary
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        var totalQuantity = _recapItems.Sum(r => r.Quantity ?? 0);
                        c.Item().Text($"Total Quantity Transferred: {totalQuantity:N2}").Style(HeaderStyle);
                    });
                });
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Page ").Style(DefaultTextStyle);
                text.CurrentPageNumber().Style(DefaultTextStyle);
                text.Span(" of ").Style(DefaultTextStyle);
                text.TotalPages().Style(DefaultTextStyle);
            });

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span($"Total Records: {_recapItems?.Count ?? 0}").Style(DefaultTextStyle);
            });
        });
    }
}