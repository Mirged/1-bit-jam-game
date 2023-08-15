using JadePhoenix.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class CharacterWeaponHandler : CharacterAbility
    {
        #region VARIABLES

        [Header("Input")]
        public bool ContinuousPress = false;
        public bool GettingHitInterruptsAttack = false;

        [Header("Buffering")]
        public bool BufferInput;
        public bool NewInputExtendsBuffer;
        public float MaxBufferDuration = .25f;

        [Header("Weapon")]
        /// the position from which projectiles will be spawned (can be safely left empty)
        public Transform ProjectileSpawn;
        public Weapon CurrentWeapon;
        [Range(1, 3)] 
        public int WeaponID = 1;

        public WeaponAim WeaponAimComponent { get { return _weaponAim; } }

        protected float _fireTimer = 0f;
        protected WeaponAim _weaponAim;
        protected ProjectileWeapon _projectileWeapon;
        protected float _bufferTimer = 0f;
        protected bool _buffering = false;

        protected const string _aliveAnimationParameterName = "Alive";
        protected const string _idleAnimationParameterName = "Idle";
        protected const string _attackAnimationParameterName = "Attack";
        protected int _aliveAnimationParameter;
        protected int _idleAnimationParameter;
        protected int _attackAnimationParameter;

        #endregion

        protected override void Initialization()
        {
            base.Initialization();
            Setup();
        }

        /// <summary>
        /// Used for extended flexibility.
        /// </summary>
        public virtual void Setup()
        {
            _character = GetComponent<Character>();
            _movement.ChangeState(CharacterStates.MovementStates.Idle);

            if (CurrentWeapon == null)
            {
                CurrentWeapon = transform.Find("Weapon").GetComponent<Weapon>();
            }

            CurrentWeapon.SetOwner(_character, this);
            _weaponAim = CurrentWeapon.gameObject.GetComponent<WeaponAim>();

            _projectileWeapon = CurrentWeapon.gameObject.GetComponent<ProjectileWeapon>();
            if (_projectileWeapon != null)
            {
                _projectileWeapon.SetProjectileSpawnTransform(ProjectileSpawn);
            }
            CurrentWeapon.Initialization();
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            HandleBuffer();
        }

        protected override void HandleInput()
        {
            if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
            {
                return;
            }

            if (_inputManager.ShootButton.State.CurrentState == JP_Input.ButtonStates.ButtonDown)
            {
                ShootStart();
            }

            if (CurrentWeapon != null)
            {
                if ((CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && (_inputManager.ShootButton.State.CurrentState == JP_Input.ButtonStates.ButtonPressed))
                {
                    //Debug.Log($"{this.GetType()}.HandleInput: Weapon TriggerMode is Auto and button held, ShootStart called.", gameObject);
                    ShootStart();
                }
            }

            if (_inputManager.ShootButton.State.CurrentState == JP_Input.ButtonStates.ButtonUp)
            {
                //Debug.Log($"{this.GetType()}.HandleInput: Got button up, ShootStop called.", gameObject);
                ShootStop();
            }

            if (CurrentWeapon != null)
            {
                if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses
                && _inputManager.ShootButton.State.CurrentState == JP_Input.ButtonStates.Off)
                {
                    CurrentWeapon.WeaponInputStop();
                }
            }
        }

        /// <summary>
        /// Triggers an attack if the weapon is idle and an input has been buffered
        /// </summary>
        protected virtual void HandleBuffer()
        {
            if (CurrentWeapon == null)
            {
                return;
            }

            // if we are currently buffering an input and if the weapon is now idle
            if (_buffering && CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
            {
                // and if our buffer is still valid, we trigger an attack
                if (Time.time < _bufferTimer)
                {
                    ShootStart();
                }
                else
                {
                    _buffering = false;
                }
            }
        }

        /// <summary>
        /// Causes the character to start shooting
        /// </summary>
        public virtual void ShootStart()
        {
            // if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
            if ((CurrentWeapon == null)
            || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }

            //  if we've decided to buffer input, and if the weapon is in use right now
            if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle))
            {
                // if we're not already buffering, or if each new input extends the buffer, we turn our buffering state to true
                ExtendBuffer();
            }

            _movement.ChangeState(CharacterStates.MovementStates.Attacking);
            AnimatorExtensions.UpdateAnimatorTrigger(_animator, _attackAnimationParameter, _character.AnimatorParameters);

            CurrentWeapon.WeaponInputStart();
        }

        protected virtual void ExtendBuffer()
        {
            if (!_buffering || NewInputExtendsBuffer)
            {
                _buffering = true;
                _bufferTimer = Time.time + MaxBufferDuration;
            }
        }

        /// <summary>
        /// Causes the character to stop shooting
        /// </summary>
        public virtual void ShootStop()
        {
            _movement.ChangeState(CharacterStates.MovementStates.Idle);

            if (CurrentWeapon == null)
            {
                return;
            }

            if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
            || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
            || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop))
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) 
            && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) 
            && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
            {
                return;
            }

            if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse)
            {
                return;
            }

            CurrentWeapon.TurnWeaponOff();
        }

        protected override void OnHit(GameObject instigator)
        {
            base.OnHit(instigator);
            if (GettingHitInterruptsAttack && (CurrentWeapon != null))
            {
                CurrentWeapon.Interrupt();
            }
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            ShootStop();
        }

        protected override void OnRespawn()
        {
            base.OnRespawn();
            Setup();
        }

        protected override void InitializeAnimatorParameters()
        {
            if (CurrentWeapon == null) { return; }

            RegisterAnimatorParameter(_aliveAnimationParameterName, AnimatorControllerParameterType.Bool, out _aliveAnimationParameter);
            RegisterAnimatorParameter(_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
            RegisterAnimatorParameter(_attackAnimationParameterName + WeaponID, AnimatorControllerParameterType.Bool, out _attackAnimationParameter);
            RegisterAnimatorParameter(_attackAnimationParameterName + WeaponID, AnimatorControllerParameterType.Trigger, out _attackAnimationParameter);
        }

        public override void UpdateAnimator()
        {
            if (CurrentWeapon == null) { return; }

            AnimatorExtensions.UpdateAnimatorBool(_animator, _aliveAnimationParameter, !(_condition.CurrentState == CharacterStates.CharacterConditions.Dead), _character.AnimatorParameters);
            AnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, _movement.CurrentState == CharacterStates.MovementStates.Idle, _character.AnimatorParameters);
            AnimatorExtensions.UpdateAnimatorBool(_animator, _attackAnimationParameter, _movement.CurrentState == CharacterStates.MovementStates.Attacking, _character.AnimatorParameters);
        }
    }
}

