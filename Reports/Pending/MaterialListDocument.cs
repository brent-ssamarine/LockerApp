using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public class MaterialListDocument : BaseDocument
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly string? _category;
    private List<MaterialList>? _materials;

    public MaterialListDocument(
        IServiceProvider serviceProvider,
        DateTime startDate,
        DateTime endDate,
        string? category = null) : base(serviceProvider)
    {
        _startDate = startDate;
        _endDate = endDate;
        _category = category;
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
                column.Item().Text("Material List Report")
                    .Style(TitleStyle);

                column.Item().Text($"Period: {_startDate:MM/dd/yyyy} - {_endDate:MM/dd/yyyy}")
                    .Style(SubtitleStyle);

                if (!string.IsNullOrEmpty(_category))
                {
                    column.Item().Text($"Category: {_category}")
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
        if (_materials != null) return;

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();

        var query = dbContext.MaterialLists
            .Where(m => m.Date >= _startDate && m.Date <= _endDate);

        if (!string.IsNullOrEmpty(_category))
        {
            query = query.Where(m => m.Category == _category);
        }

        _materials = await query
            .OrderBy(m => m.Date)
            .ThenBy(m => m.MaterialName)
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
                row.RelativeItem(3).Text("Material").Style(HeaderStyle);
                row.RelativeItem(2).Text("Category").Style(HeaderStyle);
                row.RelativeItem(1).Text("Quantity").Style(HeaderStyle).AlignRight();
                row.RelativeItem(2).Text("Unit").Style(HeaderStyle);
                row.RelativeItem(2).Text("Location").Style(HeaderStyle);
            });

            // Table content
            if (_materials != null)
            {
                foreach (var material in _materials)
                {
                    column.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Row(row =>
                    {
                        row.RelativeItem(2).Text(material.Date?.ToString("MM/dd/yyyy") ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(3).Text(material.MaterialName ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(material.Category ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(1).AlignRight().Text(material.Quantity?.ToString("N2") ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(material.Unit ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(material.Location ?? "").Style(DefaultTextStyle);
                    });
                }
            }

            // Summary section
            if (_materials != null && _materials.Any())
            {
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Summary by Category").Style(HeaderStyle);
                        
                        var summary = _materials
                            .GroupBy(m => m.Category)
                            .Select(g => new { Category = g.Key ?? "Uncategorized", Count = g.Count() });

                        foreach (var item in summary)
                        {
                            c.Item().Text($"{item.Category}: {item.Count} items").Style(DefaultTextStyle);
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
                text.Span($"Total Items: {_materials?.Count ?? 0}").Style(DefaultTextStyle);
            });
        });
    }
} 