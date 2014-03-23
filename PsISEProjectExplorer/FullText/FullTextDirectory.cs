using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IEnumerable<SearchResult> SearchTerm(string searchTerm, FullTextFieldType field)
        {
            Query query = new TermQuery(new Term(field.ToString(), searchTerm));
            return this.RunQuery(query, field);   
        }

        public IEnumerable<SearchResult> Search(string searchText, FullTextFieldType field)
        {
            Query query = this.CustomQueryParser.Parse(searchText, field.ToString());
            return this.RunQuery(query, field);
            
        }

        public void DeleteDocument(string path)
        {
            this.IndexWriter.DeleteDocuments(new Term(FullTextFieldType.PATH.ToString(), path));
        }

        private IEnumerable<SearchResult> RunQuery(Query query, FullTextFieldType field)
        {
            IndexReader newReader = this.IndexReader.Reopen();
            if (newReader != this.IndexReader)
            {
                this.IndexReader.Dispose();
                this.IndexReader = newReader;
            }
            IndexSearcher searcher = new IndexSearcher(this.IndexReader);
            if (query == null)
            {
                return Enumerable.Empty<SearchResult>();
            }
            TopDocs hits = searcher.Search(query, 1000);

            var result = new List<SearchResult>();
            
            foreach (ScoreDoc scoreDoc in hits.ScoreDocs)
            {
                string path = searcher.Doc(scoreDoc.Doc).Get(FullTextFieldType.PATH.ToString());
                ITermFreqVector freqVector = this.IndexReader.GetTermFreqVector(scoreDoc.Doc, field.ToString());
                result.Add(new SearchResult(path));
            }

            return result;
        }

    }
}
