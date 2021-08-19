using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Store;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Cn.Smart;

namespace Exert
{
    public class Engine : IDisposable
    {
        public string Name { get; private set; }
        public LuceneVersion Version { get; private set; }
        public string Folder { get; private set; }
        public Analyzer Analyzer { get; private set; }
        public IndexWriterConfig WriterConfig { get; private set; }
        public FSDirectory Directory { get; private set; }
        public IndexWriter Writer { get; private set; }

        public Engine(string name)
        {
            Name = name;
            Version = LuceneVersion.LUCENE_48;
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Folder = Path.Combine(baseDir, name);
            Analyzer = new SmartChineseAnalyzer(Version);
            WriterConfig = new IndexWriterConfig(Version, Analyzer);
            Directory = FSDirectory.Open(Folder);
            Writer = new IndexWriter(Directory, WriterConfig);
        }

        public void Insert<T>(T one)
        {
            var doc = new Document();
            foreach (var p in typeof(T).GetProperties())
            {
                var v = p.GetValue(one, null);
                if (v is string)
                {
                    doc.Add(new TextField(p.Name, v as string, Field.Store.YES));
                }
                else if (v is int)
                {
                    doc.Add(new Int32Field(p.Name, (int)v, Field.Store.YES));
                }
                else if (v is double)
                {
                    doc.Add(new DoubleField(p.Name, (double)v, Field.Store.YES));
                }
            }
            Writer.AddDocument(doc);
            Writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        public TopDocs Search(Query query, int n)
        {
            using var reader = Writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            return searcher.Search(query, n);
        }

        public Document GetDoc(int i)
        {
            using var reader = Writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            return searcher.Doc(i);
        }

        public List<Document> GetDocs(List<int> indexes)
        {
            using var reader = Writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            List<Document> result = new List<Document>();
            foreach(int i in indexes)
            {
                result.Add(searcher.Doc(i));
            }
            return result;
        }

        public void Dispose()
        {
            Writer?.Dispose();
            Directory?.Dispose();
            Analyzer?.Dispose();
        }
    }
}
