using FpHighlights.Services.Interfaces;
using iTextSharp.awt.geom;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace FpHighlights.Services
{
    internal class PdfUtils : IPdfUtils
    {
        public void HighlightPdf(string pdfUrl, List<float[]> bestBoundaryBoxes, string pdfOutputPath)
        {
            var reader = new PdfReader(new Uri(pdfUrl));

            using (var fileStream = new FileStream(pdfOutputPath, FileMode.Create, FileAccess.Write))
            {
                var document = new Document(reader.GetPageSizeWithRotation(1));
                var writer = PdfWriter.GetInstance(document, fileStream);

                document.Open();

                for (var i = 1; i <= reader.NumberOfPages; i++)
                {
                    document.NewPage();
                    var page = writer.GetImportedPage(reader, i);

                    var contentByte = writer.DirectContent;
                    contentByte.AddTemplate(page, new AffineTransform());
                    contentByte.SetColorFill(BaseColor.GREEN);

                    var rectangles = bestBoundaryBoxes.Where(x => x[0] == i - 1);

                    foreach (var rect in rectangles)
                    {
                        var x1 = rect[1];
                        var y1 = page.Height - rect[2];
                        var x2 = rect[3];
                        var y2 = page.Height - rect[4];

                        contentByte.Rectangle(x1, y1, x2 - x1, y2 - y1);
                        var state = new PdfGState();
                        state.FillOpacity = 0.2f;
                        contentByte.SetGState(state);
                        contentByte.Fill();
                    }
                }

                document.Close();
                writer.Close();
            }
        }
    }
}
