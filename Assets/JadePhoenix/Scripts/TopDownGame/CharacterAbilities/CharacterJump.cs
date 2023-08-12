using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class CharacterJump : CharacterAbility
    {
        [Header("Jump Parameters")]
        public float JumpForce = 10f; // The force applied to the character when jumping.
        public KeyCode JumpKey = KeyCode.Space; // The key used to initiate a jump.

        // Parameters to track the state of the jump
        private bool _jumpInput;

        // Animator hash
        private int _jumpingParameter;

        protected override void HandleInput()
        {
            // Check for jump input
            _jumpInput = Input.GetKeyDown(JumpKey);
            //Debug.Log($"{this.GetType()}.HandleInput: JumpInput = {Input.GetKeyDown(JumpKey)}.", gameObject);
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
            && _movement.CurrentState != CharacterStates.MovementStates.Falling)
            {
                Jump();
            }

            // Call the method to handle gravity in the PlatformerController
            HandleGravity();

            if (_controller.IsGrounded && _movement.CurrentState == CharacterStates.MovementStates.Falling)
            {
                _movement.ChangeState(CharacterStates.MovementStates.Idle);
            }
        }

        private void Jump()
        {
            // Apply jump force
            _controller.Rigidbody.velocity = new Vector2(_controller.Rigidbody.velocity.x, JumpForce);
            _movement.ChangeState(CharacterStates.MovementStates.Jumping);

            // Notify PlatformerController to use jumping gravity
            _controller.IsJumping = true;
        }

        private void HandleGravity()
        {
            // Detect if the character has reached the peak of the jump
            if (_movement.CurrentState == CharacterStates.MovementStates.Jumping && _controller.Rigidbody.velocity.y <= 0)
            {
                _movement.ChangeState(CharacterStates.MovementStates.Falling);

                // Notify PlatformerController to use falling gravity
                _controller.IsJumping = false;
            }
        }

        public override void UpdateAnimator()
        {
            _animator.SetBool(_jumpingParameter, _movement.CurrentState == CharacterStates.MovementStates.Jumping);
        }
    }
}
