﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English.Features
{
    class MentionDistanceFeature : Feature
    {
        public MentionDistanceFeature(IConceptPair pair, EMR emr)
            : base("Mention-Distance")
        {
            var concepts = emr.Concepts;
            var i = concepts.IndexOf(pair.Antecedent);
            int j = i;
            for (; j < concepts.Count && !concepts[j].Equals(pair.Anaphora); j++) ;
            SetContinuousValue(j - i - 1);
        }
    }
}
