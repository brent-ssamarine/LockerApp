using AccessMigrationApp.Reports;
using QuestPDF.Fluent;

namespace AccessMigrationApp.Services;

public interface IReportService
{
    Task<byte[]> GenerateGearListPdf(int locationId, string locationName, string berth, int finished);
    Task<byte[]> GenerateRecapPdf(int? locationId = null);
    Task<byte[]> GenerateMaterialListPdf(int? locationId);
    // Task<byte[]> GenerateJobListPdf(DateTime startDate, DateTime endDate, string? jobType = null);
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

    public async Task<byte[]> GenerateRecapPdf(int? locationId = null)
    {
        var document = new RecapDocument(_serviceProvider, locationId);
        await document.PrepareAsync();
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMaterialListPdf(int? locationId)
    {
        var document = new AccessMigrationApp.Reports.MaterialListDocument(_serviceProvider, locationId);
        await document.PrepareAsync();
        return document.GeneratePdf();
    }
}