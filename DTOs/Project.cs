using CsvHelper.Configuration;

public class Project {
    public int ProjectId { get; set; }
    public int ArticleId { get; set; }
    public string Url { get; set; }
    public string PdfUrl { get; set; }
    public bool IsFulltext { get; set; }
    public bool IsDuplicate { get; set; }
    public bool IsIncludeAb { get; set; }
    public bool IsIncludeFt { get; set; }
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
        Map(x => x.IsFulltext).Name("is_fulltext").TypeConverterOption.BooleanValues(true, true, "1")
            .TypeConverterOption.BooleanValues(false, true, "0", String.Empty);
        Map(x => x.IsDuplicate).Name("is_duplicate").TypeConverterOption.BooleanValues(true, true, "1")
            .TypeConverterOption.BooleanValues(false, true, "0", String.Empty);
        Map(x => x.IsIncludeAb).Name("is_include_ab").TypeConverterOption.BooleanValues(true, true, "1")
            .TypeConverterOption.BooleanValues(false, true, "0", String.Empty);
        Map(x => x.IsIncludeFt).Name("is_include_ft").TypeConverterOption.BooleanValues(true, true, "1")
            .TypeConverterOption.BooleanValues(false, true, "0", String.Empty);
        Map(x => x.Tags).Name("tags");
        Map(x => x.CiteId).Name("cite_id");
    }
}