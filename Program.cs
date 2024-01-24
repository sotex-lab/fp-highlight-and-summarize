using System.Globalization;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using CsvHelper;
using Newtonsoft.Json;

var INPUT_FOLDER_PATH = "./";
var OUTPUT_FOLDER_PATH = "./output";
var LONGTERM_BUCKET = OUTPUT_FOLDER_PATH;

var client = new OpenAIClient(new Uri("https://pico-gpt4.openai.azure.com/"), new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!));

var questionList = new List<string> {
    "Specify the primary focus or objectives of the review. What are the key research questions or goals that the review aims to address?"
};
var totalMinimumTokens = 500;
var minimumTokens = 30;

var articleId = new List<int>();
var questions = new List<string>();
var answers = new List<string>();
var HighlightPdf = new List<string>();

var fileName = "138103_20240115.csv";
var inputFileName = INPUT_FOLDER_PATH + fileName;


using var csvfile = new CsvReader(new StreamReader(inputFileName), CultureInfo.InvariantCulture);
csvfile.Context.RegisterClassMap<ProjectMap>();

foreach (var row in csvfile.GetRecords<Project>())
{
    var parsedJson = new WholeObject();
    try
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(row.Url);

        if (!response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("Not successful fetch for url: " + row.Url);
            continue;
        }

        var content = await response.Content.ReadAsStringAsync();
        parsedJson = JsonConvert.DeserializeObject<WholeObject>(await response.Content.ReadAsStringAsync());
    }
    catch (Exception e)
    {
        System.Console.WriteLine(e);
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
        if (article.TokenSizes[i] <= minimumTokens) continue;

        texts.Add(article.Text[i]);
        embeddings.Add(Utils.DecodeEmbeddings(article.Embedding[i]));
        tokenSizes.Add(article.TokenSizes[i]);
        boundaryBoxes.Add(JsonConvert.DeserializeObject<float[][]>(article.BoundaryBoxes[i])!);
    }

    foreach(var question in questionList) {
        var embeddedQuestion = Utils.EmbedText(question, client);
        var scoreIndex = Utils.RankSentencesBySimilarity(embeddedQuestion, embeddings, tokenSizes, totalMinimumTokens);
        var bestSentences = string.Join(" ", scoreIndex.Select(x => texts[x]));

        var bestBoundaryBoxes = new List<float[]>();
        foreach (var matrix in scoreIndex.Select(x => boundaryBoxes[x])) {
            bestBoundaryBoxes.AddRange(matrix);
        }

        var prompt = new StringBuilder().Append("Data: ").AppendLine(bestSentences).Append("Question :").Append(question).ToString();
        var answer = Utils.GptResponse(prompt, client);

        var localPdfPath = Path.Combine(OUTPUT_FOLDER_PATH, row.ArticleId + ".pdf");
        Utils.HighlightPdf(row.PdfUrl, bestBoundaryBoxes, localPdfPath);
    }

    return;
}