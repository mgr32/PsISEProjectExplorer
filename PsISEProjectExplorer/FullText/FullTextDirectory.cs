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
        private Analyzer Analyzer { get; set; }

        private Directory LuceneDirectory { get; set; }

        private IndexWriter IndexWriter { get; set; }

        private IndexReader IndexReader { get; set; }

        private CustomQueryParser CustomQueryParser { get; set; }

        public DocumentFactory DocumentCreator { get; private set; }

        public FullTextDirectory()
        {
			Analyzer = new CustomAnalyzer();
			LuceneDirectory = new RAMDirectory();
			IndexWriter = new IndexWriter(LuceneDirectory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
			IndexReader = IndexWriter.GetReader();
			CustomQueryParser = new CustomQueryParser();
			DocumentCreator = new DocumentFactory(IndexWriter);
        }

        public IList<SearchResult> SearchTerm(string searchTerm, FullTextFieldType field)
        {
            Query query = new TermQuery(new Term(field.ToString(), searchTerm));
            return RunQuery(query);   
        }

        public IList<SearchResult> Search(string searchText, FullTextFieldType field)
        {
            Query query = CustomQueryParser.Parse(searchText, field.ToString());
            return RunQuery(query);
            
        }

        public void DeleteDocument(string path)
        {
			IndexWriter.DeleteDocuments(new Term(FullTextFieldType.Path.ToString(), path));
        }

        private IList<SearchResult> RunQuery(Query query)
        {
            // If two threads ran this method simultaneously, there would be issues with this.IndexReader.
            // Alternatively, there could be one RAMDirectory per filesystem directory.
            lock (this)
            {
                IndexReader newReader = IndexReader.Reopen();
                if (newReader != IndexReader)
                {
					IndexReader.Dispose();
					IndexReader = newReader;
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
