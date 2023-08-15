using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JadePhoenix.Tools
{
    /// <summary>
    /// Represents an object to be used with the MultipleObjectPooler.
    /// </summary>
    [System.Serializable]
    public class MultipleObjectPoolerObject
    {
        public GameObject GameObjectToPool;
        public int PoolSize;
        public bool PoolCanExpand = true;
        public bool Enabled = true;
    }

    /// <summary>
    /// A pool that can handle multiple types of objects and expands if needed.
    /// </summary>
    public class MultipleObjectPooler : ObjectPooler
    {
        public List<MultipleObjectPoolerObject> Pool;
        public Dictionary<string, ObjectPool> ObjectPoolsByObjectType;

        protected List<GameObject> _pooledGameObjects;
        protected List<GameObject> _pooledGameObjectsOriginalOrder;
        protected List<MultipleObjectPoolerObject> _randomizedPool;
        protected int _currentIndex = 0;

        /// <summary>
        /// Fills the object pool by creating and organizing objects.
        /// </summary>
        public override void FillObjectPool()
        {
            // Create the waiting pool (if not already created).
            CreateWaitingPool();

            // Initialize the lists to store pooled objects and the randomized pool.
            _randomizedPool = new List<MultipleObjectPoolerObject>();

            // Randomize the order of objects in the pool for better distribution.
            for (int i = 0; i < Pool.Count; i++)
            {
                _randomizedPool.Add(Pool[i]);
            }
            _randomizedPool.Shuffle();

            // Fill the pool with objects.
            int[] poolSizes = new int[Pool.Count];
            for (int i = 0; i < Pool.Count; i++)
            {
                // Set the pool sizes based on the specified PoolSize property of each object type.
                poolSizes[i] = Pool[i].PoolSize;
            }

            // Continue pooling objects until there are no more objects to add to the pool.
            bool objectsRemainingToPool = true;
            while (objectsRemainingToPool)
            {
                objectsRemainingToPool = false;
                for (int i = 0; i < Pool.Count; i++)
                {
                    if (poolSizes[i] > 0)
                    {
                        // Add one object of type Pool[i].GameObjectToPool to the corresponding waiting pool.
                        AddOneObjectToThePool(Pool[i].GameObjectToPool);
                        poolSizes[i]--;
                        objectsRemainingToPool = true;
                    }
                }
            }

            // Store the original order of pooled objects for future reference (optional).
            _pooledGameObjectsOriginalOrder = new List<GameObject>(ObjectPoolsByObjectType.Values.SelectMany(pool => pool.PooledObjects));
        }

        protected override void CreateWaitingPool()
        {
            if (!NestWaitingPool)
            {
                Debug.Log($"{this.GetType()}.CreateWaitingPool: NestWaitingPool is false, exiting method.", gameObject); 
                return;
            }

            // Initialize the dictionary to store waiting pools for each unique object.
            if (ObjectPoolsByObjectType == null)
            {
                ObjectPoolsByObjectType = new Dictionary<string, ObjectPool>();
            }

            for (int i = 0; i < Pool.Count; i++)
            {
                string objToPool = Pool[i].GameObjectToPool.name;

                if (!ObjectPoolsByObjectType.ContainsKey(objToPool) || !ObjectPoolsByObjectType[objToPool])
                {
                    // Create a new waiting pool for each unique object type.
                    GameObject waitingPool = GameObject.Find(DetermineObjectPoolName(objToPool));

                    if (!MutualizeWaitingPools || waitingPool == null)
                    {
                        // Create a container that will hold all instances created for this object type.
                        waitingPool = new GameObject(DetermineObjectPoolName(objToPool));
                        // Add pooled object to Dictionary using objToPool as the Key and the ObjectPool component as the Value.
                        ObjectPoolsByObjectType[objToPool] = waitingPool.AddComponent<ObjectPool>();
                        ObjectPoolsByObjectType[objToPool].PooledObjects = new List<GameObject>();
                    }
                    else
                    {
                        ObjectPoolsByObjectType[objToPool] = waitingPool.GetComponent<ObjectPool>();
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new object to the pool.
        /// </summary>
        /// <param name="typeOfObject">The type of object to add.</param>
        /// <returns>The newly added object.</returns>
        protected virtual GameObject AddOneObjectToThePool(GameObject typeOfObject)
        {
            GameObject newGameObject = Instantiate(typeOfObject);
            newGameObject.gameObject.SetActive(false);
            if (NestWaitingPool)
            {
                ObjectPool pool = ObjectPoolsByObjectType[typeOfObject.name];
                newGameObject.transform.SetParent(pool.transform);
            }
            newGameObject.name = typeOfObject.name;

            // Instead of adding to _pooledGameObjects, we add the object to the corresponding waiting pool.
            ObjectPoolsByObjectType[typeOfObject.name].PooledObjects.Add(newGameObject);

            return newGameObject;
        }

        /// <summary>
        /// Gets an object from the pool.
        /// </summary>
        /// <returns>The pooled game object.</returns>
        public override GameObject GetPooledGameObject()
        {
            int newIndex;
            if (_currentIndex >= _pooledGameObjectsOriginalOrder.Count)
            {
                ResetCurrentIndex();
            }
            MultipleObjectPoolerObject searchedObject = GetObjectPool(_pooledGameObjectsOriginalOrder[_currentIndex].gameObject.name);

            if (_currentIndex >= _pooledGameObjectsOriginalOrder.Count) { return null; }
            if (!searchedObject.Enabled) { _currentIndex++; return null; }

            if (_pooledGameObjectsOriginalOrder[_currentIndex].gameObject.activeInHierarchy)
            {
                GameObject findObject = FindInactiveObject(_pooledGameObjectsOriginalOrder[_currentIndex].gameObject.name, _pooledGameObjectsOriginalOrder);
                if (findObject != null)
                {
                    _currentIndex++;
                    return findObject;
                }

                // If its pool can expand, we create a new one.
                if (searchedObject.PoolCanExpand)
                {
                    _currentIndex++;
                    return AddOneObjectToThePool(searchedObject.GameObjectToPool);
                }
                else
                {
                    // If it can't expand, we return nothing.
                    return null;
                }
            }
            else
            {
                // If the object is inactive, we return it.
                newIndex = _currentIndex;
                _currentIndex++;
                return _pooledGameObjectsOriginalOrder[newIndex];
            }
        }

        protected virtual GameObject FindInactiveObject(string name, List<GameObject> pooledGameObjects)
        {
            for (int i = 0; i < pooledGameObjects.Count; i++)
            {
                if (pooledGameObjects[i].name.Equals(name))
                {
                    if (!pooledGameObjects[i].activeInHierarchy)
                    {
                        return pooledGameObjects[i];
                    }
                }
            }
            return null;
        }

        protected virtual MultipleObjectPoolerObject GetObjectPool(string objectPoolName)
        {
            if (objectPoolName == null) { return null; }

            foreach (MultipleObjectPoolerObject poolerObject in Pool)
            {
                if (objectPoolName.Equals(poolerObject.GameObjectToPool.name)) 
                { 
                    return poolerObject; 
                }
            }
            return null;
        }

        /// <summary>
        /// Resets the current index to the beginning of the pool.
        /// </summary>
        public virtual void ResetCurrentIndex()
        {
            _currentIndex = 0;
        }

        /// <summary>
        /// Gets an object of the specified name from the pool.
        /// </summary>
        /// <returns>The pooled game object of the specified name.</returns>
        /// <param name="searchedName">The name of the object to get from the pool.</param>
        public virtual GameObject GetPooledGameObjectOfType(string searchedName)
        {
            GameObject newObject = FindInactiveObject(searchedName, _pooledGameObjectsOriginalOrder);

            if (newObject != null)
            {
                return newObject;
            }
            else
            {
                // Check if the key exists in the dictionary.
                if (ObjectPoolsByObjectType.ContainsKey(searchedName))
                {
                    MultipleObjectPoolerObject foundObject = GetObjectPool(searchedName);
                    if (foundObject.PoolCanExpand)
                    {
                        GameObject newGameObject = AddOneObjectToThePool(foundObject.GameObjectToPool);
                        return newGameObject;
                    }
                }
                else
                {
                    // Handle the case where the searchedObject is not in the dictionary.
                    Debug.LogError($"{this.GetType()}.GetPooledGameObjectOfType: The object {searchedName} is not in the ObjectPoolsByObjectType dictionary.");
                }
            }

            // If the pool was empty for that object and not allowed to expand, we return nothing.
            return null;
        }

        /// <summary>
        /// Finds an object in the pool based on its name, active or inactive.
        /// Returns null if there's no object by that name in the pool.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="searchedName">The name of the object to find in the pool.</param>
        /// <param name="list">The list of objects to search in.</param>
        public virtual GameObject FindObject(string searchedName, List<GameObject> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                // If we find an object inside the pool that matches the asked type and if that object is inactive right now.
                if (list[i].name.Equals(searchedName))
                {
                    return list[i];
                }
            }
            return null;
        }
    }
}
