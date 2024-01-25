using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights_new.Services.Interfaces
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
