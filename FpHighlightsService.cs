using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Azure;
using Azure.AI.OpenAI;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights
{
    public class FpHighlightsService : IFpHighlightsService
    {
        public float[] DecodeEmbedding(string embedding)
        {
            byte[] decodedBytes = Convert.FromBase64String(embedding);
            float[] decodedEmbedding = new float[decodedBytes.Length / sizeof(float)];

            using (MemoryStream stream = new MemoryStream(decodedBytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                for (int i = 0; i < decodedEmbedding.Length; i++)
                {
                    decodedEmbedding[i] = reader.ReadSingle();
                }
            }

            return decodedEmbedding;
        }

        public float[] EmbedText(string text, OpenAIClient client)
        {
            EmbeddingsOptions embeddingOptions = new()
            {
                DeploymentName = "text-embedding-ada-002",
                Input = { text },
            };

            var returnValue = client.GetEmbeddings(embeddingOptions);

            return returnValue.Value.Data[0].Embedding.ToArray();
        }

        public float CosineSimilarity(float[] vec1, float[] vec2)
        {
            float dotProduct = vec1.Zip(vec2, (a, b) => a * b).Sum();

            float norm1 = (float)Math.Sqrt(vec1.Select(a => a * a).Sum());
            float norm2 = (float)Math.Sqrt(vec2.Select(b => b * b).Sum());

            float norms = norm1 * norm2;

            if (norms == 0)
            {
                return 0.0f;
            }

            return dotProduct / norms;
        }

        public List<int> RankSentencesBySimilarity(float[] embeddedQuestion, List<float[]> embeddings, List<int> tokenSizes, Func<float[], float[], float> cosineSimilarity, int TOTAL_MINIMUM_TOKENS)
        {
            var scores = embeddings
            .Select((emb, idx) => (idx, cosineSimilarity(embeddedQuestion, emb)))
            .ToList();

            var sortedScores = scores.OrderByDescending(x => x.Item2).ToList();

            var idx = 0;
            var totalTokenSize = 0;
            var scoreIndex = new List<int>();

            while (totalTokenSize < TOTAL_MINIMUM_TOKENS && idx < sortedScores.Count)
            {
                scoreIndex.Add(sortedScores[idx].Item1);
                totalTokenSize += tokenSizes[sortedScores[idx].Item1];
                idx++;
            }

            return scoreIndex;
        }

        public async Task<string> GetResponse(string prompt, OpenAIClient client)
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

            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;

            return responseMessage.Content;
        }

        public void HighlightPdf(string pdfFilePath, float[][] bestBoundaryBoxes, string pdfOutputPath)
        {
            using PdfDocument document = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Modify);
            for (int i = 0; i < bestBoundaryBoxes.Length; i++)
            {
                float[] box = bestBoundaryBoxes[i];
                int pageIndex = (int)box[0];
                float x1 = box[1];
                float y1 = box[2];
                float x2 = box[3];
                float y2 = box[4];

                PdfPage page = document.Pages[pageIndex];

                using XGraphics gfx = XGraphics.FromPdfPage(page);
                XRect rect = new(x1, page.Height - y2, x2 - x1, y2 - y1);

                gfx.DrawRectangle(XBrushes.LightGreen, rect);
                gfx.DrawString(" ", new XFont("Arial", 1), XBrushes.LightGreen, rect, XStringFormats.TopLeft);
            }

            document.Save(pdfOutputPath);
        }

        public string UploadPdf(AmazonS3Client s3Client, string LONGTERM_BUCKET, string filePath, string projectId, string articleId)
        {
            var currentDateTime = DateTime.Now.ToString("dd-MM-yy_HH:mm:ss:fff");
            var s3FilePath = $"experimental_pdfs/{projectId}/{articleId}_{currentDateTime}.pdf";
            try
            {
                TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                fileTransferUtility.Upload(filePath, LONGTERM_BUCKET, s3FilePath);
                //logger
            }
            catch (AmazonS3Exception e)
            {
                //logger
            }
            catch (Exception e)
            {
                //logger
            }

            var request = new GetPreSignedUrlRequest
            {
                BucketName = LONGTERM_BUCKET,
                Key = s3FilePath,
                Expires = DateTime.Now.AddSeconds(86400),
                Protocol = Protocol.HTTPS
            };

            string url = s3Client.GetPreSignedURL(request);
            return url;
        }
    }

    public interface IFpHighlightsService
    {
        float[] DecodeEmbedding(string embedding);
        float[] EmbedText(string text, OpenAIClient client);
        float CosineSimilarity(float[] vec1, float[] vec2);
        List<int> RankSentencesBySimilarity(float[] embeddedQuestion,
            List<float[]> embeddings, 
            List<int> tokenSizes, 
            Func<float[], float[], float> cosineSimilarity, 
            int TOTAL_MINIMUM_TOKENS);
        Task<string> GetResponse(string prompt, OpenAIClient client);
        void HighlightPdf(string pdfFilePath, float[][] bestBoundaryBoxes, string pdfOutputPath);
        string UploadPdf(AmazonS3Client s3Client, string LONGTERM_BUCKET, string filePath, string projectId, string articleId);
    }
}

