using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class CharacterJump : CharacterAbility
    {
        [Header("Jump Parameters")]
        public float JumpForce = 10f; // The force applied to the character when jumping.
        public KeyCode JumpKey = KeyCode.Space; // The key used to initiate a jump.

        // Parameters to track the state of the jump
        protected bool _jumpInput;

        // Animator hash
        protected int _jumpingParameter;

        protected override void HandleInput()
        {
            // Check for jump input
            _jumpInput = Input.GetKeyDown(JumpKey);
        }

        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter("Jumping", AnimatorControllerParameterType.Bool, out _jumpingParameter);
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();

            if (AbilityPermitted 
            && _jumpInput
            && _movement.CurrentState != CharacterStates.MovementStates.Jumping
            && _movement.CurrentState != CharacterStates.MovementStates.Falling
            && _movement.CurrentState != CharacterStates.MovementStates.Dashing)
            {
                Jump();
            }

            // Change our State based on whether the controller is falling.
            if (_controller.IsFalling)
            {
                _movement.ChangeState(CharacterStates.MovementStates.Falling);
                _controller.IsJumping = false;
            }

            if (_controller.IsGrounded && _movement.CurrentState == CharacterStates.MovementStates.Falling)
            {
                _movement.ChangeState(CharacterStates.MovementStates.Idle);
            }
        }

        protected virtual void Jump()
        {
            // Apply jump force
            _controller.Rigidbody.velocity = new Vector2(_controller.Rigidbody.velocity.x, JumpForce);
            _movement.ChangeState(CharacterStates.MovementStates.Jumping);

            // Notify PlatformerController to use jumping gravity
            _controller.IsJumping = true;
        }

        public override void UpdateAnimator()
        {
            _animator.SetBool(_jumpingParameter, _movement.CurrentState == CharacterStates.MovementStates.Jumping);
        }
    }
}
