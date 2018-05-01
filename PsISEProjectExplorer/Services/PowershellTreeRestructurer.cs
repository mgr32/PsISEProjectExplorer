using PsISEProjectExplorer.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class PowershellTreeRestructurer
    {

        public bool ShowRegions { get; set; }

        public PowershellItem AddRegions(PowershellItem rootItem, string fileContents)
        {
            if (!ShowRegions)
            {
                return rootItem;
            }
            List<PowershellRegion> regions = GetPowershellRegions(fileContents);
            if (!regions.Any())
            {
                return rootItem;
            }
            foreach (PowershellRegion region in regions)
            {
                List<PowershellItem> itemsInRegion = GetItemsInRegionWithMinimalNesting(rootItem, region);
                if (!itemsInRegion.Any())
                {
                    continue;
                }
                PowershellItem firstItemInRegion = itemsInRegion.First();
                PowershellItem regionItem = ConvertRegionToPowershellItem(region, firstItemInRegion);
                foreach (PowershellItem item in itemsInRegion)
                {
                    item.Reparent(regionItem);
                }
            }
            return rootItem;
        }

        private List<PowershellItem> GetItemsInRegionWithMinimalNesting(PowershellItem rootItem, PowershellRegion region)
        {
            List<PowershellItem> itemsInRegion = GetItemsInRegion(rootItem, region, new List<PowershellItem>());
            if (!itemsInRegion.Any())
            {
                return itemsInRegion;
            }
            int minimalNesting = itemsInRegion.Min(item => item.NestingLevel);
            return itemsInRegion.Where(item => item.NestingLevel == minimalNesting).ToList();
        }

        private List<PowershellItem> GetItemsInRegion(PowershellItem item, PowershellRegion region, List<PowershellItem> result)
        {
            if (item.StartLine > region.StartLine && item.StartLine < region.EndLine)
            {
                result.Add(item);
                return result;
            }
            foreach (PowershellItem child in item.Children)
            {
                result = GetItemsInRegion(child, region, result);
            }
            return result;
        }

        private PowershellItem ConvertRegionToPowershellItem(PowershellRegion region, PowershellItem firstItemInRegion)
        {
            return new PowershellItem(Enums.PowershellItemType.Region, region.Name, region.StartLine, 0, region.Name.Length + 8, firstItemInRegion.NestingLevel, firstItemInRegion.Parent, null);
        }

        private List<PowershellRegion> GetPowershellRegions(string fileContents)
        {
            if (string.IsNullOrWhiteSpace(fileContents))
            {
                return new List<PowershellRegion>();
            }
            int lineNum = 1;
            StringReader strReader = new StringReader(fileContents);
            List<PowershellRegion> result = new List<PowershellRegion>();
            Stack<PowershellRegion> currentRegions = new Stack<PowershellRegion>();
            while (true)
            {
                string line = strReader.ReadLine();
                if (line == null)
                {
                    break;
                }
                line = line.Trim();
                if (line.ToLower().StartsWith("#region"))
                {
                    PowershellRegion newRegion = new PowershellRegion();
                    newRegion.StartLine = lineNum;
                    newRegion.Name = line.Substring(8);
                    currentRegions.Push(newRegion);
                }
                else if (line.ToLower().StartsWith("#endregion") && currentRegions.Count != 0)
                {
                    PowershellRegion region = currentRegions.Pop();
                    region.EndLine = lineNum;
                    result.Add(region);
                }
                lineNum++;
            }
            return result;
        }
    }
}
