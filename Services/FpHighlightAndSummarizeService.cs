using CsvHelper;
using FpHighlights.ProviderData;
using FpHighlights.Services.Interfaces;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using FpHighlights.DTOs;
using CsvHelper.Expressions;

namespace FpHighlights.Services
{
    public class FpHighlightAndSummarizeService : IFpHighlightAndSummarizeService
    {
        private readonly IDataProvider _dataProvider;
        private readonly ITextUtils _textUtils;
        private readonly IPdfUtils _pdfUtils;
        private readonly ILogger<FpHighlightAndSummarizeService> _logger;

        public FpHighlightAndSummarizeService(IDataProvider dataProvider, ITextUtils textUtils, IPdfUtils pdfUtils, ILogger<FpHighlightAndSummarizeService> logger)
        {
            _dataProvider = dataProvider;
            _textUtils = textUtils;
            _pdfUtils = pdfUtils;
            _logger = logger;
        }

        public async Task<IEnumerable<Response>> HighlightPdf(string fileName, List<string> questionList, int articleCount = 0)
        {
            var inputFileName = _dataProvider.GetInputFolderPath() + fileName;
            int tempArticleCount = 0;
            var result = new List<Response>();

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
                        _logger.LogDebug("Not successful fetch for url: " + row.Url);
                        continue;
                    }

                    parsedJson = JsonConvert.DeserializeObject<WholeObject>(await response.Content.ReadAsStringAsync());
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e.Message);
                    continue;
                }


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

                var currentResponse = new Response() { ArticleId = row.ArticleId, Responses = new List<string>() };

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
                    var answer = await _textUtils.GptResponse(prompt);
                    currentResponse.Responses.Add(answer);

                    var localPdfPath = Path.Combine(_dataProvider.GetOutputFolderPath(), row.ArticleId + ".pdf");
                    _pdfUtils.HighlightPdf(row.PdfUrl, bestBoundaryBoxes, localPdfPath);
                }
                result.Add(currentResponse);
            }
            return result;
        }
    }
}
