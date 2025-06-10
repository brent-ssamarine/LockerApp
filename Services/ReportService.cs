using AccessMigrationApp.Reports;
using QuestPDF.Fluent;

namespace AccessMigrationApp.Services;

public interface IReportService
{
    Task<byte[]> GenerateGearListPdf(int locationId, string locationName, string berth, int finished);
    Task<byte[]> GenerateRecapPdf(DateTime startDate, DateTime endDate, string inspectedBy);
    // Task<byte[]> GenerateMaterialsListPdf(int locationId, string locationName, string berth, DateTime startDate, DateTime endDate);
    // Task<byte[]> GenerateJobListPdf(DateTime startDate, DateTime endDate, string? jobType = null);
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
} 