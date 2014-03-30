using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using System.Collections.Generic;
using System.Linq;

namespace PsISEProjectExplorer.FullText
{
    public class FullTextDirectory
    {
        private Analyzer Analyzer { get; set; }

        private Directory LuceneDirectory { get; set; }

        private IndexWriter IndexWriter { get; set; }

        private IndexReader IndexReader { get; set; }

        private CustomQueryParser CustomQueryParser { get; set; }

        public DocumentFactory DocumentCreator { get; private set; }

        public FullTextDirectory()
        {
            this.Analyzer = new CustomAnalyzer();
            this.LuceneDirectory = new RAMDirectory();
            this.IndexWriter = new IndexWriter(this.LuceneDirectory, this.Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            this.IndexReader = this.IndexWriter.GetReader();
            this.CustomQueryParser = new CustomQueryParser(this.Analyzer);
            this.DocumentCreator = new DocumentFactory(this.IndexWriter);
        }

        public IList<SearchResult> SearchTerm(string searchTerm, FullTextFieldType field)
        {
            Query query = new TermQuery(new Term(field.ToString(), searchTerm));
            return this.RunQuery(query);   
        }

        public IList<SearchResult> Search(string searchText, FullTextFieldType field)
        {
            Query query = this.CustomQueryParser.Parse(searchText, field.ToString());
            return this.RunQuery(query);
            
        }

        public void DeleteDocument(string path)
        {
            this.IndexWriter.DeleteDocuments(new Term(FullTextFieldType.Path.ToString(), path));
        }

        private IList<SearchResult> RunQuery(Query query)
        {
            var newReader = this.IndexReader.Reopen();
            if (newReader != this.IndexReader)
            {
                this.IndexReader.Dispose();
                this.IndexReader = newReader;
            }
            var searcher = new IndexSearcher(this.IndexReader);
            if (query == null)
            {
                return new List<SearchResult>();
            }
            TopDocs hits = searcher.Search(query, 1000);

            return hits.ScoreDocs
                .Select(scoreDoc => searcher.Doc(scoreDoc.Doc).Get(FullTextFieldType.Path.ToString()))
                .Select(path => new SearchResult(path))
                .ToList();
        }

    }
}
