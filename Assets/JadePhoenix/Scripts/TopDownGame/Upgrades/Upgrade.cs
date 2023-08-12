using JadePhoenix.TopDownGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.TopDownGame
{
    public abstract class Upgrade : ScriptableObject
    {
        public string Name;
        public string Description;

        public abstract void Apply(IUpgradeable target);
    }
}
