using LockerApp.Data.LockerDB;
using LockerApp.Models.LockerDB;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LockerApp.Reports;

public class JobListDocument : BaseDocument
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly string? _jobType;
    private List<JobList>? _jobs;

    public JobListDocument(
        IServiceProvider serviceProvider,
        DateTime startDate,
        DateTime endDate,
        string? jobType = null) : base(serviceProvider)
    {
        _startDate = startDate;
        _endDate = endDate;
        _jobType = jobType;
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
                column.Item().Text("Job List Report")
                    .Style(TitleStyle);

                column.Item().Text($"Period: {_startDate:MM/dd/yyyy} - {_endDate:MM/dd/yyyy}")
                    .Style(SubtitleStyle);

                if (!string.IsNullOrEmpty(_jobType))
                {
                    column.Item().Text($"Job Type: {_jobType}")
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
        if (_jobs != null) return;

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();

        var query = dbContext.JobLists
            .Where(j => j.Date >= _startDate && j.Date <= _endDate);

        if (!string.IsNullOrEmpty(_jobType))
        {
            query = query.Where(j => j.JobType == _jobType);
        }

        _jobs = await query
            .OrderBy(j => j.Date)
            .ThenBy(j => j.JobNumber)
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
                row.RelativeItem(2).Text("Job #").Style(HeaderStyle);
                row.RelativeItem(2).Text("Type").Style(HeaderStyle);
                row.RelativeItem(3).Text("Description").Style(HeaderStyle);
                row.RelativeItem(2).Text("Status").Style(HeaderStyle);
                row.RelativeItem(2).Text("Completed").Style(HeaderStyle);
            });

            // Table content
            if (_jobs != null)
            {
                foreach (var job in _jobs)
                {
                    column.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Row(row =>
                    {
                        row.RelativeItem(2).Text(job.Date?.ToString("MM/dd/yyyy") ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(job.JobNumber ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(job.JobType ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(3).Text(job.Description ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(job.Status ?? "").Style(DefaultTextStyle);
                        row.RelativeItem(2).Text(job.CompletedDate?.ToString("MM/dd/yyyy") ?? "").Style(DefaultTextStyle);
                    });
                }
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
                text.Span($"Total Jobs: {_jobs?.Count ?? 0}").Style(DefaultTextStyle);
            });
        });
    }
} 