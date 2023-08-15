using JadePhoenix.Tools;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    [RequireComponent(typeof(Weapon))]
    public class WeaponAim : MonoBehaviour
    {
        public enum AimControls { Off, Mouse, Script }

        #region Variables

        [Header("Control Mode")]
        [Tooltip("Determines the aiming control method.")]
        public AimControls AimControl = AimControls.Off;

        [Header("Weapon Rotation")]
        [Tooltip("Speed of weapon rotation, 0 is instant.")]
        public float WeaponRotationSpeed = 1f;

        [Tooltip("Minimum angle for weapon rotation.")]
        public float MinimumAngle = -180f;

        [Tooltip("Maximum angle for weapon rotation.")]
        public float MaximumAngle = 180f;

        [Header("CameraTarget")]
        [Tooltip("Should the camera target move towards the reticle?")]
        public bool MoveCameraTargetTowardsReticle = false;

        [Range(0f, 1f), Tooltip("Offset for the camera target.")]
        public float CameraTargetOffset = 0.3f;

        [Tooltip("Maximum distance for the camera target.")]
        public float CameraTargetMaxDistance = 10f;

        [Tooltip("Movement speed of the camera target.")]
        public float CameraTargetSpeed = 5f;

        public float CurrentAngleAbsolute { get; private set; }
        public float CurrentAngle { get; private set; }

        protected Weapon _weapon;
        protected Vector2 _currentAim = Vector2.zero;
        protected Quaternion _lookRotation;
        protected Vector2 _direction;
        protected float _additionalAngle;
        protected Quaternion _initialRotation;
        protected Vector2 _reticlePosition;
        protected Vector2 _newCamTargetPosition;
        protected Vector2 _newCamTargetDirection;
        protected Vector2 _inputMovement;
        protected Camera _mainCamera;

        #endregion

        #region Unity Lifecycle

        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Update()
        {
            GetCurrentAim();
            DetermineWeaponRotation();
            MoveCameraTarget();
        }

        protected virtual void LateUpdate()
        {
            ResetAdditionalAngle();
        }

        #endregion

        /// <summary>
        /// Initializes necessary variables and references.
        /// </summary>
        protected virtual void Initialization()
        {
            _weapon = GetComponent<Weapon>();
            _initialRotation = transform.rotation;
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Determines the current aiming direction based on the selected AimControl.
        /// </summary>
        protected virtual void GetCurrentAim()
        {
            if (_weapon.Owner == null) { return; }

            switch (AimControl)
            {
                case AimControls.Off:
                    GetOffAim();
                    break;

                case AimControls.Script:
                    GetScriptAim();
                    break;

                case AimControls.Mouse:
                    GetMouseAim();
                    break;
            }
        }

        /// <summary>
        /// Sets the aiming direction to the right when the AimControl is set to 'Off'.
        /// </summary>
        protected virtual void GetOffAim()
        {
            _currentAim = Vector2.right;
            _direction = Vector2.right;
        }

        /// <summary>
        /// Sets the aiming direction based on a given point in the script.
        /// </summary>
        protected virtual void GetScriptAim()
        {
            _direction = -(Vector2)transform.position - _currentAim;
        }

        /// <summary>
        /// Sets the aiming direction towards the mouse position.
        /// </summary>
        protected virtual void GetMouseAim()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
            _direction = (Vector2)mouseWorldPosition - (Vector2)transform.position;
            _reticlePosition = (Vector2)mouseWorldPosition;
            _currentAim = _direction;
        }

        /// <summary>
        /// Determines the rotation of the weapon based on the current aiming direction.
        /// </summary>
        protected virtual void DetermineWeaponRotation()
        {
            if (_currentAim != Vector2.zero)
            {
                CurrentAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;
                CurrentAngle += _additionalAngle;
                CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
                _lookRotation = Quaternion.Euler(0, 0, CurrentAngle);
                RotateWeapon(_lookRotation);
            }
            else
            {
                CurrentAngle = 0f;
                RotateWeapon(_initialRotation);
            }
        }

        /// <summary>
        /// Rotates the weapon towards the desired direction.
        /// </summary>
        /// <param name="newRotation">The desired rotation for the weapon.</param>
        protected virtual void RotateWeapon(Quaternion newRotation)
        {
            if (WeaponRotationSpeed == 0f)
            {
                transform.rotation = newRotation;
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, WeaponRotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Moves the camera target towards the aiming reticle.
        /// </summary>
        protected virtual void MoveCameraTarget()
        {
            if (MoveCameraTargetTowardsReticle && (_weapon.Owner != null))
            {
                _newCamTargetPosition = _reticlePosition;
                _newCamTargetDirection = _newCamTargetPosition - (Vector2)transform.position;
                if (_newCamTargetDirection.magnitude > CameraTargetMaxDistance)
                {
                    _newCamTargetDirection.Normalize();
                    _newCamTargetDirection *= CameraTargetMaxDistance;
                }
                _newCamTargetPosition = (Vector2)transform.position + _newCamTargetDirection;
                _newCamTargetPosition = Vector2.Lerp((Vector2)_weapon.Owner.CameraTarget.transform.position,
                                                     Vector2.Lerp((Vector2)transform.position, _newCamTargetPosition, CameraTargetOffset),
                                                     Time.deltaTime * CameraTargetSpeed);
                _weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
            }
        }

        /// <summary>
        /// Resets any additional angle applied to the weapon's rotation.
        /// </summary>
        protected virtual void ResetAdditionalAngle()
        {
            _additionalAngle = 0;
        }

        #region Public Methods

        /// <summary>
        /// Sets the direction in which the weapon is aiming.
        /// </summary>
        /// <param name="newAim">New aim direction in 2D space.</param>
        public void SetCurrentAim(Vector2 newAim)
        {
            _currentAim = newAim;
        }

        #endregion
    }
}

