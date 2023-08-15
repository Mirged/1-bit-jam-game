using JadePhoenix.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class Projectile : PoolableObject
    {
        [Header("Movement")]
        [Tooltip("If true, the projectile will rotate at PreInitialization toward its rotation.")]
        public bool FaceDirection = true;

        [Tooltip("The speed of the object.")]
        public float Speed = 0f;

        [Tooltip("The acceleration of the object over time.")]
        public float Acceleration = 0f;

        [Tooltip("The current direction of the object.")]
        public Vector3 Direction = Vector3.left;

        [Tooltip("If true, the spawner can change the object's direction. Otherwise will use the direction set in the Inspector.")]
        public bool DirectionChangedBySpawner = true;

        [Header("Spawn")]
        [Tooltip("Initial delay before object can be destroyed.")]
        public float InitialInvulnerabilityDuration = 0f;

        [Tooltip("Can the projectile damage its owner?")]
        public bool DamageOwner = false;

        protected Weapon _weapon;
        protected GameObject _owner;
        protected Vector3 _movement;
        protected float _initialSpeed;
        protected DamageOnTouch _damageOnTouch;
        protected WaitForSeconds _initialInvulnerabilityDurationWFS;
        protected Collider _collider;
        protected Rigidbody2D _rigidbody;
        protected Vector3 _initialLocalScale;
        protected bool _shouldMove = true;
        protected Health _health;

        #region UNITY LIFECYCLE

        /// <summary>
        /// On awake, we store the initial speed of the object 
        /// </summary>
        protected virtual void Awake()
        {
            PreInitialization();
        }

        protected virtual void FixedUpdate()
        {
            if (_shouldMove)
            {
                Movement();
            }
        }

        /// <summary>
        /// On enable, we trigger a short invulnerability
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Initialization();
            if (InitialInvulnerabilityDuration > 0)
            {
                StartCoroutine(InitialInvulnerability());
            }

            if (_health != null)
            {
                _health.OnDeath += OnDeath;
            }
        }

        /// <summary>
        /// On disable, we plug our OnDeath method to the health component
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
            }
        }

        #endregion

        /// <summary>
        /// PreInitializes the projectile.
        /// </summary>
        protected virtual void PreInitialization()
        {
            _initialSpeed = Speed;
            _health = GetComponent<Health>();
            _collider = GetComponent<Collider>();
            _damageOnTouch = GetComponent<DamageOnTouch>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _initialInvulnerabilityDurationWFS = new WaitForSeconds(InitialInvulnerabilityDuration);
            _initialLocalScale = transform.localScale;
        }

        /// <summary>
        /// Handles the projectile's initial invincibility
        /// </summary>
        /// <returns>The invulnerability.</returns>
        protected virtual IEnumerator InitialInvulnerability()
        {
            if (_damageOnTouch == null) { yield break; }
            if (_weapon == null) { yield break; }

            _damageOnTouch.ClearIgnoreList();
            _damageOnTouch.IgnoreGameObject(_weapon.Owner.gameObject);
            yield return _initialInvulnerabilityDurationWFS;
            if (DamageOwner)
            {
                _damageOnTouch.StopIgnoringObject(_weapon.Owner.gameObject);
            }
        }

        /// <summary>
        /// Initializes the projectile.
        /// </summary>
        protected virtual void Initialization()
        {
            Speed = _initialSpeed;
            transform.localScale = _initialLocalScale;
            _shouldMove = true;

            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        /// <summary>
        /// On death, we stop our projectile
        /// </summary>
        protected virtual void OnDeath()
        {
            StopAt();
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Handles the projectile's movement, every frame
        /// </summary>
        public virtual void Movement()
        {
            _movement = Direction * (Speed / 10) * Time.deltaTime;
            //Debug.Log($"{this.GetType()}.Movement: Direction: {Direction}, Speed: {Speed}, Movement: {_movement}", gameObject);
            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(this.transform.position + _movement);
            }
            // We apply the acceleration to increase the speed
            Speed += Acceleration * Time.deltaTime;
        }

        /// <summary>
        /// Sets the projectile's direction.
        /// </summary>
        /// <param name="newDirection">New direction.</param>
        /// <param name="newRotation">New rotation.</param>
        public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation)
        {
            if (DirectionChangedBySpawner)
            {
                Direction = newDirection;
            }
            if (FaceDirection)
            {
                transform.rotation = newRotation;
            }
        }

        /// <summary>
        /// Sets the projectile's parent weapon.
        /// </summary>
        /// <param name="newWeapon">New weapon.</param>
        public virtual void SetWeapon(Weapon newWeapon)
        {
            _weapon = newWeapon;
        }

        /// <summary>
        /// Sets the projectile's owner.
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(GameObject newOwner)
        {
            _owner = newOwner;
            DamageOnTouch damageOnTouch = this.gameObject.GetComponent<DamageOnTouch>();
            if (damageOnTouch != null)
            {
                damageOnTouch.Owner = newOwner;
                damageOnTouch.Owner = newOwner;
                if (!DamageOwner)
                {
                    damageOnTouch.IgnoreGameObject(newOwner);
                }
            }
        }

        /// <summary>
        /// On death, disables colliders and prevents movement
        /// </summary>
        public virtual void StopAt()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            _shouldMove = false;
        }

        #endregion
    }
}

