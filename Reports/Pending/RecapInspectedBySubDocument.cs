using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public class RecapInspectedBySubDocument : BaseDocument
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly string _subInspector;
    private List<Recap>? _recapItems;

    public RecapInspectedBySubDocument(
        IServiceProvider serviceProvider,
        DateTime startDate,
        DateTime endDate,
        string subInspector) : base(serviceProvider)
    {
        _startDate = startDate;
        _endDate = endDate;
        _subInspector = subInspector;
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
                column.Item().Text("Recap Report - Sub Inspector")
                    .Style(TitleStyle);

                column.Item().Text($"Period: {_startDate:MM/dd/yyyy} - {_endDate:MM/dd/yyyy}")
                    .Style(SubtitleStyle);

                column.Item().Text($"Sub Inspector: {_subInspector}")
                    .Style(SubtitleStyle);
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

        _recapItems = await dbContext.Recaps
            .Where(r => r.Date >= _startDate && 
                       r.Date <= _endDate && 
                       r.SubInspector == _subInspector)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Location)
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
                row.RelativeItem(3).Text("Location").Style(HeaderStyle);
                row.RelativeItem(2).Text("Status").Style(HeaderStyle);
                row.RelativeItem(3).Text("Notes").Style(HeaderStyle);
                row.RelativeItem(2).Text("Primary Inspector").Style(HeaderStyle);
            });

            // Table content
            if (_recapItems != null)
            {
                string? currentDate = null;
                foreach (var item in _recapItems)
                {
                    // Add date separator if it's a new date
                    string itemDate = item.Date?.ToString("MM/dd/yyyy") ?? "";
                    if (itemDate != currentDate)
                    {
                        column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium).Row(row =>
                        {
                            row.RelativeItem().Text(itemDate).Style(HeaderStyle);
                        });
                        currentDate = itemDate;
                    }

                    column.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Row(row =>
                    {
                        row.RelativeItem(2).Text(itemDate).Style(DefaultTextStyle);
                        row.RelativeItem(3).Text(item.Location ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(item.Status ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(3).Text(item.Notes ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(item.Inspector ?? "").Style(DefaultTextStyle);
                    });
                }
            }

            // Summary section
            if (_recapItems != null && _recapItems.Any())
            {
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Summary by Location").Style(HeaderStyle);
                        
                        var summary = _recapItems
                            .GroupBy(r => r.Location)
                            .Select(g => new { Location = g.Key ?? "Unknown", Count = g.Count() });

                        foreach (var item in summary)
                        {
                            c.Item().Text($"{item.Location}: {item.Count} inspections").Style(DefaultTextStyle);
                        }
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Summary by Status").Style(HeaderStyle);
                        
                        var summary = _recapItems
                            .GroupBy(r => r.Status)
                            .Select(g => new { Status = g.Key ?? "Unknown", Count = g.Count() });

                        foreach (var item in summary)
                        {
                            c.Item().Text($"{item.Status}: {item.Count}").Style(DefaultTextStyle);
                        }
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
                text.Span($"Total Inspections: {_recapItems?.Count ?? 0}").Style(DefaultTextStyle);
            });
        });
    }
} 