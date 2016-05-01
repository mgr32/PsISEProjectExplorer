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
    // note: one instance of this class can be used by many threads
    public class FullTextDirectory
    {
        private readonly Analyzer analyzer;

        private readonly Directory luceneDirectory;

        private readonly IndexWriter indexWriter;

        private IndexReader indexReader;

        private readonly CustomQueryParser customQueryParser;

        private readonly DocumentFactory documentFactory;

        public DocumentFactory DocumentFactory { get { return this.documentFactory; } }

        public FullTextDirectory(bool analyzeContents)
        {
            this.analyzer = new CustomAnalyzer();
            this.luceneDirectory = new RAMDirectory();
            this.indexWriter = new IndexWriter(this.luceneDirectory, this.analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            this.indexReader = this.indexWriter.GetReader();
            this.customQueryParser = new CustomQueryParser();
            this.documentFactory = new DocumentFactory(this.indexWriter, analyzeContents);
        }

        public IList<SearchResult> SearchTerm(string searchTerm, FullTextFieldType field)
        {
            Query query = new TermQuery(new Term(field.ToString(), searchTerm));
            return this.RunQuery(query);   
        }

        public IList<SearchResult> Search(SearchOptions searchOptions)
        {
            Query query = this.customQueryParser.Parse(searchOptions);
            return this.RunQuery(query);
            
        }

        public void DeleteDocument(string path)
        {
            this.indexWriter.DeleteDocuments(new Term(FullTextFieldType.Path.ToString(), path));
        }

        private IList<SearchResult> RunQuery(Query query)
        {
            // If two threads ran this method simultaneously, there would be issues with this.IndexReader.
            // Alternatively, there could be one RAMDirectory per filesystem directory.
            lock (this)
            {
                IndexReader newReader = this.indexReader.Reopen();
                if (newReader != this.indexReader)
                {
                    this.indexReader.Dispose();
                    this.indexReader = newReader;
                }

                IndexSearcher searcher; searcher = new IndexSearcher(newReader);
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
}
