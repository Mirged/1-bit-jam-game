using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    /// <summary>
    /// Allows a character to aim based on the mouse position in a 2D environment.
    /// </summary>
    public class CharacterAiming : CharacterAbility
    {
        [Tooltip("The speed at which the character rotates to face the aim direction.")]
        public float RotationSpeed = 10f;
        [Tooltip("A model to use instead of the owner's CharacterModel. Can be safely left null.")]
        public GameObject OverrideModel;

        protected Camera _mainCamera;
        protected Vector2 _mousePosition;
        protected Vector2 _direction;
        protected Vector2 _currentAim;

        /// <summary>
        /// Initialization function for setting up character aiming.
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            _mainCamera = Camera.main;

            if (OverrideModel == null) { return; }
            _model = OverrideModel;
        }

        /// <summary>
        /// Processes the aiming ability.
        /// </summary>
        public override void ProcessAbility()
        {
            GetMouseAim();
            RotateTowardsMouse();
        }

        /// <summary>
        /// Gets the mouse position and calculates the direction in which the character should aim.
        /// </summary>
        public virtual void GetMouseAim()
        {
            _mousePosition = Input.mousePosition;
            Vector3 worldMousePos = _mainCamera.ScreenToWorldPoint(_mousePosition);
            _direction = worldMousePos;

            // This gets the direction from the character's position to the mouse position
            _currentAim = (_direction - (Vector2)transform.position).normalized;
        }

        /// <summary>
        /// Rotates the character model to face the direction of the mouse.
        /// </summary>
        public virtual void RotateTowardsMouse()
        {
            // Calculate the angle needed to point towards the direction
            float angle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;

            // Smoothly interpolate current rotation towards the target rotation
            _model.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}

