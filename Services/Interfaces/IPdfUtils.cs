using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights_new.Services.Interfaces
{
    public interface IPdfUtils
    {
        void HighlightPdf(string pdfUrl, List<float[]> bestBoundaryBoxes, string pdfOutputPath);
    }
}
