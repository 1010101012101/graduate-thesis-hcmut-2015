﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English.Features
{
    using Utilities;
    class PronounTheyFeature : Feature
    {
        public PronounTheyFeature(PersonInstance instance)
            :base("Pronoun-They", 2, 0)
        {
            var conLex = instance.Concept.Lexicon;
            var conArr = conLex.Split(' ');

            if (conArr.Length == 1)
            {
                var kw_searcher = KeywordService.Instance.THEY_KEYWORDS;
                var exist = kw_searcher.Match(conLex, KWSearchOptions.WholeWord | KWSearchOptions.IgnoreCase);

                if (exist)
                {
                    SetCategoricalValue(1);
                }
            }
        }
    }
}
