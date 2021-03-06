﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English
{
    using Features;

    class TestPairFeatures : FeatureVector
    {
        public TestPairFeatures(TestPair instance, EMR emr, double classValue,
            WikiDataDictionary wikiData, UMLSDataDictionary umlsData, TemporalDataDictionary temporalData)
            : base(size: 20, classValue: classValue)
        {
            var anaWiki = wikiData.Get(instance.Anaphora.Lexicon);
            var anteWiki = wikiData.Get(instance.Antecedent.Lexicon);

            this[0] = new WikiMatchFeature(anaWiki, anteWiki);
            this[1] = new WikiAnchorLinkFeature(anaWiki, anteWiki);
            this[2] = new WikiBoldNameMatchFeature(anaWiki, anteWiki);
            this[3] = new WordNetMatchFeature(instance);

            this[4] = new PositionFeature(instance, emr);
            this[5] = new IndicatorFeature(instance, umlsData, emr);
            this[6] = new TemporalFeature(instance, emr, temporalData);
            this[7] = new SectionFeature(instance, emr, KeywordService.Instance.SECTION_TITLES);
            this[8] = new ModifierFeature(instance, emr);
            this[9] = new AnatomyFeature(instance, umlsData);
            this[10] = new EquipmentFeature(instance, umlsData);

            this[11] = new SentenceDistanceFeature(instance);
            this[12] = new ArticleFeature(instance);
            this[13] = new HeadNounFeature(instance);
            this[14] = new ContainFeature(instance);
            this[15] = new CapitolMatchFeature(instance);
            this[16] = new SubstringFeature(instance);
            this[17] = new CosineDistanceFeature(instance);
            this[18] = new StringMatchFeature(instance);
            this[19] = new ProcedureMatch(instance);
        }
    }
}
