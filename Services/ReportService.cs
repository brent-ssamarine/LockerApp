using AccessMigrationApp.Reports;
using QuestPDF.Fluent;

namespace AccessMigrationApp.Services;

public interface IReportService
{
    Task<byte[]> GenerateGearListPdf(int locationId, string locationName, string berth, int finished);
    Task<byte[]> GenerateRecapPdf(DateTime startDate, DateTime endDate, string inspectedBy);
    // Task<byte[]> GenerateJobListPdf(DateTime startDate, DateTime endDate, string? jobType = null);
    // Task<byte[]> GenerateMaterialListPdf(DateTime startDate, DateTime endDate, string? category = null);
    // Task<byte[]> GenerateRecapInspectedBySubPdf(DateTime startDate, DateTime endDate, string subInspector);
    // Task<byte[]> GenerateTestSchedulerPdf(DateTime startDate, DateTime endDate, string? testType = null);
}

public class ReportService : IReportService
{
    private readonly IServiceProvider _serviceProvider;

    public ReportService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<byte[]> GenerateGearListPdf(int locationId, string locationName, string berth, int finished)
    {
        var document = new GearListDocument(_serviceProvider, locationId, locationName, berth, finished);
        await document.PrepareAsync();
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateRecapPdf(DateTime startDate, DateTime endDate, string inspectedBy)
    {
        var document = new RecapDocument(_serviceProvider, startDate, endDate, inspectedBy);
        await document.PrepareAsync();
        return document.GeneratePdf();
    }

    /*
    public async Task<byte[]> GenerateJobListPdf(DateTime startDate, DateTime endDate, string? jobType = null)
    {
        var document = new JobListDocument(_serviceProvider, startDate, endDate, jobType);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMaterialListPdf(DateTime startDate, DateTime endDate, string? category = null)
    {
        var document = new MaterialListDocument(_serviceProvider, startDate, endDate, category);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateRecapPdf(DateTime startDate, DateTime endDate, string inspectedBy)
    {
        var document = new RecapDocument(_serviceProvider, startDate, endDate, inspectedBy);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateRecapInspectedBySubPdf(DateTime startDate, DateTime endDate, string subInspector)
    {
        var document = new RecapInspectedBySubDocument(_serviceProvider, startDate, endDate, subInspector);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateTestSchedulerPdf(DateTime startDate, DateTime endDate, string? testType = null)
    {
        var document = new TestSchedulerDocument(_serviceProvider, startDate, endDate, testType);
        return document.GeneratePdf();
    }
    */
} 