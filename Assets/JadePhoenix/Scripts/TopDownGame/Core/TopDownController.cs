using UnityEngine;

namespace JadePhoenix.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class TopDownController : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        public Vector3 Speed;
        public Vector3 Velocity;
        public Vector3 VelocityLastFrame;
        public Vector3 Acceleration;
        public Vector3 CurrentMovement;
        public Vector3 CurrentDirection;
        public float Friction;
        public Vector3 AddedForce;
        public bool FreeMovement = true;

        [Header("Layer Masks")]
        public LayerMask ObstaclesLayerMask;

        public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

        #endregion

        protected Vector3 _positionLastFrame;
        protected Vector3 _impact;
        protected Rigidbody2D _rigidBody;
        protected Collider2D _collider;
        protected Vector3 _orientedMovement;
        protected SpriteRenderer _spriteRenderer;

        protected virtual void Awake()
        {
            CurrentDirection = transform.forward;

            Initialization();
        }

        protected virtual void Initialization()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            _spriteRenderer = GetComponent<Character>().Model.GetComponent<SpriteRenderer>();
        }

        #region UNITY LIFECYCLE

        protected virtual void Update()
        {
            DetermineDirection();
            Velocity = _rigidBody.velocity;
            Acceleration = (_rigidBody.velocity - (Vector2)VelocityLastFrame) / Time.fixedDeltaTime;
        }

        protected virtual void LateUpdate()
        {
            ComputeSpeed();
            VelocityLastFrame = _rigidBody.velocity;
        }

        protected virtual void FixedUpdate()
        {
            ApplyImpact();

            if (!FreeMovement) { return; }

            if (Friction > 1)
            {
                CurrentMovement = CurrentMovement / Friction;
            }

            // if we have a low friction we lerp the speed accordingly
            if (Friction > 0 && Friction < 1)
            {
                CurrentMovement = Vector3.Lerp(Speed, CurrentMovement, Time.deltaTime * Friction);
            }

            Vector2 newMovement = _rigidBody.position + (Vector2)(CurrentMovement + AddedForce) * Time.fixedDeltaTime;

            _rigidBody.MovePosition(newMovement);
        }

        #endregion

        protected virtual void DetermineDirection()
        {
            if (CurrentMovement != Vector3.zero)
            {
                CurrentDirection = CurrentMovement.normalized;
            }
        }

        protected virtual void ComputeSpeed()
        {
            Speed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
            // we round the speed to 2 decimals
            Speed.x = Mathf.Round(Speed.x * 100f) / 100f;
            Speed.y = Mathf.Round(Speed.y * 100f) / 100f;
            Speed.z = Mathf.Round(Speed.z * 100f) / 100f;
            _positionLastFrame = this.transform.position;
        }

        protected virtual void ApplyImpact()
        {
            if (_impact.magnitude > 0.2f)
            {
                _rigidBody.AddForce(_impact);
            }
            _impact = Vector3.Lerp(_impact, Vector3.zero, 5f * Time.deltaTime);
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Use this to apply an impact to a controller, moving it in the specified direction at the specified force
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public virtual void Impact(Vector3 direction, float force)
        {
            direction = direction.normalized;
            _impact += direction.normalized * force;
        }

        /// <summary>
        /// Sets the current movement
        /// </summary>
        /// <param name="movement"></param>
        public virtual void SetMovement(Vector3 movement)
        {
            _orientedMovement = movement;
            _orientedMovement.y = _orientedMovement.z;
            _orientedMovement.z = 0f;
            CurrentMovement = _orientedMovement;
        }

        /// <summary>
        /// Adds a force of the specified vector
        /// </summary>
        /// <param name="movement"></param>
        public virtual void AddForce(Vector3 movement)
        {
            Impact(movement.normalized, movement.magnitude);
        }

        /// <summary>
        /// Tries to move to the specified position
        /// </summary>
        /// <param name="newPosition"></param>
        public virtual void MovePosition(Vector3 newPosition)
        {
            _rigidBody.MovePosition(newPosition);
        }

        /// <summary>
        /// Sets this rigidbody as kinematic
        /// </summary>
        /// <param name="state"></param>
        public virtual void SetKinematic(bool state)
        {
            _rigidBody.isKinematic = state;
        }

        /// <summary>
        /// Enables the collider
        /// </summary>
        public virtual void CollisionsOn()
        {
            _collider.enabled = true;
        }

        /// <summary>
        /// Disables the collider
        /// </summary>
        public virtual void CollisionsOff()
        {
            _collider.enabled = false;
        }

        /// <summary>
        /// Resets all values for this controller
        /// </summary>
        public virtual void Reset()
        {
            _impact = Vector3.zero;
            Speed = Vector3.zero;
            Velocity = Vector3.zero;
            VelocityLastFrame = Vector3.zero;
            Acceleration = Vector3.zero;
            CurrentMovement = Vector3.zero;
            CurrentDirection = Vector3.zero;
            AddedForce = Vector3.zero;
        }

        #endregion
    }
}
