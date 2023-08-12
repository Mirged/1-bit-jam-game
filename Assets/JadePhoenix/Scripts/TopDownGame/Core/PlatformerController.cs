using UnityEngine;

namespace JadePhoenix.Gameplay
{
    /// <summary>
    /// PlatformerController is responsible for controlling the physics-based movement of a character in a 2D platformer game.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlatformerController : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        [Tooltip("The speed at which the character moves.")]
        public Vector2 Speed;

        [Tooltip("The current velocity of the character.")]
        public Vector2 Velocity;

        [Tooltip("The velocity of the character in the last frame.")]
        public Vector2 VelocityLastFrame;

        [Tooltip("The current acceleration of the character.")]
        public Vector2 Acceleration;

        [Tooltip("The current movement direction and magnitude.")]
        public Vector2 CurrentMovement;

        [Tooltip("The current direction of the character's movement.")]
        public Vector2 CurrentDirection;

        [Tooltip("The friction affecting the character's movement.")]
        public float Friction;

        [Tooltip("Additional force applied to the character.")]
        public Vector2 AddedForce;

        [Tooltip("If true, the character can move freely; otherwise, movement is restricted.")]
        public bool FreeMovement = true;

        [Header("Layer Masks")]
        [Tooltip("Layer mask to define what layers are considered as obstacles.")]
        public LayerMask ObstaclesLayerMask;

        #endregion

        protected Vector2 _positionLastFrame;
        protected Vector2 _impact;
        protected Rigidbody2D _rigidBody;
        protected Collider2D _collider;
        protected SpriteRenderer _spriteRenderer;

        #region UNITY LIFECYCLE

        /// <summary>
        /// Initialization code for Awake.
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
        }

        /// <summary>
        /// Updates the character's velocity and acceleration.
        /// </summary>
        protected virtual void Update()
        {
            DetermineDirection();
            Velocity = _rigidBody.velocity;
            Acceleration = (_rigidBody.velocity - VelocityLastFrame) / Time.fixedDeltaTime;
        }

        /// <summary>
        /// Computes the character's speed.
        /// </summary>
        protected virtual void LateUpdate()
        {
            ComputeSpeed();
            VelocityLastFrame = _rigidBody.velocity;
        }

        /// <summary>
        /// Handles the physics-based movement of the character.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            ApplyImpact();

            if (!FreeMovement) { return; }

            if (Friction > 1)
            {
                CurrentMovement /= Friction;
            }

            if (Friction > 0 && Friction < 1)
            {
                CurrentMovement = Vector2.Lerp(Vector2.one * Speed, CurrentMovement, Time.deltaTime * Friction);
            }

            Vector2 newMovement = _rigidBody.position + (CurrentMovement + AddedForce) * Time.fixedDeltaTime;

            _rigidBody.MovePosition(newMovement);
        }

        #endregion

        /// <summary>
        /// Initialization of variables and components.
        /// </summary>
        protected virtual void Initialization()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<Character>().Model.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Computes the direction of the controller based on the CurrentMovement variable.
        /// </summary>
        protected virtual void DetermineDirection()
        {
            if (CurrentMovement != Vector2.zero)
            {
                CurrentDirection = CurrentMovement.normalized;
            }
        }

        /// <summary>
        /// Computes the character's speed.
        /// </summary>
        protected virtual void ComputeSpeed()
        {
            Speed = ((Vector2)this.transform.position - _positionLastFrame) / Time.deltaTime;
            _positionLastFrame = this.transform.position;
        }

        /// <summary>
        /// Applies an impact to the character's movement.
        /// </summary>
        protected virtual void ApplyImpact()
        {
            if (_impact.magnitude > 0.2f)
            {
                _rigidBody.AddForce(_impact);
            }
            _impact = Vector2.Lerp(_impact, Vector2.zero, 5f * Time.deltaTime);
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Applies an impact to the character, moving it in the specified direction with the specified force.
        /// </summary>
        /// <param name="direction">Direction of the impact.</param>
        /// <param name="force">Magnitude of the impact force.</param>
        public virtual void Impact(Vector2 direction, float force)
        {
            direction = direction.normalized;
            _impact += direction * force;
        }

        /// <summary>
        /// Sets the current movement direction and magnitude.
        /// </summary>
        /// <param name="movement">Desired movement vector.</param>
        public virtual void SetMovement(Vector2 movement)
        {
            CurrentMovement = movement;
        }

        /// <summary>
        /// Adds a force of the specified vector.
        /// </summary>
        /// <param name="movement">The force to be added.</param>
        public virtual void AddForce(Vector2 movement)
        {
            Impact(movement.normalized, movement.magnitude);
        }

        /// <summary>
        /// Tries to move the character to the specified position.
        /// </summary>
        /// <param name="newPosition">The desired position.</param>
        public virtual void MovePosition(Vector2 newPosition)
        {
            _rigidBody.MovePosition(newPosition);
        }

        /// <summary>
        /// Sets the character's Rigidbody2D as kinematic or non-kinematic.
        /// </summary>
        /// <param name="state">The desired kinematic state.</param>
        public virtual void SetKinematic(bool state)
        {
            _rigidBody.isKinematic = state;
        }

        /// <summary>
        /// Enables the character's collider.
        /// </summary>
        public virtual void CollisionsOn()
        {
            _collider.enabled = true;
        }

        /// <summary>
        /// Disables the character's collider.
        /// </summary>
        public virtual void CollisionsOff()
        {
            _collider.enabled = false;
        }

        /// <summary>
        /// Resets all values for the character's movement.
        /// </summary>
        public virtual void Reset()
        {
            _impact = Vector2.zero;
            Speed = Vector2.zero;
            Velocity = Vector2.zero;
            VelocityLastFrame = Vector2.zero;
            Acceleration = Vector2.zero;
            CurrentMovement = Vector2.zero;
            AddedForce = Vector2.zero;
        }

        #endregion
    }
}
