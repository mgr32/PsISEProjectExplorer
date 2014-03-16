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
            if (indexerParams.FilesChanged == null)
            {
                DocumentHierarchy docHierarchy = indexerParams.DocumentHierarchyIndexer.CreateDocumentHierarchy(indexerParams.RootDirectory);
                e.Result = new DocumentHierarchySearcher(docHierarchy);
            }
        }

    }
}
