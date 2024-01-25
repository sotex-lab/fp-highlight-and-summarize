using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpHighlights.DTOs
{
    public class Response
    {
        public int ArticleId { get; set; }
        public IList<string> Responses { get; set; }
    }
}
