using Azure.AI.OpenAI;
using Azure;
using fp_highlights_new.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights_new.Services
{
    public class TextUtils : ITextUtils
    {
        private readonly OpenAIClient _openAIClient;

        public TextUtils(OpenAIClient openAIClient)
        {
            _openAIClient = openAIClient;
        }

        public float[] DecodeEmbeddings(string embeddings)
        {
            var decodedEmbeddings = Convert.FromBase64String(embeddings);
            var decodedFloats = new float[decodedEmbeddings.Length / sizeof(float)];

            Buffer.BlockCopy(decodedEmbeddings, 0, decodedFloats, 0, 1536 * sizeof(float));

            return decodedFloats;
        }

        public float[] EmbedText(string text)
        {
            EmbeddingsOptions embeddingOptions = new()
            {
                DeploymentName = "text-embedding-ada-002",
                Input = { text },
            };

            var returnValue = _openAIClient.GetEmbeddings(embeddingOptions);

            return returnValue.Value.Data[0].Embedding.ToArray();
        }

        public float CosineSimilarity(float[] a, float[] b)
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

        public List<int> RankSentencesBySimilarity(float[] embeddedQuestion, List<float[]> embeddings, List<int> tokenSizes, int totalMinimumTokens)
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

        public async Task<string> GptResponse(string prompt)
        {
            string systemMessage = "You are an intelligent and adaptive research assistant. Your primary task is to comprehend and respond to various types of questions. This involves analyzing and synthesizing information from the provided text and replying with a brief response. Get your information form the `Material and Methods` section and the `results` section.  Ingnore abstract and citations content.";

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

            Response<ChatCompletions> response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;

            return responseMessage.Content;
        }
    }
}
