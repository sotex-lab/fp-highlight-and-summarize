using Newtonsoft.Json;

internal class Article {
    [JsonProperty("text")]
    public List<string> Text { get; set; }
    [JsonProperty("token_size")]
    public List<int> TokenSizes { get; set; }
    [JsonProperty("embedding")]
    public List<string> Embedding { get; set; }
    [JsonProperty("boundary_boxes")]
    public List<string> BoundaryBoxes { get; set; }
}

internal class WholeObject {

    [JsonProperty("article")]
    public Article Article { get; set; }
}