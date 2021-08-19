using System;
using System.IO;
using Lucene.Net.Util;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Analysis.TokenAttributes;

namespace Exert
{
    class Program
    {
        static void Main(string[] args)
        {
            using Engine engine = new Engine("index");
            engine.Insert(new
            {
                name = "李四",
                info = "目击证人，上海人。",
                age = 65,
            });
            var query = new MultiPhraseQuery();
            using var ts = engine.Analyzer.GetTokenStream("info", "上海人");
            ts.Reset();
            while (ts.IncrementToken())
            {
                var i = ts.GetAttribute<ICharTermAttribute>();
                Console.WriteLine(i.ToString());
                query.Add(new Term("info", i.ToString()));
            }
            
            var hits = engine.Search(query, 20).ScoreDocs;
            Console.WriteLine($"hits: {hits.Length}");
            foreach (var hit in hits)
            {
                var fd = engine.GetDoc(hit.Doc);
                Console.WriteLine($"{hit.Score:f8} {fd.Get("name"),-15} {fd.Get("info"),-40} {fd.Get("age")}");
            }
        }
    }
}
