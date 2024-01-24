using CsvHelper.Configuration;

public class Project {
    public int ProjectId { get; set; }
    public int ArticleId { get; set; }
    public string Url { get; set; }
    public string PdfUrl { get; set; }
    public string IsFulltext { get; set; }
    public string IsDuplicate { get; set; }
    public string IsIncluideAb { get; set; }
    public string Tags { get; set; }
    public string CiteId { get; set; }
}

public sealed class ProjectMap : ClassMap<Project> {
    public ProjectMap()
    {
        Map(x => x.ProjectId).Name("project_id");
        Map(x => x.ArticleId).Name("article_id");
        Map(x => x.Url).Name("url");   
        Map(x => x.PdfUrl).Name("pdf_url");
        Map(x => x.IsFulltext).Name("is_fulltext");
        Map(x => x.IsDuplicate).Name("is_duplicate");
        Map(x => x.IsIncluideAb).Name("is_include_ft");
        Map(x => x.Tags).Name("tags");
        Map(x => x.CiteId).Name("cite_id");
    }
}