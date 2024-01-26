using FpHighlights.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FpHighlights.Services.Interfaces
{
    public interface IFpHighlightAndSummarizeService
    {
        Task<IEnumerable<Response>> HighlightPdf(string fileName, List<string> questionList, int articleCount = 0);
    }
}
