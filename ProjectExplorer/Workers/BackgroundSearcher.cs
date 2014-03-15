using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer
{
    public class BackgroundSearcher : BackgroundWorker
    {
        public BackgroundSearcher()
        {
            this.DoWork += new DoWorkEventHandler(doWork);
        }

        private void doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundSearcherParams indexerParams = (BackgroundSearcherParams)e.Argument;
            if (indexerParams.DocumentHierarchySearcher == null) 
            {
                e.Result = null;
                return;
            }
            e.Result = indexerParams.DocumentHierarchySearcher.GetFilteredDocumentHierarchyNodes(indexerParams.SearchText, indexerParams.SearchOptions);
        }
    }

}
