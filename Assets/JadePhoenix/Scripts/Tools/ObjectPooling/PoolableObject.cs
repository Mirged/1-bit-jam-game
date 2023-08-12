using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    public class PoolableObject : MonoBehaviour
    {
        public delegate void Events();
        public event Events OnSpawnComplete;

        [Tooltip("The life time of the object in seconds. Setting to 0 will live indefinitely.")]
        public float LifeTime = 0f;

        /// <summary>
        /// Turns the instance inactive, ready for reuse.
        /// </summary>
        public virtual void Destroy()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Initiates death countdown when enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (LifeTime > 0f)
            {
                Invoke("Destroy", LifeTime);
            }
        }

        /// <summary>
        /// Disables death timer if disabled externally.
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        /// <summary>
        /// Triggers OnSpawnComplete event.
        /// </summary>
        public virtual void TriggerOnSpawnComplete()
        {
            OnSpawnComplete?.Invoke();
        }
    }
}
