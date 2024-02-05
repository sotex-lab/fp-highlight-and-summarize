using FpHighlights.Services.Interfaces;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FpHighlights.Services
{
    internal class PdfUtils : IPdfUtils
    {
        public void HighlightPdf(string pdfUrl, List<float[]> bestBoundaryBoxes, string pdfOutputPath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var client = new WebClient();
            byte[] pdfBytes = client.DownloadData(pdfUrl);
            using var pdfStream = new MemoryStream(pdfBytes);
            var document = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);

            using PdfDocument outputDocument = new PdfDocument();
            for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
            {
                PdfPage originalPage = document.Pages[pageIndex];
                PdfPage newPage = outputDocument.AddPage(originalPage);

                using XGraphics gfx = XGraphics.FromPdfPage(newPage);
                var rectangles = bestBoundaryBoxes.Where(x => x[0] == pageIndex);

                foreach (var rect in rectangles)
                {
                    float x1 = rect[1];
                    float y1 = rect[2];
                    float x2 = rect[3];
                    float y2 = rect[4];

                    var highlightRect = new XRect(x1, y1, x2 - x1, y2 - y1);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(50, XColors.Lime)), highlightRect);
                }
            }
            outputDocument.Save(pdfOutputPath);
        }
    }
}
