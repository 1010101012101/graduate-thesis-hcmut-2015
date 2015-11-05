﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English.Features
{
    using Utilities;
    class ArticleFeature : Feature
    {
        public ArticleFeature(IConceptPair instance)
            :base("Article-Feature", 9, 8)
        {
            var anaIndex = GetArticleWordIndex(instance.Anaphora.Lexicon);
            var anteIndex = GetArticleWordIndex(instance.Antecedent.Lexicon);

            var pairIndex = anaIndex * 3 + anteIndex;
            SetCategoricalValue(pairIndex);
        }

        private int GetArticleWordIndex(string term)
        {
            var searcher = new AhoCorasickKeywordDictionary(new string[] { "a", "an" });
            if(searcher.Match(term, KWSearchOptions.IgnoreCase | KWSearchOptions.WholeWord))
            {
                return 0;
            }

            searcher = new AhoCorasickKeywordDictionary(new string[] { "the", "his", "her", "my", "their", "that" });
            if (searcher.Match(term, KWSearchOptions.IgnoreCase | KWSearchOptions.WholeWord))
            {
                return 1;
            }

            return 2;
        }
    }
}
