using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights_new.DataProvider
{
    public class DataProvider : IDataProvider
    {
        private readonly string INPUT_FOLDER_PATH;
        private readonly string OUTPUT_FOLDER_PATH;
        private readonly int TOTAL_MINIMUM_TOKENS;
        private readonly int MINIMUM_TOKENS;

        public DataProvider(string inputFolderPath, string outputFolderPath, int totalMinimumTokens, int minimumTokens)
        {
            INPUT_FOLDER_PATH = inputFolderPath;
            OUTPUT_FOLDER_PATH = outputFolderPath;
            TOTAL_MINIMUM_TOKENS = totalMinimumTokens;
            MINIMUM_TOKENS = minimumTokens;
        }

        public string GetInputFolderPath() => INPUT_FOLDER_PATH;
        public string GetOutputFolderPath() => OUTPUT_FOLDER_PATH;
        public int GetTotalMinimumTokens() => TOTAL_MINIMUM_TOKENS;
        public int GetMinimumTokens() => MINIMUM_TOKENS;
    }
}
