using JadePhoenix.Tools;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class Weapon : MonoBehaviour
    {
        public enum TriggerModes { SemiAuto, Auto }
        public enum WeaponStates
        {
            WeaponIdle,
            WeaponStart,
            WeaponDelayBeforeUse,
            WeaponUse,
            WeaponDelayBetweenUses,
            WeaponStop,
            WeaponReloadStart,
            WeaponReload,
            WeaponReloadStop,
            WeaponInterrupted
        }

        #region Variables

        [Header("Use")]
        [Tooltip("The trigger mode of the weapon (SemiAuto or Auto).")]
        public TriggerModes TriggerMode = TriggerModes.Auto;
        [Tooltip("The delay before the weapon can be used for every shot.")]
        public float DelayBeforeUse = 0f;
        [Tooltip("Determines if the delay before use can be interrupted by releasing the shoot button.")]
        public bool DelayBeforeUseReleaseInterruption = true;
        [Tooltip("The time (in seconds) between two shots.")]
        public float TimeBetweenUses = 1f;
        [Tooltip("Determines if the time between uses can be interrupted by releasing the shoot button.")]
        public bool TimeBetweenUsesReleaseInterruption = true;

        [Header("Magazine")]
        [Tooltip("Determines if the weapon uses magazines or draws ammo from a global pool.")]
        public bool MagazineBased = false;
        [Tooltip("The size of the weapon's magazine.")]
        public int MagazineSize = 30;
        [Tooltip("The time (in seconds) it takes to reload the weapon.")]
        public float ReloadTime = 2f;
        [Tooltip("The amount of ammo consumed every time the weapon fires.")]
        public int AmmoConsumedPerShot = 1;
        [Tooltip("The current amount of ammo loaded inside the weapon.")]
        public int CurrentAmmoLoaded = 0;

        [Header("Recoil")]
        [Tooltip("The force of the weapon's recoil.")]
        public float RecoilForce = 0f;

        [Header("Settings")]
        [Tooltip("Determines if the weapon initializes itself on start or must be manually initialized.")]
        public bool InitializeOnStart = false;
        [Tooltip("Determines if the weapon can be interrupted.")]
        public bool Interruptable = false;

        [Tooltip("The character that owns this weapon.")]
        public Character Owner { get; protected set; }
        [Tooltip("The weapon's owner's CharacterWeaponHandler component.")]
        public CharacterWeaponHandler CharacterWeaponHandler { get; set; }
        [Tooltip("The weapon's state machine.")]
        public StateMachine<WeaponStates> WeaponState;

        protected float _delayBeforeUseCounter = 0f;
        protected float _delayBetweenUsesCounter = 0f;
        protected float _reloadingCounter = 0f;
        protected bool _triggerReleased = false;
        protected bool _reloading = false;
        protected TopDownController _controller;
        protected CharacterMovement _characterMovement;

        #endregion

        #region Unity Lifecycle Methods

        protected virtual void Start()
        {
            if (InitializeOnStart)
            {
                Initialization();
            }
        }

        protected virtual void LateUpdate()
        {
            ProcessWeaponState();
        }

        #endregion

        public virtual void Initialization()
        {
            WeaponState = new StateMachine<WeaponStates>(gameObject, true);
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
            CurrentAmmoLoaded = MagazineSize;
        }

        public virtual void SetOwner(Character owner, CharacterWeaponHandler weaponHandler)
        {
            Owner = owner;
            if (Owner != null)
            {
                CharacterWeaponHandler = weaponHandler;
                _characterMovement = Owner.GetComponent<CharacterMovement>();
                _controller = Owner.GetComponent<TopDownController>();
            }
        }

        #region WEAPON STATE MACHINE CASE METHODS

        protected virtual void ProcessWeaponState()
        {
            if (WeaponState == null) { return; }

            switch (WeaponState.CurrentState)
            {
                case WeaponStates.WeaponIdle:
                    CaseWeaponIdle();
                    break;
                case WeaponStates.WeaponDelayBeforeUse:
                    CaseWeaponDelayBeforeUse();
                    break;
                case WeaponStates.WeaponStart:
                    CaseWeaponStart();
                    break;
                case WeaponStates.WeaponUse:
                    CaseWeaponUse();
                    break;
                case WeaponStates.WeaponDelayBetweenUses:
                    CaseWeaponDelayBetweenUses();
                    break;
                case WeaponStates.WeaponStop:
                    CaseWeaponStop();
                    break;
                case WeaponStates.WeaponReloadStart:
                    CaseWeaponReloadStart();
                    break;
                case WeaponStates.WeaponReload:
                    CaseWeaponReload();
                    break;
                case WeaponStates.WeaponReloadStop:
                    CaseWeaponReloadStop();
                    break;
                case WeaponStates.WeaponInterrupted:
                    CaseWeaponInterrupted();
                    break;
            }
        }

        /// <summary>
        /// If the weapon is idle, we currently do nothing.
        /// </summary>
        protected virtual void CaseWeaponIdle()
        {

        }

        /// <summary>
        /// When the weapon starts, we either wait for the delay or initiate a shot.
        /// </summary>
        protected virtual void CaseWeaponStart()
        {
            if (DelayBeforeUse > 0)
            {
                _delayBeforeUseCounter = DelayBeforeUse;
                WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
            }
            else
            {
                ShootRequest();
            }
        }

        protected virtual void CaseWeaponDelayBeforeUse()
        {
            _delayBeforeUseCounter -= Time.deltaTime;
            if (_delayBeforeUseCounter <= 0)
            {
                ShootRequest();
            }
        }

        protected virtual void CaseWeaponUse()
        {
            WeaponUse();
            _delayBetweenUsesCounter = TimeBetweenUses;
            WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
        }

        protected virtual void CaseWeaponDelayBetweenUses()
        {
            _delayBetweenUsesCounter -= Time.deltaTime;
            if (_delayBetweenUsesCounter <= 0)
            {
                if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
                {
                    ShootRequest();
                }
                else
                {
                    TurnWeaponOff();
                }
            }
        }

        protected virtual void CaseWeaponStop()
        {
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

        protected virtual void CaseWeaponReloadStart()
        {
            _reloadingCounter = ReloadTime;
            WeaponState.ChangeState(WeaponStates.WeaponReload);
        }

        protected virtual void CaseWeaponReload()
        {
            _reloadingCounter -= Time.deltaTime;
            if (_reloadingCounter <= 0)
            {
                WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
            }
        }

        protected virtual void CaseWeaponReloadStop()
        {
            _reloading = false;
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
            CurrentAmmoLoaded = MagazineSize;
        }

        protected virtual void CaseWeaponInterrupted()
        {
            TurnWeaponOff();
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

        #endregion

        #region PUBLIC METHODS

        public virtual void WeaponInputStart()
        {
            if (_reloading) { return; }

            if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
            {
                _triggerReleased = false;
                TurnWeaponOn();
            }
        }

        public virtual void WeaponInputStop()
        {
            if (_reloading)
            {
                return;
            }
            _triggerReleased = true;
        }

        /// <summary>
        /// Makes a request for the weapon to shoot. Takes into account magazine and reloading.
        /// </summary>
        public virtual void ShootRequest()
        {
            if (_reloading) return;
            if (MagazineBased)
            {
                if (CurrentAmmoLoaded > 0)
                {
                    WeaponState.ChangeState(WeaponStates.WeaponUse);
                    CurrentAmmoLoaded -= AmmoConsumedPerShot;
                }
                else
                {
                    InitiateReloadWeapon();
                }
            }
            else
            {
                WeaponState.ChangeState(WeaponStates.WeaponUse);
            }
        }

        /// <summary>
        /// Defines the behavior when the weapon is used (like triggering sfx, vfx, recoil).
        /// </summary>
        public virtual void WeaponUse()
        {
            if (RecoilForce != 0f && _controller != null && Owner != null)
            {
                _controller.Impact(-this.transform.forward, RecoilForce);
            }
        }

        /// <summary>
        /// Handle what happens when the weapon starts
        /// </summary>
        protected virtual void TurnWeaponOn()
        {
            WeaponState.ChangeState(WeaponStates.WeaponStart);

        }

        /// <summary>
        /// Turns the weapon off, primarily ending its current state of operation.
        /// </summary>
        public virtual void TurnWeaponOff()
        {
            if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
            {
                return;
            }
            _triggerReleased = true;
            WeaponState.ChangeState(WeaponStates.WeaponStop);
        }

        /// <summary>
        /// Initiates the reload process for the weapon.
        /// </summary>
        public virtual void InitiateReloadWeapon()
        {
            if (_reloading)
            {
                return;
            }
            WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
            _reloading = true;
        }

        /// <summary>
        /// Interrupts the weapon's current operation if it's interruptible.
        /// </summary>
        public virtual void Interrupt()
        {
            if (Interruptable)
            {
                WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
            }
        }

        #endregion
    }
}
