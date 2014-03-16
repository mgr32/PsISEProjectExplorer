using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.DocHierarchy.HierarchyLogic
{
    public class DocumentHierarchies
    {
        private IDictionary<string, DocumentHierarchy> HierarchyMap { get; set; }

        private DocumentHierarchyFactory documentHierarchyFactory;

        public DocumentHierarchies()
        {
            this.HierarchyMap = new Dictionary<string, DocumentHierarchy>();
            this.documentHierarchyFactory = new DocumentHierarchyFactory();
        }

        public DocumentHierarchy CreateDocumentHierarchy(string rootPath)
        {
            if (!this.HierarchyMap.ContainsKey(rootPath))
            {
                this.HierarchyMap.Add(rootPath, this.documentHierarchyFactory.CreateDocumentHierarchy(rootPath));
            }
            return this.HierarchyMap[rootPath];
        }

        public DocumentHierarchy GetDocumentHierarchy(string rootPath)
        {
         
            return this.HierarchyMap[rootPath];
        }
    }
}
