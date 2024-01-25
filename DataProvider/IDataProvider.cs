using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fp_highlights_new.DataProvider
{
    public interface IDataProvider
    {
        string GetInputFolderPath();
        string GetOutputFolderPath();
        int GetTotalMinimumTokens();
        int GetMinimumTokens();
    }
}