using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public interface IPowershellTokenizer
    {
        PowershellItem GetPowershellItems(string path, string contents);

        string GetTokenAtColumn(string line, int column);
    }
}
