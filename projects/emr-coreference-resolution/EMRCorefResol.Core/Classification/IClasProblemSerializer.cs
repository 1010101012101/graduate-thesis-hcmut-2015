﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.Classification
{
    public interface IClasProblemSerializer
    {
        void Serialize(ClasProblem problem, string filePath);
        void Serialize(ClasProblem problem, string filePath, bool doScale);
        ClasProblem Deserialize(string filePath);
    }
}