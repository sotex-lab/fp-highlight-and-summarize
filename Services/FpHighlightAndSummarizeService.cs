using Azure.AI.OpenAI;
using Azure;
using CsvHelper;
using fp_highlights_new.DataProvider;
using fp_highlights_new.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights_new.Services
{
    public class FpHighlightAndSummarizeService : IFpHighlightAndSummarizeService
    {
        private readonly IDataProvider _dataProvider;
        private readonly ITextUtils _textUtils;
        private readonly IPdfUtils _pdfUtils;

        public FpHighlightAndSummarizeService(IDataProvider dataProvider, ITextUtils textUtils, IPdfUtils pdfUtils)
        {
            _dataProvider = dataProvider;
            _textUtils = textUtils;
            _pdfUtils = pdfUtils;
        }

        public async Task HighlightPdf(string fileName, List<string> questionList, int articleCount = 0)
        {
            var articleId = new List<int>();
            var isIncludeAb = new List<bool>();
            var isIncludeFt = new List<bool>();
            var citeId = new List<string>();
            var questions = new List<string>();
            var answers = new List<string>();
            var HighlightPdf = new List<string>();

            var inputFileName = _dataProvider.GetInputFolderPath() + fileName;
            int tempArticleCount = 0;

            using var csvfile = new CsvReader(new StreamReader(inputFileName), CultureInfo.InvariantCulture);
            csvfile.Context.RegisterClassMap<ProjectMap>();

            foreach (var row in csvfile.GetRecords<Project>())
            {
                if (row.IsIncludeFt is false || row.IsFulltext is false || row.Url == null || row.Url == "")
                {
                    continue;
                }

                if (articleCount != 0)
                {
                    tempArticleCount++;
                    if (tempArticleCount > articleCount)
                    {
                        break;
                    }
                }

                var parsedJson = new WholeObject();
                try
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(row.Url);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Not successful fetch for url: " + row.Url);
                        continue;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    parsedJson = JsonConvert.DeserializeObject<WholeObject>(await response.Content.ReadAsStringAsync());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                // System.Console.WriteLine("Looking at the article\n" + JsonConvert.SerializeObject(parsedJson, Formatting.Indented));

                var article = parsedJson!.Article;

                var embeddings = new List<float[]>();
                var texts = new List<string>();
                var tokenSizes = new List<int>();
                var boundaryBoxes = new List<float[][]>();

                for (int i = 0; i < article!.Text.Count; i++)
                {
                    if (article.TokenSizes[i] <= _dataProvider.GetMinimumTokens()) continue;

                    texts.Add(article.Text[i]);
                    embeddings.Add(_textUtils.DecodeEmbeddings(article.Embedding[i]));
                    tokenSizes.Add(article.TokenSizes[i]);
                    boundaryBoxes.Add(JsonConvert.DeserializeObject<float[][]>(article.BoundaryBoxes[i])!);
                }

                foreach (var question in questionList)
                {
                    var embeddedQuestion = _textUtils.EmbedText(question);
                    var scoreIndex = _textUtils.RankSentencesBySimilarity(embeddedQuestion, embeddings, tokenSizes, _dataProvider.GetTotalMinimumTokens());
                    var bestSentences = string.Join(" ", scoreIndex.Select(x => texts[x]));

                    var bestBoundaryBoxes = new List<float[]>();
                    foreach (var matrix in scoreIndex.Select(x => boundaryBoxes[x]))
                    {
                        bestBoundaryBoxes.AddRange(matrix);
                    }

                    var prompt = new StringBuilder().Append("Data: ").AppendLine(bestSentences).Append("Question :").Append(question).ToString();
                    var answer = _textUtils.GptResponse(prompt);

                    var localPdfPath = Path.Combine(_dataProvider.GetOutputFolderPath(), row.ArticleId + ".pdf");
                    _pdfUtils.HighlightPdf(row.PdfUrl, bestBoundaryBoxes, localPdfPath);
                }

                return;
            }
        }
    }
}
