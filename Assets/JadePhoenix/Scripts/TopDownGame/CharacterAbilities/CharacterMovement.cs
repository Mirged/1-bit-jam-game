using JadePhoenix.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.TopDownGame
{
    [RequireComponent(typeof(TopDownController))]
    public class CharacterMovement : CharacterAbility
    {
        public float MovementSpeed;
        public bool MovementForbidden;

        [Header("Settings")]
        public bool InputAuthorized = true;

        [Header("Speed")]
        public float WalkSpeed = 6f;
        public bool ShouldSetMovement = true;
        public float IdleThreshold = 0.05f;

        [Header("Acceleration")]
        /// the acceleration to apply to the current speed / 0f : no acceleration, instant full speed
        public float Acceleration = 10f;
        /// the deceleration to apply to the current speed / 0f : no deceleration, instant stop
        public float Deceleration = 10f;
        public bool InterpolateMovementSpeed = false;
        public float MovementSpeedMultiplier;

        protected float _movementSpeed;
        protected float _horizontalMovement;
        protected float _verticalMovement;
        protected Vector3 _movementVector;
        protected Vector2 _currentInput = Vector2.zero;
        protected Vector2 _normalizedInput;
        protected Vector2 _lerpedInput = Vector2.zero;
        protected float _acceleration = 0f;

        protected const string _walkingAnimationParameterName = "Walking";
        protected const string _idleAnimationParameterName = "Idle";
        protected int _walkingAnimationParameter;
        protected int _idleAnimationParameter;

        protected override void Initialization()
        {
            base.Initialization();
            MovementSpeed = WalkSpeed;
            _movement.ChangeState(CharacterStates.MovementStates.Idle);
            MovementSpeedMultiplier = 1f;
            MovementForbidden = false;
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            HandleMovement();
        }

        protected override void HandleInput()
        {
            if (InputAuthorized)
            {
                _horizontalMovement = _horizontalInput;
                _verticalMovement = _verticalInput;
            }
            else
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
            }
        }

        protected virtual void FixedUpdate()
        {
            HandleMovement();
        }

        protected virtual void HandleMovement()
        {
            if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) { return; }

            if (MovementForbidden)
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
            }

            if ((_controller.CurrentMovement.magnitude > IdleThreshold)
            && (_movement.CurrentState == CharacterStates.MovementStates.Idle))
            {
                _movement.ChangeState(CharacterStates.MovementStates.Walking);
            }

            if (_movement.CurrentState == CharacterStates.MovementStates.Walking
            && _controller.CurrentMovement.magnitude <= IdleThreshold)
            {
                _movement.ChangeState(CharacterStates.MovementStates.Idle);
            }

            if (ShouldSetMovement)
            {
                SetMovement();
            }
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Resets this character's speed
        /// </summary>
        public virtual void ResetSpeed()
        {
            MovementSpeed = WalkSpeed;
        }

        /// <summary>
        /// Moves the controller
        /// </summary>
        public virtual void SetMovement()
        {
            // Reset the movement vector and current input
            _movementVector = Vector3.zero;
            _currentInput = Vector2.zero;

            // Store the horizontal and vertical input values
            _currentInput.x = _horizontalMovement;
            _currentInput.y = _verticalMovement;

            // Normalize the input vector
            _normalizedInput = _currentInput.normalized;

            // Check if acceleration or deceleration is zero
            if ((Acceleration == 0) || (Deceleration == 0))
            {
                // If either acceleration or deceleration is zero, use the current input directly
                _lerpedInput = _currentInput;
            }
            else
            {
                // If either acceleration and deceleration are non-zero
                if (_normalizedInput.magnitude == 0)
                {
                    // If the normalized input magnitude is zero, the input is not active
                    // Gradually decrease acceleration and interpolate the input vector towards zero
                    _acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * Time.deltaTime);
                    _lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _acceleration, Time.deltaTime * Deceleration);
                }
                else
                {
                    // If the normalized input magnitude is non-zero, the input is active
                    // Gradually increase acceleration and clamp the input vector based on acceleration
                    _acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * Time.deltaTime);
                    _lerpedInput = Vector2.ClampMagnitude(_normalizedInput, _acceleration);
                }
            }

            // Assign the x and  components of the lerped input to the movement vector
            _movementVector.x = _lerpedInput.x;
            _movementVector.y = 0;
            _movementVector.z = _lerpedInput.y;

            // Adjust the movement speed based on interpolation and movement speed multiplier
            if (InterpolateMovementSpeed)
            {
                _movementSpeed = Mathf.Lerp(_movementSpeed, MovementSpeed * MovementSpeedMultiplier, _acceleration * Time.deltaTime);
            }
            else
            {
                _movementSpeed = MovementSpeed * MovementSpeedMultiplier;
            }

            // Scale the movement vector by the movement speed
            _movementVector *= _movementSpeed;

            // Clamp the movement vector magnitude to the maximum movement speed
            if (_movementVector.magnitude > MovementSpeed)
            {
                _movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed);
            }

            // Check if both current input and current movement are below the idle threshold
            if ((_currentInput.magnitude <= IdleThreshold) && (_controller.CurrentMovement.magnitude < IdleThreshold))
            {
                // If so, set the movement vector to zero to indicate no movement
                _movementVector = Vector3.zero;
            }

            // Pass the final movement vector to the controller
            _controller.SetMovement(_movementVector);
        }

        public virtual void SetMovement(Vector2 value)
        {
            _horizontalMovement = value.x;
            _verticalMovement = value.y;
        }

        /// <summary>
        /// Sets the horizontal part of the movement
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetHorizontalMovement(float value)
        {
            _horizontalMovement = value;
        }

        /// <summary>
        /// Sets the vertical part of the movement
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetVerticalMovement(float value)
        {
            _verticalMovement = value;
        }

        #endregion

        /// <summary>
        /// On Respawn, resets the speed
        /// </summary>
        protected override void OnRespawn()
        {
            ResetSpeed();
            MovementForbidden = false;
        }

        protected override void InitializeAnimatorParameters()
        {
            //Debug.Log($"{this.GetType()}.InitializeAnimatorParameters: Initializing Movement parameters.", gameObject);

            RegisterAnimatorParameter(_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
            RegisterAnimatorParameter(_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
        }

        public override void UpdateAnimator()
        {
            //Debug.Log($"{this.GetType()}.UpdateAnimator: Updating Animator. Walking = [{_movement.CurrentState == CharacterStates.MovementStates.Walking}]", gameObject);

            AnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, _movement.CurrentState == CharacterStates.MovementStates.Walking, _character.AnimatorParameters);
            AnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, _movement.CurrentState == CharacterStates.MovementStates.Idle, _character.AnimatorParameters);
        }
    }
}
