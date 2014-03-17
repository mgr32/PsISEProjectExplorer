using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundIndexer : BackgroundWorker
    {

        public BackgroundIndexer()
        {
            this.DoWork += new DoWorkEventHandler(doWork);
        }

        private void doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundIndexerParams indexerParams = (BackgroundIndexerParams)e.Argument;
            DocumentHierarchy docHierarchy = null;
            if (indexerParams.PathsChanged == null)
            {
                docHierarchy = indexerParams.DocumentHierarchyIndexer.CreateDocumentHierarchy(indexerParams.RootDirectory);
            }
            else
            {
                foreach (string path in indexerParams.PathsChanged)
                {
                    docHierarchy = indexerParams.DocumentHierarchyIndexer.UpdateDocumentHierarchy(indexerParams.RootDirectory, path);
                }
            }

            e.Result = new DocumentHierarchySearcher(docHierarchy);
        }

    }
}
