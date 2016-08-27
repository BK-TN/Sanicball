using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Sanicball.Logic
{
    [Serializable]
    public class MatchClient
    {
        public Guid Guid { get; private set; }
        public string Name { get; private set; }

        public MatchClient(Guid guid, string name)
        {
            Guid = guid;
            Name = name;
        }
    }
}