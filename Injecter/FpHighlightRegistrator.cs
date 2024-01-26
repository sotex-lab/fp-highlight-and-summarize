using Azure.AI.OpenAI;
using Azure;
using FpHighlights.ProviderData;
using FpHighlights.Services.Interfaces;
using FpHighlights.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FpHighlights.Injecter
{
    public static class FpHighlightRegistrator
    {
        public static IServiceCollection AddFpHighlightService(this IServiceCollection services, Func<FpHighlightConfig> func)
        {
            var config = func();

            services.AddSingleton(new OpenAIClient(config.OpenAIConfig.Uri, config.OpenAIConfig.Credentials));
            services.AddSingleton<IDataProvider, DataProvider>(_ => new DataProvider(config.DataProviderConfig.InputFolderPath, config.DataProviderConfig.OutputFolderPath, config.DataProviderConfig.TotalMinimumTokens, config.DataProviderConfig.MinimumTokens));
            services.AddTransient<ITextUtils, TextUtils>();
            services.AddTransient<IPdfUtils, PdfUtils>();
            services.AddTransient<IFpHighlightAndSummarizeService, FpHighlightAndSummarizeService>();

            return services;
        }
    }

    public class FpHighlightConfig
    {
        private OpenAIConfig openAIConfig;
        private DataProviderConfig dataProviderConfig;
        public OpenAIConfig OpenAIConfig
        {
            get => openAIConfig;
            set
            {
                openAIConfig = value ?? throw new ArgumentNullException("Invalid OpenAIConfig");
            }
        }
        public DataProviderConfig DataProviderConfig
        {
            get => dataProviderConfig;
            set
            {
                dataProviderConfig = value ?? throw new ArgumentNullException("Invalid DataProviderConfig");
            }
        }
    }

    public class OpenAIConfig
    {
        private Uri uri;
        private AzureKeyCredential credentials;
        public Uri Uri
        {
            get => uri;
            set
            {
                uri = value ?? throw new ArgumentNullException("Invalid uri");
            }
        }
        public AzureKeyCredential Credentials
        {
            get => credentials;
            set
            {
                credentials = value ?? throw new ArgumentException("Invalid credentials");
            }
        }
    }

    public class DataProviderConfig
    {
        private string inputFolderPath;
        private string outputFolderPath;

        public string InputFolderPath
        {
            get => inputFolderPath;
            set
            {
                inputFolderPath = value ?? throw new ArgumentException("Invalid input folder path");
            }
        }
        public string OutputFolderPath
        {
            get => outputFolderPath;
            set
            {
                outputFolderPath = value ?? throw new ArgumentException("Invalid output folder path");
            }
        }
        public int TotalMinimumTokens { get; set; }
        public int MinimumTokens { get; set; }
    }
}
