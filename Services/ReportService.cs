using AccessMigrationApp.Reports;
using QuestPDF.Fluent;

namespace AccessMigrationApp.Services;

public interface IReportService
{
    Task<byte[]> GenerateGearListPdf(int locationId, string locationName, string berth, int finished);
    Task<byte[]> GenerateRecapPdf(DateTime startDate, DateTime endDate, string inspectedBy, int? locationId = null);
    Task<byte[]> GenerateMaterialListPdf(int? locationId, string locationName, string berth, DateTime startDate);
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

    public async Task<byte[]> GenerateRecapPdf(DateTime startDate, DateTime endDate, string inspectedBy, int? locationId = null)
    {
        var document = new RecapDocument(_serviceProvider, startDate, endDate, inspectedBy, locationId);
        await document.PrepareAsync();
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMaterialListPdf(int? locationId, string locationName, string berth, DateTime startDate)
    {
        var document = new AccessMigrationApp.Reports.MaterialListDocument(_serviceProvider, locationId, locationName, berth, startDate);
        await document.PrepareAsync();
        return document.GeneratePdf();
    }
}