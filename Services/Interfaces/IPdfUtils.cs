using PdfSharp.Pdf;
using System.Collections.Generic;

namespace FpHighlights.Services.Interfaces
{
    public interface IPdfUtils
    {
        PdfDocument? OpenPdf(string pdfUrl);
        void HighlightPdf(PdfDocument document, List<float[]> bestBoundaryBoxes);
    }
}
