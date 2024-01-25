using System.Globalization;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using CsvHelper;
using fp_highlights_new.DataProvider;
using fp_highlights_new.Injecter;
using fp_highlights_new.Services;
using fp_highlights_new.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var builder = new ServiceCollection();

builder.AddFpHighlightService(() => new FpHighlightConfig
{
    OpenAIConfig = new OpenAIConfig
    {
        Uri = new Uri("https://pico-gpt4.openai.azure.com/"),
        Credentials = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!)
    },
    DataProviderConfig = new DataProviderConfig
    {
        InputFolderPath = Directory.GetCurrentDirectory() + "/",
        OutputFolderPath = Directory.GetCurrentDirectory(),
        TotalMinimumTokens = 500,
        MinimumTokens = 30
    }
});
var container = builder.BuildServiceProvider();
var scope = container.CreateScope();

var service = scope.ServiceProvider.GetRequiredService<IFpHighlightAndSummarizeService>();

List<string> QUESTION_LIST = new List<string> {
            "How do various socioeconomic, racial, and geographical factors influence pregnancy outcomes and maternal health across different populations in the United States?",
            "Was the allocation sequence random?",
        };

await service.HighlightPdf("138103_20240115.csv", QUESTION_LIST);