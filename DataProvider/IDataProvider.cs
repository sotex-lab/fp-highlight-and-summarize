namespace FpHighlights.ProviderData
{
    public interface IDataProvider
    {
        string GetInputFolderPath();
        string GetOutputFolderPath();
        int GetTotalMinimumTokens();
        int GetMinimumTokens();
    }
}