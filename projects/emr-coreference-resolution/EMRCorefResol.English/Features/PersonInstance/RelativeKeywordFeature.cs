﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English.Features
{
    class RelativeKeywordFeature : Feature
    {
        public RelativeKeywordFeature(PersonInstance instance)
            : base("Relative-Keyword", 2, 0)
        {
            string[] stopWords = { "wife", "brother", "sister", "sibling", "nephew", "parent", "mother", "father" };
            foreach (string stopWord in stopWords)
            {
                if (checkContain(instance.Concept.Lexicon.ToLower(), stopWord))
                {
                    SetCategoricalValue(1);
                    return;
                }
            }
        }

        private bool checkContain(string s1, string s2)
        {
            return Regex.IsMatch(s1, string.Format(@"\b{0}\b", Regex.Escape(s2)));
        }
    }
}
