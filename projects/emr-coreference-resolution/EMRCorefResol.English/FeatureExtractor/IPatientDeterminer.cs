﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.English
{
    interface IPatientDeterminer
    {
        bool? IsPatient(Concept concept);
    }
}
