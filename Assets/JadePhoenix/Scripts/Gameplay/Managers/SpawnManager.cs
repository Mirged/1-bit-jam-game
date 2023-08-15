using JadePhoenix.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    /// <summary>
    /// The SpawnManager class handles the spawning of objects in the game using an ObjectPooler.
    /// It is responsible for fetching pooled objects, activating them, and setting their position.
    /// </summary>
    public class SpawnManager : Singleton<SpawnManager>
    {
        [Tooltip("Flag to control whether spawning is allowed.")]
        public bool CanSpawn = true;

        /// <summary>
        /// Gets the ObjectPooler used by this SpawnManager. This can be either a MultipleObjectPooler or SimpleObjectPooler.
        /// The setter is private to prevent external modification.
        /// </summary>
        public ObjectPooler ObjectPooler { get; private set; }

        /// <summary>
        /// Unity's Awake method. Calls Initialization to set up the SpawnManager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Initialization();
        }

        /// <summary>
        /// Initializes the SpawnManager by fetching the required ObjectPooler.
        /// If no ObjectPooler is found, a warning is logged.
        /// </summary>
        protected virtual void Initialization()
        {
            // Ensure that ObjectPooler is not initialized more than once
            if (ObjectPooler != null)
            {
                Debug.LogWarning($"{this.GetType()}.Initialization: ObjectPooler [{this.ObjectPooler}] already Initialized.", gameObject);
            }

            // Fetch the needed ObjectPooler with explicit casting
            ObjectPooler = GetComponent<MultipleObjectPooler>() as ObjectPooler ?? GetComponent<SimpleObjectPooler>() as ObjectPooler;

            if (ObjectPooler == null)
            {
                Debug.LogWarning($"{this.GetType()}.Initialization: no object pooler (simple or multiple) is attached to the SpawnManager, it won't be able to spawn anything.", gameObject);
            }
        }

        /// <summary>
        /// Spawns an object from the pool at the specified position.
        /// </summary>
        /// <param name="positionToSpawn">Position where the object will be spawned.</param>
        /// <returns>The spawned GameObject, or null if the spawn fails.</returns>
        public virtual GameObject SpawnAtPosition(Vector3 positionToSpawn)
        {
            if (ObjectPooler == null)
            {
                Debug.LogError($"{this.GetType()}.SpawnAtPosition: ObjectPooler is null.");
                return null;
            }

            GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

            if (nextGameObject == null) { return null; }
            if (nextGameObject.GetComponent<PoolableObject>() == null)
            {
                throw new Exception($"{this.GetType()}.SpawnAtPosition: {this.gameObject.name} is attempting to spawn an object {nextGameObject.name} that does not have a PoolableObject component.");
            }

            nextGameObject.SetActive(true);
            nextGameObject.GetComponent<PoolableObject>().TriggerOnSpawnComplete();

            Health objectHealth = nextGameObject.GetComponent<Health>();
            if (objectHealth != null)
            {
                objectHealth.Revive();
            }

            nextGameObject.transform.position = positionToSpawn;

            return nextGameObject;
        }

        /// <summary>
        /// Spawns an object from the pool at the specified position, using a specific object name if a MultipleObjectPooler is used.
        /// </summary>
        /// <param name="positionToSpawn">Position where the object will be spawned.</param>
        /// <param name="objectName">The name of the object to spawn, used only with MultipleObjectPooler.</param>
        /// <returns>The spawned GameObject, or null if the spawn fails.</returns>
        public virtual GameObject SpawnAtPosition(Vector3 positionToSpawn, string objectName)
        {
            if (ObjectPooler == null)
            {
                Debug.LogError($"{this.GetType()}.SpawnAtPosition: ObjectPooler is null.");
                return null;
            }

            if (ObjectPooler.GetType() != typeof(MultipleObjectPooler))
            {
                Debug.LogWarning($"{this.GetType()}.SpawnAtPosition: ObjectPooler is not type MultipleObjectPooler, using basic SpawnAtPosition method.");
                return SpawnAtPosition(positionToSpawn);
            }

            MultipleObjectPooler objectPooler = ObjectPooler as MultipleObjectPooler;  // Using 'as' for safer casting

            GameObject nextGameObject = objectPooler.GetPooledGameObjectOfType(objectName);

            if (nextGameObject == null)
            {
                Debug.LogWarning($"{this.GetType()}.SpawnAtPosition: {objectName} not found. Returning Null.", nextGameObject);
                return null;
            }
            if (nextGameObject.GetComponent<PoolableObject>() == null)
            {
                throw new Exception($"{this.GetType()}.SpawnAtPosition: {this.gameObject.name} is attempting to spawn an object {nextGameObject.name} that does not have a PoolableObject component.");
            }

            nextGameObject.SetActive(true);
            nextGameObject.GetComponent<PoolableObject>().TriggerOnSpawnComplete();

            Health objectHealth = nextGameObject.GetComponent<Health>();
            if (objectHealth != null)
            {
                objectHealth.Revive();
            }

            nextGameObject.transform.position = positionToSpawn;

            //Debug.Log($"{this.GetType()}.SpawnAtPosition: NextGameObject [{nextGameObject.name}] found. Returning NextGameObject.", nextGameObject);
            return nextGameObject;
        }
    }
}

