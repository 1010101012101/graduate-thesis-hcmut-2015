﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol
{
    public interface IPreprocessor
    {
        IIndexedEnumerable<IClasInstance> Process(EMR emr);
    }
}