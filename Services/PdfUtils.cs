using FpHighlights.Services.Interfaces;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FpHighlights.Services
{
    internal class PdfUtils : IPdfUtils
    {
        private readonly ILogger<PdfUtils> _logger;

        public PdfUtils(ILogger<PdfUtils> logger)
        {
            _logger = logger;
        }

        public PdfDocument? OpenPdf(string pdfUrl)
        {
            using var client = new WebClient();
            var pdfBytes = client.DownloadData(pdfUrl);
            using var pdfStream = new MemoryStream(pdfBytes);
            try
            {
                PdfDocument document = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Modify);
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Pdf file {pdfUrl} cannot be processed: {ex}");
                return null;
            }
        }

        public void HighlightPdf(PdfDocument document, List<float[]> bestBoundaryBoxes)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
            {
                PdfPage currentPage = document.Pages[pageIndex];

                using XGraphics gfx = XGraphics.FromPdfPage(currentPage);
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
        }

    }
}
