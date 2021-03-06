﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English
{
    using Features;
    using System.IO;

    class TreatmentPairFeatures : FeatureVector
    {
        public TreatmentPairFeatures(TreatmentPair instance, EMR emr, double classValue,
            MedDataDictionary medData, WikiDataDictionary wikiData, UMLSDataDictionary umlsData, 
            TemporalDataDictionary temporalData)
            : base(size: 20, classValue: classValue)
        {
            var anaMedicationInfo = medData.Get(new MedKey(instance.Anaphora, emr));
            var anteMedicationInfo = medData.Get(new MedKey(instance.Antecedent, emr));

            var anaWiki = wikiData.Get(instance.Anaphora.Lexicon);
            var anteWiki = wikiData.Get(instance.Antecedent.Lexicon);

            this[0] = new WikiMatchFeature(anaWiki, anteWiki);
            this[1] = new WikiAnchorLinkFeature(anaWiki, anteWiki);
            this[2] = new WikiBoldNameMatchFeature(anaWiki, anteWiki);
            this[3] = new WordNetMatchFeature(instance);

            this[4] = new PositionFeature(instance, emr);
            this[5] = new DrugFeature(anaMedicationInfo, anteMedicationInfo);
            this[6] = new DosageFeature(anaMedicationInfo, anteMedicationInfo);
            this[7] = new FrequencyFeature(anaMedicationInfo, anteMedicationInfo);
            this[8] = new DurationFeature(anteMedicationInfo, anteMedicationInfo);
            this[9] = new TemporalFeature(instance, emr, temporalData);
            this[10] = new SectionFeature(instance, emr, KeywordService.Instance.SECTION_TITLES);
            this[11] = new OperationFeature(instance, umlsData);
            this[12] = new ProcedureMatch(instance);

            this[13] = new SentenceDistanceFeature(instance);
            this[14] = new ArticleFeature(instance);
            this[15] = new HeadNounFeature(instance);
            this[16] = new ContainFeature(instance);
            this[17] = new CapitolMatchFeature(instance);
            this[18] = new SubstringFeature(instance);
            this[19] = new CosineDistanceFeature(instance);
            //this[19] = new StringMatchFeature(instance);            
        }
    }
}
