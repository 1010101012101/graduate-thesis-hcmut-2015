﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English.Features
{
    class FirstChunkAfterMention : Feature
    {
        public FirstChunkAfterMention(ISingleConcept instance, EMR emr)
            :base("First-ChunkAfter", 9, 0)
        {
            var line = EMRExtensions.GetLine(emr, instance.Concept.Begin.Line);

            var chunks = Service.English.GetChunks(line);

            if (instance.Concept.End.WordIndex < chunks.Length -1)
            {
                var nextChunk = chunks[instance.Concept.Begin.WordIndex + 1];
                var tags = nextChunk.Split('|')[1].Split('-');
                if (!tags[0].Equals("O", StringComparison.InvariantCultureIgnoreCase))
                {
                    var index = getChunkIndex(tags[1]);
                    SetCategoricalValue(index);
                }
            }
        }

        private int getChunkIndex(string chunk)
        {
            switch (chunk.ToUpper())
            {
                case "NP":
                    return 1;
                case "PP":
                    return 2;
                case "VP":
                    return 3;
                case "ADVP":
                    return 4;
                case "ADJP":
                    return 5;
                case "SBAR":
                    return 6;
                case "PRT":
                    return 7;
                case "INTJ":
                    return 8;
                default:
                    return 0;
            }
        }
    }
}
