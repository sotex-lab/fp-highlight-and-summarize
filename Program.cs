using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using fp_highlights;
using CsvHelper;
using System.Globalization;
using Newtonsoft.Json.Linq;


var builder = new ServiceCollection();

Uri azureOpenAIResourceUri = new("https://pico-gpt4.openai.azure.com/");
AzureKeyCredential azureOpenAIApiKey = new(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));

builder.AddSingleton(new OpenAIClient(azureOpenAIResourceUri, azureOpenAIApiKey));
builder.AddSingleton<IDataProvider, DataProvider>();
builder.AddSingleton<IFpHighlightsService, FpHighlightsService>();


var container = builder.BuildServiceProvider();
var scope = container.CreateScope();

var dependency = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var _dataProvider = scope.ServiceProvider.GetRequiredService<IDataProvider>();
var _fpHighlightsService = scope.ServiceProvider.GetRequiredService<IFpHighlightsService>();
var _openAIClient = scope.ServiceProvider.GetRequiredService<OpenAIClient>();

// See https://aka.ms/new-console-template for more information
List<int> articleIds = [];
List<string> questions = [];
List<string> answers = [];
List<string> highlightedPdfs = [];

string fileName = "138103_20240115.csv";
string inputFilePath = Path.Combine(_dataProvider.GetInputFolderPath(), fileName);

using var reader = new StreamReader(inputFilePath);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var records = csv.GetRecords<dynamic>();

foreach (var row in records)
{
    JObject embeddingJsonObject;
    byte[] pdfBytes;
    try
    {
        var url = row.url;
        var pdf_url = row.pdf_url;

        string embeddingJson;
        using (HttpClient httpClient = new())
        {
            embeddingJson = httpClient.GetStringAsync(url).Result;
        }

        embeddingJsonObject = JObject.Parse(embeddingJson);

        using (HttpClient httpClient = new())
        {
            pdfBytes = httpClient.GetByteArrayAsync(pdf_url).Result;
        }
    }
    catch (Exception)
    {
        continue;
    }

    List<string> texts = [];
    List<float[]> embeddings = [];
    List<int> tokenSizes = [];
    //List<string> boundaryBoxes = [];

    for (int i = 0; i < embeddingJsonObject["article"]["text"].ToArray().Length; i++)
    {
        string text = embeddingJsonObject["article"]["text"][i].ToString();
        string embedding = embeddingJsonObject["article"]["embedding"][i].ToString();
        int tokenSize = Convert.ToInt32(embeddingJsonObject["article"]["token_size"][i].ToString());
        //string boundaryBox = embeddingJsonObject["article"]["boundary_boxes"][i].ToString();

        if (tokenSize > _dataProvider.GetMinimumTokens())
        {
            texts.Add(text);
            embeddings.Add(_fpHighlightsService.DecodeEmbedding(embedding));
            tokenSizes.Add(tokenSize);
            //boundaryBoxes.Add(boundaryBox);
        }
    }

    foreach(var q in _dataProvider.GetQuestionList())
    {
        var embeddedQuestion = _fpHighlightsService.EmbedText(q, _openAIClient);
        var scoreIndex = _fpHighlightsService.RankSentencesBySimilarity(embeddedQuestion, embeddings, tokenSizes, _fpHighlightsService.CosineSimilarity, _dataProvider.GetTotalMinimumTokens());
        
        List<string> bestSentences = [];
        //List<string> bestBoundaryBoxes = [];
        foreach (int idx in scoreIndex)
        {
            bestSentences.Add(texts[idx]);

            //List<string> boxes = JsonConvert.DeserializeObject<List<string>>(boundaryBoxes[idx]);
            //bestBoundaryBoxes.AddRange(boxes);
        }

        string bestSentencesString = string.Join(" ", bestSentences);
        string prompt = $"Data: {bestSentencesString}\nQuestion: {q}";
        string answer = await _fpHighlightsService.GetResponse(prompt, _openAIClient);

        string localPdfFile = Path.Combine(Path.GetTempPath(), $"{row.article_id}.pdf");
        Console.WriteLine($">>>>>>>{localPdfFile}");

        //fpHighlightsService.HighlightPdf(pdfBytes, bestBoundaryBoxes, localPdfFile);

        //string highlightedPdfUrl = fpHighlightsService.UploadPdf(s3Client, dataProvider.GetLongtermBucket(), localPdfFile, row.project_id, row.article_id);

        File.Delete(localPdfFile);

        articleIds.Add(row.article_id);
        questions.Add(q);
        answers.Add(answer);
        //highlightedPdfs.Add(highlightedPdfUrl);
    }
}

List<string> OUTPUT_COLUMNS = ["article_id", "questions", "answers", "highlighted_pdfs"];
List<List<object>> result = [];

for (int i = 0; i < articleIds.Count; i++)
{
    List<object> row =
    [
        articleIds[i],
        questions[i],
        answers[i],
        highlightedPdfs[i]
    ];

    result.Add(row);
}


