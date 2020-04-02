using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Xml;
using Newtonsoft.Json;

namespace SearchLoad2
{
    public partial class TestDocument
    {
        [System.ComponentModel.DataAnnotations.Key]
        [JsonProperty("Id")]        
        public string Id { get; set; }

        [IsSearchable]
        [Analyzer("jj_custom_analyzer")]
        [JsonProperty("Content")]
        public string Content { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string indexName = "index";

            SearchServiceClient searchClient = new SearchServiceClient("jjsearch", new SearchCredentials("160B616814BC19916B887A75E4D47A44"));

            if (!searchClient.Indexes.Exists(indexName))
            {
                var newIndex = new Index()
                {
                    Name = indexName,
                    Fields = FieldBuilder.BuildForType<TestDocument>(),
                    Analyzers = new[]
                    {
                        new CustomAnalyzer()
                        {
                                Name = "jj_custom_analyzer",
                                Tokenizer = "jj_czech_tokenizer",
                                TokenFilters = new[] { TokenFilterName.AsciiFolding }
                        }
                    },
                    Tokenizers = new[]
                    {
                        new MicrosoftLanguageStemmingTokenizer()
                        {
                            Name = "jj_czech_tokenizer",
                            Language = MicrosoftStemmingTokenizerLanguage.Czech
                        }
                    }
                };

                searchClient.Indexes.Create(newIndex);
            }

            ISearchIndexClient indexClient = searchClient.Indexes.GetClient(indexName);

            string[] dataToLoad = new string[]
                {
                    "Jak se máš",
                    "Jak se máte",
                    "Máme se dobře",
                    "Dobrých časů ubývá"
                };

            int i = 0;
            var documents = new List<TestDocument>();
            foreach (string str in dataToLoad)
            {
                TestDocument document = new TestDocument()
                {
                    Id = i.ToString(),
                    Content = str
                };
                documents.Add(document);
                i++;
            }
            var batch = IndexBatch.Upload(documents);
            var result = indexClient.Documents.Index(batch);
        }
    }
}
