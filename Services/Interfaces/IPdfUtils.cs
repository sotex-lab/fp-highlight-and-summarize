namespace FpHighlights.Services.Interfaces
{
    public interface IPdfUtils
    {
        void HighlightPdf(string pdfUrl, List<float[]> bestBoundaryBoxes, string pdfOutputPath);
    }
}
