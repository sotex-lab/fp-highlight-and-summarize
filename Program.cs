using Azure;
using FpHighlights.Injecter;
using FpHighlights.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FpHighlights 
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await MainAsync();
        }

        static async Task MainAsync()
        {
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

            builder.AddLogging();

            var container = builder.BuildServiceProvider();
            var scope = container.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<IFpHighlightAndSummarizeService>();

            List<string> questionList = new List<string> {
            "How do various socioeconomic, racial, and geographical factors influence pregnancy outcomes and maternal health across different populations in the United States?",
            "Was the allocation sequence random?",
        };

            await service.HighlightPdf("138103_20240115.csv", questionList);
        }
    }
}