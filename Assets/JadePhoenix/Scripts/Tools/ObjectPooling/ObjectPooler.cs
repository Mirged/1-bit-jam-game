using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    public class ObjectPooler : MonoBehaviour
    {
        [Tooltip("If this is true, the pool will try not to create a new waiting pool if it finds one with the same name.")]
        public bool MutualizeWaitingPools = false;

        [Tooltip("If this is true, all waiting and active objects will be regrouped under an empty game object. Otherwise, they will be kept at top level in the heirarchy.")]
        public bool NestWaitingPool = true;

        // Object used to group pooled objects.
        protected GameObject _waitingPool = null;
        protected ObjectPool _objectPool;

        protected virtual void Awake()
        {
            FillObjectPool();
        }

        protected virtual void CreateWaitingPool()
        {
            if (!NestWaitingPool) { return; }

            GameObject waitingPool = GameObject.Find(DetermineObjectPoolName());

            if (!MutualizeWaitingPools || waitingPool == null)
            {
                // create a container that will hold all instances created
                _waitingPool = new GameObject(DetermineObjectPoolName());
                _objectPool = _waitingPool.AddComponent<ObjectPool>();
                _objectPool.PooledObjects = new List<GameObject>();
            }
            else
            {
                _waitingPool = waitingPool;
                _objectPool = waitingPool.GetComponent<ObjectPool>();
            }
        }

        /// <summary>
        /// Determines the name of the object pool.
        /// </summary>
        /// <returns>The name of the object pool.</returns>
        protected virtual string DetermineObjectPoolName()
        {
            return $"[{this.GetType().Name}]_{this.name}";
        }

        /// <summary>
        /// Determines the name of the object pool.
        /// </summary>
        /// <param name="objectToPool">The object that is being pooled.</param>
        /// <returns>The name of the object pool.</returns>
        protected virtual string DetermineObjectPoolName(string objectToPool)
        {
            return $"[{this.GetType().Name}]_{this.name}: {objectToPool}";
        }

        public virtual void FillObjectPool()
        {
            Debug.LogError($"{gameObject.name}.FillObjectPool: should not have the base ObjectPooler class.");
        }

        /// <summary>
        /// Implement this method to return a gameobject
        /// </summary>
        /// <returns>The pooled game object.</returns>
        public virtual GameObject GetPooledGameObject()
        {
            Debug.LogError($"{gameObject.name}.GetPooledGameObject: should not have the base ObjectPooler class.");
            return null;
        }

        /// <summary>
        /// Destroys the object pool
        /// </summary>
        public virtual void DestroyObjectPool()
        {
            if (_waitingPool != null)
            {
                Destroy(_waitingPool);
            }
        }
    }
}
