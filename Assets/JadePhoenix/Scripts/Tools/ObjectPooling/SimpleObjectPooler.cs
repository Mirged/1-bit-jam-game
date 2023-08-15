using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    /// <summary>
    /// SimpleObjectPooler is a class derived from ObjectPooler, responsible for pooling game objects.
    /// It handles the creation, management, and retrieval of pooled objects of a single type.
    /// </summary>
    public class SimpleObjectPooler : ObjectPooler
    {
        [Tooltip("The GameObject that the pool will manage.")]
        public GameObject ObjectToPool;

        [Tooltip("The initial number of objects in the pool.")]
        public int PoolCount = 20;

        [Tooltip("Determines whether the pool can expand if more objects are needed.")]
        public bool PoolCanExpand = true;

        // Protected list holding the pooled GameObjects.
        protected List<GameObject> _pooledObjects;

        /// <summary>
        /// Fills the object pool with the specified GameObjects.
        /// If the ObjectToPool is not defined, the method will exit early.
        /// </summary>
        public override void FillObjectPool()
        {
            if (ObjectToPool == null) { return; }

            CreateWaitingPool();

            _pooledObjects = new List<GameObject>();

            int objectsToSpawn = PoolCount;

            // Get reference to existing pool if one is found.
            if (_objectPool != null)
            {
                objectsToSpawn -= _objectPool.PooledObjects.Count;
                _pooledObjects = new List<GameObject>(_objectPool.PooledObjects);
            }

            // Add specified number of objects to pool.
            for (int i = 0; i < objectsToSpawn; i++)
            {
                AddObjectToPool();
            }
        }

        /// <summary>
        /// Creates a new object and adds it to the pool.
        /// </summary>
        /// <returns>The newly created and added GameObject.</returns>
        protected virtual GameObject AddObjectToPool()
        {
            if (ObjectToPool == null)
            {
                Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any ObjectToPool defined.", gameObject);
                return null;
            }

            GameObject newGameObject = Instantiate(ObjectToPool);
            newGameObject.gameObject.SetActive(false);
            if (NestWaitingPool)
            {
                newGameObject.transform.SetParent(_waitingPool.transform);
            }
            newGameObject.name = $"{ObjectToPool.name}-{_pooledObjects.Count}";

            _pooledObjects.Add(newGameObject);
            _objectPool.PooledObjects.Add(newGameObject);

            return newGameObject;
        }

        /// <summary>
        /// This method returns one inactive object from the pool.
        /// </summary>
        /// <returns>The pooled game object, or null if none are found.</returns>
        public override GameObject GetPooledGameObject()
        {
            // We loop through the pool looking for an available inactive object.
            for (int i = 0; i < _pooledObjects.Count; i++)
            {
                if (!_pooledObjects[i].gameObject.activeInHierarchy)
                {
                    return _pooledObjects[i];
                }
            }
            // If no inactive object is found and the pool is able to expand, we add an object to the pool.
            if (PoolCanExpand)
            {
                return AddObjectToPool();
            }

            // If the pool is empty and can't expand, we send a warning and return nothing.
            Debug.LogWarning("The " + gameObject.name + " ObjectPooler has no PooledObjects available and cannot expand.", gameObject);
            return null;
        }
    }
}
