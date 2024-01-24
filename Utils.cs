
using System.ComponentModel;
using Azure;
using Azure.AI.OpenAI;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

public static class Utils
{
    public static float[] DecodeEmbeddings(string embeddings)
    {
        var decodedEmbeddings = Convert.FromBase64String(embeddings);
        var decodedFloats = new float[decodedEmbeddings.Length / sizeof(float)];

        Buffer.BlockCopy(decodedEmbeddings, 0, decodedFloats, 0, 1536 * sizeof(float));

        return decodedFloats;
    }

    public static float[] EmbedText(string text, OpenAIClient _client)
    {
        EmbeddingsOptions embeddingOptions = new()
        {
            DeploymentName = "text-embedding-ada-002",
            Input = { text },
        };

        var returnValue = _client.GetEmbeddings(embeddingOptions);

        return returnValue.Value.Data[0].Embedding.ToArray();
    }

    public static float CosineSimilarity(float[] a, float[] b)
    {
        float dotProduct = a.Zip(b, (x, y) => x * y).Sum();

        var normA = a.Select(x => Math.Pow(x, 2)).Sum();
        normA = Math.Pow(normA, 2);
        var normB = a.Select(x => Math.Pow(x, 2)).Sum();
        normB = Math.Pow(normB, 2);

        float norms = (float)(normA * normB);

        if (norms == 0)
        {
            System.Console.WriteLine("Zero vector detected in cosine similarity computation.");
            return 0;
        }

        return dotProduct / norms;
    }

    public static List<int> RankSentencesBySimilarity(float[] embeddedQuestion, List<float[]> embeddings, List<int> tokenSizes, int totalMinimumTokens)
    {
        var scores = embeddings.Select((x, i) => new Tuple<int, float>(i, CosineSimilarity(embeddedQuestion, x))).OrderByDescending(x => x.Item2).ToList();
        var idx = 0;
        var totalTokenSize = 0;
        var scoreIndex = new List<int>();

        while (totalTokenSize < totalMinimumTokens && idx < scores.Count())
        {
            var element = scores[idx++];
            scoreIndex.Add(element.Item1);
            totalTokenSize += tokenSizes[element.Item1];
        }

        return scoreIndex;
    }

    public static async Task<string> GptResponse(string prompt, OpenAIClient _client)
    {
        string systemMessage = "You answer questions using the available information given to you";

        ChatCompletionsOptions chatCompletionsOptions = new()
        {
            DeploymentName = "gpt-3.5-turbo",
            Messages =
                {
                    new ChatRequestSystemMessage(systemMessage),
                    new ChatRequestUserMessage(prompt)
                },
            MaxTokens = 750,
            Temperature = 0.7f
        };

        Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
        ChatResponseMessage responseMessage = response.Value.Choices[0].Message;

        return responseMessage.Content;
    }

    public static void HighlightPdf(Stream pdf, List<float[]> bestBoundaryBoxes, string pdfOutputPath) {
        using PdfDocument document = PdfReader.Open(pdf, PdfDocumentOpenMode.Modify);
            foreach (var box in bestBoundaryBoxes)
            {
                int pageIndex = (int)box[0];
                float x1 = box[1];
                float y1 = box[2];
                float x2 = box[3];
                float y2 = box[4];

                PdfPage page = document.Pages[pageIndex];

                using XGraphics gfx = XGraphics.FromPdfPage(page);
                XRect rect = new(x1 + page.BleedBox.X1, y1 - page.BleedBox.Y1, x2 - x1, y2 - y1);

                gfx.DrawRectangle(XBrushes.LightGreen, rect);
                gfx.DrawString(" ", new XFont("Arial", 1), XBrushes.LightGreen, rect, XStringFormats.TopLeft);
            }

            document.Save(pdfOutputPath);
    }
}