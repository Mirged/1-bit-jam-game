using JadePhoenix.Tools;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class Entity : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("3D Model representation of the entity.")]
        public GameObject Model;

        public Health Health { get { return _health; } }

        // Protected Variables Group
        protected Health _health;

        #region UNITY LIFECYCLE

        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
        }

        /// <summary>
        /// This is called every frame.
        /// </summary>
        protected virtual void Update()
        {
            EveryFrame();
        }

        /// <summary>
        /// Resets the entity.
        /// </summary>
        public virtual void Reset() { }

        #endregion

        /// <summary>
        /// Initializes the state machines, components, and other essential data.
        /// </summary>
        protected virtual void Initialization()
        {
            _health = GetComponent<Health>();

            Transform model = transform.Find("Model");
            if (model != null)
            {
                Model = model.gameObject;
            }
        }

        /// <summary>
        /// Processes entity's actions and updates the state.
        /// </summary>
        protected virtual void EveryFrame()
        {
            // Entity-specific processing logic
        }
    }
}
