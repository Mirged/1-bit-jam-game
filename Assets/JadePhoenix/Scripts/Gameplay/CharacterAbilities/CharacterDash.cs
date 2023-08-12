using JadePhoenix.Tools;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class CharacterDash : CharacterAbility
    {
        public enum DashModes { Fixed, MainMovement, MousePosition }
        public DashModes DashMode = DashModes.MainMovement;

        [Header("Dash Parameters")]
        public Vector3 DashDirection = Vector3.forward;
        public float DashDistance = 6f;
        public float DashDuration = 0.2f;
        public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        public float Cooldown = 1f;

        protected Timer _cooldownTimer;
        protected bool _dashReady = true;
        protected Timer _dashTimer;
        protected Vector3 _dashOrigin;
        protected Vector3 _dashDestination;
        protected Vector3 _newPosition;
        protected bool _dashing;

        protected const string _dashingAnimationParameterName = "Dashing";
        protected const string _idleAnimationParameterName = "Idle";
        protected int _dashingAnimationParameter;
        protected int _idleAnimationParameter;

        protected override void Initialization()
        {
            base.Initialization();
            _dashTimer = new Timer(DashDuration, null, StopDashing);
            _cooldownTimer = new Timer(Cooldown, null, DashReady);
        }

        protected override void HandleInput()
        {
            base.HandleInput();

            if (!AbilityPermitted || _dashing)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && _dashReady)
            {
                Debug.Log($"{this.GetType()}.HandleInput: Starting Dash.", gameObject);
                StartDashing();
            }
        }

        protected virtual void StartDashing()
        {
            _dashOrigin = this.transform.position;
            _movement.ChangeState(CharacterStates.MovementStates.Dashing);
            _controller.FreeMovement = false;
            _dashing = true;
            _dashReady = false;

            switch (DashMode)
            {
                case DashModes.MainMovement:
                    _dashDestination = (Vector2)this.transform.position + _controller.CurrentDirection.normalized * DashDistance;
                    break;

                case DashModes.Fixed:
                    _dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
                    break;

                case DashModes.MousePosition:
                    Camera mainCamera = Camera.main;
                    Vector3 inputPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    inputPosition.z = this.transform.position.z;
                    _dashDestination = this.transform.position + (inputPosition - this.transform.position).normalized * DashDistance;
                    break;
            }

            _dashTimer.StartTimer(true, true);
        }

        public override void ProcessAbility()
        {
            _dashTimer.UpdateTimer();
            _cooldownTimer.UpdateTimer();

            if (_dashing)
            {
                if (_dashTimer.IsRunning)
                {
                    _newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer.ElapsedTime / DashDuration));
                    _controller.MovePosition(_newPosition);
                }
                else
                {
                    StopDashing();
                }
            }
        }

        public virtual void StopDashing()
        {
            _movement.ChangeState(CharacterStates.MovementStates.Falling);
            _cooldownTimer.StartTimer(true, true);
            _controller.FreeMovement = true;
            _controller.Rigidbody.velocity = Vector2.zero;
            _dashing = false;
        }

        protected virtual void DashReady()
        {
            _dashReady = true;
        }

        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_dashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashingAnimationParameter);
            RegisterAnimatorParameter(_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
        }

        public override void UpdateAnimator()
        {
            AnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, _movement.CurrentState == CharacterStates.MovementStates.Dashing, _character.AnimatorParameters);
            AnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, _movement.CurrentState == CharacterStates.MovementStates.Idle, _character.AnimatorParameters);
        }
    }
}
