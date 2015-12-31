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

        public int EndColumn { get; private set; }

        public int NestingLevel { get; private set; }

        public PowershellItem Parent { get; private set; }

        public string ParsingErrors { get; private set; }

        public IList<PowershellItem> Children { get; private set; }

        public PowershellItem(PowershellItemType type, string name, int startLine, int startColumn, int endColumn, int nestingLevel, PowershellItem parent, string parsingErrors)
        {
            this.Type = type;
            this.Name = name;
            this.StartLine = startLine;
            this.StartColumn = startColumn;
            this.EndColumn = endColumn;
            this.NestingLevel = nestingLevel;
            this.Parent = parent;
            this.ParsingErrors = parsingErrors;
            this.Children = new List<PowershellItem>();
            if (parent != null)
            {
                this.Parent.Children.Add(this);
            }
        }
    }
}
