using JadePhoenix.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.TopDownGame
{
    public class SpawnManager : Singleton<SpawnManager>
    {
        public bool CanSpawn = true;
        public ObjectPooler ObjectPooler { get; private set; } // It's a good practice to make setters private if they shouldn't be changed externally

        protected override void Awake()
        {
            base.Awake();
            Initialization();
        }

        protected virtual void Initialization()
        {
            // Ensure that ObjectPooler is not initialized more than once
            if (ObjectPooler != null) { return; }

            // Fetch the needed ObjectPooler with explicit casting
            ObjectPooler = GetComponent<MultipleObjectPooler>() as ObjectPooler ?? GetComponent<SimpleObjectPooler>() as ObjectPooler;

            if (ObjectPooler == null)
            {
                Debug.LogWarning($"{this.GetType()}.Initialization: no object pooler (simple or multiple) is attached to the SpawnManager, it won't be able to spawn anything.", gameObject);
            }
        }

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

        public virtual GameObject SpawnAtPosition(Vector3 positionToSpawn, string objectName)
        {
            if (ObjectPooler == null)
            {
                Debug.LogError($"{this.GetType()}.SpawnAtPosition: ObjectPooler is null.");
                return null;
            }

            if (ObjectPooler.GetType() != typeof(MultipleObjectPooler))
            {
                return SpawnAtPosition(positionToSpawn);
            }

            MultipleObjectPooler objectPooler = ObjectPooler as MultipleObjectPooler;  // Using 'as' for safer casting

            GameObject nextGameObject = objectPooler.GetPooledGameObjectOfType(objectName);

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
    }
}

