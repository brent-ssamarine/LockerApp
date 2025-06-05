using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public class TestSchedulerDocument : BaseDocument
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly string? _testType;
    private List<TestScheduler>? _tests;

    public TestSchedulerDocument(
        IServiceProvider serviceProvider,
        DateTime startDate,
        DateTime endDate,
        string? testType = null) : base(serviceProvider)
    {
        _startDate = startDate;
        _endDate = endDate;
        _testType = testType;
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
                column.Item().Text("Test Schedule Report")
                    .Style(TitleStyle);

                column.Item().Text($"Period: {_startDate:MM/dd/yyyy} - {_endDate:MM/dd/yyyy}")
                    .Style(SubtitleStyle);

                if (!string.IsNullOrEmpty(_testType))
                {
                    column.Item().Text($"Test Type: {_testType}")
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
        if (_tests != null) return;

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();

        var query = dbContext.TestSchedulers
            .Where(t => t.TestDate >= _startDate && t.TestDate <= _endDate);

        if (!string.IsNullOrEmpty(_testType))
        {
            query = query.Where(t => t.TestType == _testType);
        }

        _tests = await query
            .OrderBy(t => t.TestDate)
            .ThenBy(t => t.TestType)
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
                row.RelativeItem(2).Text("Test Date").Style(HeaderStyle);
                row.RelativeItem(2).Text("Type").Style(HeaderStyle);
                row.RelativeItem(3).Text("Equipment").Style(HeaderStyle);
                row.RelativeItem(2).Text("Location").Style(HeaderStyle);
                row.RelativeItem(2).Text("Status").Style(HeaderStyle);
                row.RelativeItem(2).Text("Completed").Style(HeaderStyle);
            });

            // Table content
            if (_tests != null)
            {
                string? currentDate = null;
                foreach (var test in _tests)
                {
                    // Add date separator if it's a new date
                    string testDate = test.TestDate?.ToString("MM/dd/yyyy") ?? "";
                    if (testDate != currentDate)
                    {
                        column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium).Row(row =>
                        {
                            row.RelativeItem().Text(testDate).Style(HeaderStyle);
                        });
                        currentDate = testDate;
                    }

                    column.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Row(row =>
                    {
                        row.RelativeItem(2).Text(testDate).Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(test.TestType ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(3).Text(test.Equipment ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(test.Location ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(test.Status ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(test.CompletedDate?.ToString("MM/dd/yyyy") ?? "").Style(DefaultTextStyle);
                    });
                }
            }

            // Summary section
            if (_tests != null && _tests.Any())
            {
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Summary by Test Type").Style(HeaderStyle);
                        
                        var summary = _tests
                            .GroupBy(t => t.TestType)
                            .Select(g => new { Type = g.Key ?? "Unknown", Count = g.Count() });

                        foreach (var item in summary)
                        {
                            c.Item().Text($"{item.Type}: {item.Count} tests").Style(DefaultTextStyle);
                        }
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Summary by Status").Style(HeaderStyle);
                        
                        var summary = _tests
                            .GroupBy(t => t.Status)
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
                text.Span($"Total Tests: {_tests?.Count ?? 0}").Style(DefaultTextStyle);
            });
        });
    }
} 