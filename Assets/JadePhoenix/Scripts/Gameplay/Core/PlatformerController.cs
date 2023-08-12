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

        [ReadOnly, Tooltip("The speed at which the character is moving.")]
        public Vector2 Speed;

        [ReadOnly, Tooltip("The current velocity of the character.")]
        public Vector2 Velocity;

        [ReadOnly, Tooltip("The velocity of the character in the last frame.")]
        public Vector2 VelocityLastFrame;

        [ReadOnly, Tooltip("The current acceleration of the character.")]
        public Vector2 Acceleration;

        [ReadOnly, Tooltip("The current movement direction and magnitude.")]
        public Vector2 CurrentMovement;

        [ReadOnly, Tooltip("The current direction of the character's movement.")]
        public Vector2 CurrentDirection;

        [Tooltip("The friction affecting the character's movement.")]
        public float Friction;

        [ReadOnly, Tooltip("Additional force applied to the character.")]
        public Vector2 AddedForce;

        [Tooltip("If true, the character can move freely; otherwise, movement is restricted.")]
        public bool FreeMovement = true;

        [Header("Layer Masks")]
        [Tooltip("Layer mask to define what layers are considered as obstacles.")]
        public LayerMask ObstaclesLayerMask;

        [Header("Ground Checking")]
        [Tooltip("Offset from the character's center to the start of the ground check ray.")]
        public Vector2 GroundCheckStartOffset;

        [Tooltip("Direction of the ground check ray.")]
        public Vector2 GroundCheckDirection = Vector2.down;

        [Tooltip("Length of the ground check ray.")]
        public float GroundCheckLength = 0.2f;

        [Tooltip("Width between the two rays.")]
        public float GroundCheckWidth = 0.5f;

        [Tooltip("Layer mask to define what layers are considered as ground.")]
        public LayerMask GroundLayerMask;

        [Header("Gravity Control")]
        [Tooltip("Gravity when character is ascending.")]
        public float JumpingGravityMultiplier = 0.5f; 

        [Tooltip("Gravity when character is descending.")]
        public float FallingGravityMultiplier = 1.5f; 

        public Rigidbody2D Rigidbody { get { return _rigidBody; } }
        public Collider2D Collider { get { return _collider; } }
        public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }
        public bool IsGrounded { get { return _isGrounded; } }
        public bool IsFalling { get { return _rigidBody.velocity.y <= 0 && !_isGrounded; } }
        public bool IsJumping { get; set; }

        #endregion

        protected Vector2 _positionLastFrame;
        protected Vector2 _impact;
        protected Rigidbody2D _rigidBody;
        protected Collider2D _collider;
        protected SpriteRenderer _spriteRenderer;
        protected bool _isGrounded;
        protected const float _gravity = 9.18f;

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
            CheckGround();

            ApplyImpact();

            HandleMovement();
        }

        private void OnDrawGizmos()
        {
            Vector2 rayOrigin = (Vector2)transform.position + GroundCheckStartOffset;
            Vector2 rayOriginLeft = rayOrigin - new Vector2(GroundCheckWidth / 2, 0);
            Vector2 rayOriginRight = rayOrigin + new Vector2(GroundCheckWidth / 2, 0);
            Vector2 rayEndLeft = rayOriginLeft + GroundCheckDirection * GroundCheckLength;
            Vector2 rayEndRight = rayOriginRight + GroundCheckDirection * GroundCheckLength;

            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(rayOriginLeft, rayEndLeft);
            Gizmos.DrawLine(rayOriginRight, rayEndRight);
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
        /// Checks if the character is on the ground using a raycast.
        /// </summary>
        protected virtual void CheckGround()
        {
            Vector2 rayOrigin = (Vector2)transform.position + GroundCheckStartOffset;
            Vector2 rayOriginLeft = rayOrigin - new Vector2(GroundCheckWidth / 2, 0);
            Vector2 rayOriginRight = rayOrigin + new Vector2(GroundCheckWidth / 2, 0);

            RaycastHit2D hitLeft = Physics2D.Raycast(rayOriginLeft, GroundCheckDirection, GroundCheckLength, GroundLayerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(rayOriginRight, GroundCheckDirection, GroundCheckLength, GroundLayerMask);

            _isGrounded = hitLeft.collider != null || hitRight.collider != null;
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

        protected virtual void HandleMovement()
        {
            if (!FreeMovement) { return; }

            Vector2 horizontalMovement = new Vector2(CurrentMovement.x, 0);

            if (Friction > 1)
            {
                horizontalMovement /= Friction;
            }

            if (Friction > 0 && Friction < 1)
            {
                horizontalMovement = Vector2.Lerp(Vector2.right * Speed, horizontalMovement, Time.deltaTime * Friction);
            }

            Vector2 newMovement = _rigidBody.position + (horizontalMovement + AddedForce) * Time.fixedDeltaTime;

            if (_isGrounded && !IsJumping)
            {
                // Reset vertical velocity when grounded
                _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, -1);
            }
            else
            {
                // Determine gravity multiplier
                float gravityMultiplier = _rigidBody.velocity.y > 0 && IsJumping ? JumpingGravityMultiplier : FallingGravityMultiplier;

                // Apply custom gravity
                float gravityForce = _gravity * gravityMultiplier * Time.fixedDeltaTime;
                _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, _rigidBody.velocity.y - gravityForce);

                // Preserve vertical velocity (including custom gravity)
                newMovement.y += _rigidBody.velocity.y * Time.fixedDeltaTime;
            }

            _rigidBody.MovePosition(newMovement);
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
