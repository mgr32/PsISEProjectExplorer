using Lucene.Net.Documents;
using Lucene.Net.Index;
using PsISEProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void AddFunctionEntry(string path, string name)
        {
            this.CreateDocument(path, name, string.Empty);
        }

        private void CreateDocument(string path, string name, string contents)
        {
            Document doc = new Document();
            Field field = new Field(FullTextFieldType.PATH.ToString(), path, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);
            field = new Field(FullTextFieldType.NAME.ToString(), name, Field.Store.NO, Field.Index.ANALYZED);
            field.OmitTermFreqAndPositions = true;
            doc.Add(field);
            field = new Field(FullTextFieldType.NAME_NOT_ANALYZED.ToString(), name, Field.Store.NO, Field.Index.NOT_ANALYZED);
            field.OmitTermFreqAndPositions = true;
            doc.Add(field);
            field = new Field(FullTextFieldType.CATCH_ALL.ToString(), name + " " + contents, Field.Store.NO, Field.Index.ANALYZED);
            field.OmitTermFreqAndPositions = true;
            doc.Add(field);
            this.IndexWriter.AddDocument(doc);
            this.IndexWriter.Commit();
        }
    }
}
