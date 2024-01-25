namespace FpHighlights.ProviderData
{
    internal class DataProvider : IDataProvider
    {
        private readonly string InputFolderPath;
        private readonly string OutputFolderPath;
        private readonly int TotalMinimumTokens;
        private readonly int MinimumTokens;

        public DataProvider(string inputFolderPath, string outputFolderPath, int totalMinimumTokens, int minimumTokens)
        {
            InputFolderPath = inputFolderPath;
            OutputFolderPath = outputFolderPath;
            TotalMinimumTokens = totalMinimumTokens;
            MinimumTokens = minimumTokens;
        }

        public string GetInputFolderPath() => InputFolderPath;
        public string GetOutputFolderPath() => OutputFolderPath;
        public int GetTotalMinimumTokens() => TotalMinimumTokens;
        public int GetMinimumTokens() => MinimumTokens;
    }
}
