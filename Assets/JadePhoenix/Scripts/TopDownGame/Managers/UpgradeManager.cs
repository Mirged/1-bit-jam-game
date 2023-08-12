using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public interface IUpgradeable
    {
        List<Upgrade> Upgrades { get; }

        void ApplyUpgrade(Upgrade upgrade);
    }

    public class UpgradeManager : MonoBehaviour
    {
        public List<Upgrade> AllUpgrades;

        public virtual Upgrade GetRandomUpgrade()
        {
            int randomIndex = Random.Range(0, AllUpgrades.Count);
            return AllUpgrades[randomIndex];
        }
    }
}
