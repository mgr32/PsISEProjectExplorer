using Lucene.Net.Documents;
using Lucene.Net.Index;
using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.FullText
{
    public class DocumentFactory
    {
        private IndexWriter IndexWriter { get; set; }

        public DocumentFactory(IndexWriter indexWriter)
        {
            this.IndexWriter = indexWriter;
        }

        public void AddDirectoryEntry(string path, string segment)
        {
            this.CreateDocument(path, segment, string.Empty);
        }

        public void AddFileEntry(string path, string fileName, string fileContents)
        {
            this.CreateDocument(path, fileName, fileContents);
        }

        public void AddPowershellItemEntry(string path, string name)
        {
            this.CreateDocument(path, name, string.Empty);
        }

        private void CreateDocument(string path, string name, string contents)
        {
            var doc = new Document();
            var field = new Field(FullTextFieldType.Path.ToString(), path, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);
            field = new Field(FullTextFieldType.Name.ToString(), name, Field.Store.NO, Field.Index.ANALYZED)
            {
                OmitTermFreqAndPositions = true
            };
            doc.Add(field);
            field = new Field(FullTextFieldType.NameNotAnalyzed.ToString(), name, Field.Store.NO, Field.Index.NOT_ANALYZED)
            {
                OmitTermFreqAndPositions = true
            };
            doc.Add(field);
            field = new Field(FullTextFieldType.CatchAll.ToString(), name + " " + contents, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS);
            doc.Add(field);
            this.IndexWriter.AddDocument(doc);
            this.IndexWriter.Commit();
        }
    }
}
