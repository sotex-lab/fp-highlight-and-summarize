
using Azure;
using Azure.AI.OpenAI;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Drawing;

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

    public static void HighlightPdf(string pdfUrl, List<float[]> bestBoundaryBoxes, string pdfOutputPath)
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

                foreach(var rect in rectangles){
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