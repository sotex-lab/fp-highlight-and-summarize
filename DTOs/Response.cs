using System.Collections.Generic;

namespace FpHighlights.DTOs
{
    public class Response
    {
        public int ArticleId { get; set; }
        public IList<string> Responses { get; set; }
    }
}
