using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    public class JP_Layers : MonoBehaviour
    {
        public static bool LayerInLayerMask(int layer, LayerMask layerMask)
        {
            if (((1 << layer) & layerMask) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
