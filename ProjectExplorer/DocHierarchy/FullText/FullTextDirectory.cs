using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using ProjectExplorer.DocHierarchy.FullText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.FullText
{
    public class FullTextDirectory
    {
        private static Analyzer analyzer = new CustomAnalyzer();

        private Directory luceneDirectory;

        private IndexWriter indexWriter;

        private IndexReader indexReader;

        private CustomQueryParser customQueryParser;

        public FullTextDirectory()
        {
            this.luceneDirectory = new RAMDirectory();
            this.indexWriter = new IndexWriter(this.luceneDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            this.indexReader = this.indexWriter.GetReader();
            this.customQueryParser = new CustomQueryParser();
        }

        public void AddFileSystemEntry(string path, string segment)
        {
            Document doc = new Document();
            Field field = new Field("Path", path, Field.Store.YES, Field.Index.NO);
            doc.Add(field);
            field = new Field("CatchAll", segment, Field.Store.NO, Field.Index.ANALYZED);
            field.OmitTermFreqAndPositions = true;
            doc.Add(field);
            doc.Boost = 2;
            this.indexWriter.AddDocument(doc);
            this.indexWriter.Commit();
        }

        public IEnumerable<string> Search(string searchText)
        {
            IndexReader newReader = this.indexReader.Reopen();
            if (newReader != this.indexReader)
            {
                this.indexReader.Dispose();
                this.indexReader = newReader;
            }
            IndexSearcher searcher = new IndexSearcher(this.indexReader);
            Query query = this.customQueryParser.Parse(searchText, "CatchAll");
            if (query == null)
            {
                return Enumerable.Empty<string>(); 
            }
            TopDocs hits = searcher.Search(query, 1000);
            IEnumerable<string> paths = hits.ScoreDocs.Select(scoreDoc => searcher.Doc(scoreDoc.Doc).Get("Path"));
            return paths;
        }

        /*private Query parseFilter(string filter)
        {
            //QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "CatchAll", analyzer);
            //Term term = new Term("CatchAll", filter.ToLowerInvariant());
            //return new PrefixQuery(term);
            //return new WildcardQuery(term);

            StringBuilder output = new StringBuilder();
            foreach (char c in filter)
            {

            }


            string filterPreparsed = this.preparseFilter(filter);
            return parser.Parse(filterPreparsed);
        }*/

        


        
    }
}
