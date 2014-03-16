using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using ProjectExplorer.DocHierarchy.FullText;
using ProjectExplorer.DocHierarchy.HierarchyLogic;
using ProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.FullText
{
    public class FullTextDirectory
    {
        private Analyzer Analyzer { get; set; }

        private Directory LuceneDirectory { get; set; }

        private IndexWriter IndexWriter { get; set; }

        private IndexReader IndexReader { get; set; }

        private CustomQueryParser CustomQueryParser { get; set; }

        public DocumentCreator DocumentCreator { get; private set; }

        public FullTextDirectory()
        {
            this.Analyzer = new CustomAnalyzer();
            this.LuceneDirectory = new RAMDirectory();
            this.IndexWriter = new IndexWriter(this.LuceneDirectory, this.Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            this.IndexReader = this.IndexWriter.GetReader();
            this.CustomQueryParser = new CustomQueryParser(this.Analyzer);
            this.DocumentCreator = new DocumentCreator(this.IndexWriter);
        }
        public IEnumerable<string> Search(string searchText, FullTextFieldType field)
        {
            IndexReader newReader = this.IndexReader.Reopen();
            if (newReader != this.IndexReader)
            {
                this.IndexReader.Dispose();
                this.IndexReader = newReader;
            }
            IndexSearcher searcher = new IndexSearcher(this.IndexReader);
            Query query = this.CustomQueryParser.Parse(searchText, field.ToString());
            if (query == null)
            {
                return Enumerable.Empty<string>(); 
            }
            TopDocs hits = searcher.Search(query, 1000);
            IEnumerable<string> paths = hits.ScoreDocs.Select(scoreDoc => searcher.Doc(scoreDoc.Doc).Get(FullTextFieldType.PATH.ToString()));
            return paths;
        }     

    }
}
