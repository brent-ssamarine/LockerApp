using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public abstract class BaseDocument : IDocument
{
    protected readonly IServiceProvider ServiceProvider;

    protected BaseDocument(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public abstract void Compose(IDocumentContainer container);

    public byte[] GeneratePdf()
    {
        return Document.Create(container => Compose(container)).GeneratePdf();
    }

    protected static TextStyle DefaultTextStyle => TextStyle
        .Default
        .FontSize(10)
        .FontFamily("Arial");

    protected static TextStyle HeaderStyle => TextStyle
        .Default
        .FontSize(12)
        .Bold()
        .FontFamily("Arial");

    protected static TextStyle TitleStyle => TextStyle
        .Default
        .FontSize(20)
        .Bold()
        .FontFamily("Arial");

    protected static TextStyle SubtitleStyle => TextStyle
        .Default
        .FontSize(14)
        .SemiBold()
        .FontFamily("Arial");
} 