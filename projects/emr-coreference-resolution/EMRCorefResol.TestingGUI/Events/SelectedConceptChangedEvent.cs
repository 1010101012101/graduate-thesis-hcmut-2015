﻿using HCMUT.EMRCorefResol;
using Prism.Events;
using System.Collections.Generic;

namespace EMRCorefResol.TestingGUI
{
    class SelectedConceptsChangedEvent : PubSubEvent<IReadOnlyList<Concept>>
    {
    }
}