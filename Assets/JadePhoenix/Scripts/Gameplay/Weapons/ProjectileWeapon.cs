using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JadePhoenix.Tools;

namespace JadePhoenix.Gameplay
{
    public class ProjectileWeapon : Weapon
    {
        [Header("Spawn")]
        [Tooltip("The offset position at which the projectile will spawn.")]
        public Vector3 ProjectileSpawnOffset = Vector3.zero;

        [Tooltip("The number of projectiles to spawn per shot.")]
        public int ProjectilesPerShot = 1;

        [Tooltip("The spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile.")]
        public Vector3 Spread = Vector3.zero;

        [Tooltip("Whether or not the spread should be random (if not it'll be equally distributed).")]
        public bool RandomSpread = true;

        [ReadOnly, Tooltip("The projectile's spawn position.")]
        public Vector3 SpawnPosition = Vector3.zero;

        /// <summary>
        /// The object pooler used to spawn projectiles.
        /// </summary>
        public ObjectPooler ObjectPooler { get; set; }

        protected Vector3 _randomSpreadDirection;
        protected bool _poolInitialized = false;
        protected Transform _projectileSpawnTransform;

        /// <summary>
        /// Initialize this weapon.
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();

            if (!_poolInitialized)
            {
                if (GetComponent<ObjectPooler>() != null)
                {
                    ObjectPooler = GetComponent<ObjectPooler>();
                }
                if (ObjectPooler == null)
                {
                    Debug.LogWarning(this.name + " : no object pooler is attached to this Projectile Weapon, it won't be able to shoot anything.");
                    return;
                }
                _poolInitialized = true;
            }
        }

        /// <summary>
        /// Called everytime the weapon is used.
        /// </summary>
        public override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();

            for (int i = 0; i < ProjectilesPerShot; i++)
            {
                //Debug.Log($"{this.GetType()}.WeaponUse: Spawning Projectile.", gameObject);
                SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
            }
        }

        /// <summary>
        /// Spawns a new object and positions/resizes it
        /// </summary>
        public virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

            if (nextGameObject == null) { return null; }
            if (nextGameObject.GetComponent<PoolableObject>() == null)
            {
                throw new Exception($"{gameObject.name} is trying to spawn objects that don't have a PoolableObject component.");
            }

            // Position the object.
            nextGameObject.transform.position = spawnPosition;
            if(_projectileSpawnTransform != null)
            {
                nextGameObject.transform.position = _projectileSpawnTransform.position;
            }

            // Set the direction.
            Projectile projectile = nextGameObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetWeapon(this);
                if (Owner != null)
                {
                    projectile.SetOwner(Owner.gameObject);
                }
            }

            // Activate the object
            nextGameObject.gameObject.SetActive(true);

            if (projectile != null)
            {
                if (RandomSpread)
                {
                    _randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
                    _randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
                    _randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
                }
                else
                {
                    if (totalProjectiles > 1)
                    {
                        _randomSpreadDirection.x = JP_Math.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.x, Spread.x);
                        _randomSpreadDirection.y = JP_Math.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.y, Spread.y);
                        _randomSpreadDirection.z = JP_Math.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.z, Spread.z);
                    }
                    else
                    {
                        _randomSpreadDirection = Vector3.zero;
                    }
                }

                Quaternion spread = Quaternion.Euler(_randomSpreadDirection);

                projectile.SetDirection(spread * transform.right, transform.rotation);
            }

            if (triggerObjectActivation && nextGameObject.GetComponent<PoolableObject>() != null)
            {
                nextGameObject.GetComponent<PoolableObject>().TriggerOnSpawnComplete();
            }

            return nextGameObject;
        }

        /// <summary>
        /// Sets a forced projectile spawn position.
        /// </summary>
        /// <param name="newSpawnTransform"></param>
        public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
        {
            _projectileSpawnTransform = newSpawnTransform;
        }

        /// <summary>
        /// Determines the spawn position based on the spawn offset.
        /// </summary>
        public virtual void DetermineSpawnPosition()
        {
            SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
        }

        /// <summary>
        /// When the weapon is selected, draws a circle at the spawn's position
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            DetermineSpawnPosition();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
        }
    }
}

