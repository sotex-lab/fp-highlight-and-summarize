using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights_new.Services.Interfaces
{
    public interface IFpHighlightAndSummarizeService
    {
        Task HighlightPdf(string fileName, List<string> questionList, int articleCount = 0);
    }
}
