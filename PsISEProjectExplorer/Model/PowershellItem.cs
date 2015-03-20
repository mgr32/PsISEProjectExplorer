using PsISEProjectExplorer.Enums;
using System.Collections.Generic;
namespace PsISEProjectExplorer.Model
{
    public class PowershellItem
    {
        public PowershellItemType Type { get; private set; }

        public string Name { get; private set; }

        public int StartLine { get; private set; }

        public int StartColumn { get; private set; }

        public int NestingLevel { get; private set; }

        public PowershellItem Parent { get; private set; }

        public string ParsingErrors { get; private set; }

        public IList<PowershellItem> Children { get; private set; }

        public PowershellItem(PowershellItemType type, string name, int startLine, int startColumn, int nestingLevel, PowershellItem parent, string parsingErrors)
        {
			Type = type;
			Name = name;
			StartLine = startLine;
			StartColumn = startColumn;
			NestingLevel = nestingLevel;
			Parent = parent;
			ParsingErrors = parsingErrors;
			Children = new List<PowershellItem>();
            if (parent != null)
            {
				Parent.Children.Add(this);
            }
        }
    }
}
