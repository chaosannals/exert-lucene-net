using System;
using System.IO;
using Lucene.Net.Util;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Cn.Smart;

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
            var hits = engine.Search(new MultiPhraseQuery
            {
                // new Term("info", "上海"),
                new Term("info", "北京"),
                new Term("info", "本地"),
                new Term("info", "人"),
            }, 20).ScoreDocs;
            Console.WriteLine($"hits: {hits.Length}");
            foreach (var hit in hits)
            {
                var fd = engine.GetDoc(hit.Doc);
                Console.WriteLine($"{hit.Score:f8} {fd.Get("name"),-15} {fd.Get("info"),-40} {fd.Get("age")}");
            }
        }
    }
}
