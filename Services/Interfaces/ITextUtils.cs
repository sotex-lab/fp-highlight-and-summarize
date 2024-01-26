using System.Collections.Generic;
using System.Threading.Tasks;

namespace FpHighlights.Services.Interfaces
{
    public interface ITextUtils
    {
        float[] DecodeEmbeddings(string embeddings);
        float[] EmbedText(string text);
        float CosineSimilarity(float[] a, float[] b);
        List<int> RankSentencesBySimilarity(float[] embeddedQuestion, List<float[]> embeddings, List<int> tokenSizes, int totalMinimumTokens);
        Task<string> GptResponse(string prompt);
    }
}
