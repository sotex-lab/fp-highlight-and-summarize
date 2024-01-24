using Amazon.S3;
using Azure;
using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights
{
    public class DataProvider : IDataProvider
    {
        private const string INPUT_FOLDER_PATH = "/drive/My Drive/Colab Notebooks/GPT_Full-text/CSV/";
        private const string OUTPUT_FOLDER_PATH = "/drive/My Drive/Colab Notebooks/GPT_Full-text/OUTPUT_PDF/";
        private const string LONGTERM_BUCKET = OUTPUT_FOLDER_PATH;
        private readonly List<string> QUESTION_LIST = ["Specify the primary focus or objectives of the review. What are the key research questions or goals that the review aims to address?",];
        private const int TOTAL_MINIMUM_TOKENS = 500;
        private const int MINIMUM_TOKENS = 30;

        public string GetInputFolderPath() => INPUT_FOLDER_PATH;
        public string GetOutputFolderPath() => OUTPUT_FOLDER_PATH;
        public string GetLongtermBucket() => LONGTERM_BUCKET;
        public List<string> GetQuestionList() => QUESTION_LIST;
        public int GetTotalMinimumTokens() => TOTAL_MINIMUM_TOKENS;
        public int GetMinimumTokens() => MINIMUM_TOKENS;
    }

    public interface IDataProvider
    {
        string GetInputFolderPath();
        string GetOutputFolderPath();
        string GetLongtermBucket();
        List<string> GetQuestionList();
        int GetTotalMinimumTokens();
        int GetMinimumTokens();
    }
}

