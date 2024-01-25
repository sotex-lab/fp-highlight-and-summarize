using FpHighlights.DTOs;

namespace FpHighlights.Services.Interfaces
{
    public interface IFpHighlightAndSummarizeService
    {
        Task<IEnumerable<Response>> HighlightPdf(string fileName, List<string> questionList, int articleCount = 0);
    }
}
