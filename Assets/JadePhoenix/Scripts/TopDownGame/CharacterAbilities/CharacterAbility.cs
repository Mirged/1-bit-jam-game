using JadePhoenix.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    [RequireComponent(typeof(Character))]
    public class CharacterAbility : MonoBehaviour
    {
        #region VARIABLES

        public Character Character { get { return _character; } }
        public StateMachine<CharacterStates.MovementStates> Movement { get { return _movement; } }
        public StateMachine<CharacterStates.CharacterConditions> Condition { get { return _condition; } }

        [Header("Permission")]
        [Tooltip("If true, this ability can perform as usual, if not, it'll be ignored. You can use this to unlock abilities over time for example.")]
        public bool AbilityPermitted = true;
        /// whether or not this ability has been initialized
        public bool AbilityInitialized { get { return _abilityInitialized; } }

        protected Character _character;
        protected TopDownController _controller;
        protected GameObject _model;
        protected Health _health;
        protected CharacterMovement _characterMovement;
        protected InputManager _inputManager;
        protected Animator _animator = null;
        protected CharacterStates _state;
        protected SpriteRenderer _spriteRenderer;
        protected StateMachine<CharacterStates.MovementStates> _movement;
        protected StateMachine<CharacterStates.CharacterConditions> _condition;
        protected bool _abilityInitialized = false;
        protected float _verticalInput;
        protected float _horizontalInput;

        #endregion

        #region UNITY LIFECYCLE

        /// <summary>
        /// On awake we proceed to pre initializing our ability
        /// </summary>
        protected virtual void Awake()
        {
            PreInitialization();
        }

        /// <summary>
        /// On Start(), we call the ability's intialization
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// On enable, we bind our respawn delegate
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_health == null)
            {
                _health = GetComponent<Health>();
            }

            if (_health != null)
            {
                _health.OnRevive += OnRespawn;
                _health.OnDeath += OnDeath;
                _health.OnHit += OnHit;
            }
        }

        /// <summary>
        /// On disable, we unbind our respawn delegate
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnRevive -= OnRespawn;
                _health.OnDeath -= OnDeath;
                _health.OnHit -= OnHit;
            }
        }

        #endregion

        protected virtual void PreInitialization()
        {
            _character = GetComponent<Character>();
            BindAnimator();
        }

        protected virtual void Initialization()
        {
            BindAnimator();

            _controller = GetComponent<TopDownController>();
            _health = GetComponent<Health>();
            _characterMovement = _character.GetAbility<CharacterMovement>();

            _model = _character.Model;
            _spriteRenderer = _model.GetComponent<SpriteRenderer>();
            _inputManager = _character.LinkedInputManager;
            _state = _character.CharacterState;
            _movement = _character.MovementState;
            _condition = _character.ConditionState;

            _abilityInitialized = true;
        }

        protected virtual void BindAnimator()
        {
            if (_character.Animator == null)
            {
                _character.AssignAnimator();
            }

            _animator = _character.Animator;

            if (_animator != null)
            {
                InitializeAnimatorParameters();
            }
        }

        /// <summary>
        /// Internal method to check if an input manager is present or not
        /// </summary>
        protected virtual void InternalHandleInput()
        {
            if (_inputManager == null) { return; }
            _horizontalInput = _inputManager.PrimaryMovement.x;
            _verticalInput = _inputManager.PrimaryMovement.y;
            HandleInput();
        }

        /// <summary>
        /// Changes the reference to the input manager with the one set in parameters
        /// </summary>
        public virtual void SetInputManager(InputManager newInputManager)
        {
            _inputManager = newInputManager;
        }

        /// <summary>
        /// Registers a new animator parameter to the list
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterType">Parameter type.</param>
        protected virtual void RegisterAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType, out int parameter)
        {
            parameter = Animator.StringToHash(parameterName);

            if (_animator == null)
            {
                return;
            }
            if (_animator.HasParameterOfType(parameterName, parameterType))
            {
                _character.AnimatorParameters.Add(parameter);
            }
        }

        #region PLACEHOLDER METHODS

        protected virtual void InitializeAnimatorParameters() { }

        /// <summary>
        /// Called at the very start of the ability's cycle, and intended to be overridden, looks for input and calls methods if conditions are met
        /// </summary>
        protected virtual void HandleInput() { }

        /// <summary>
        /// Override this to describe what should happen to this ability when the character takes a hit
        /// </summary>
        protected virtual void OnHit(GameObject instigator) { }

        /// <summary>
        /// Override this to describe what should happen to this ability when the character respawns
        /// </summary>
        protected virtual void OnDeath() { }

        /// <summary>
        /// Override this to describe what should happen to this ability when the character respawns
        /// </summary>
        protected virtual void OnRespawn() { }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Resets all input for this ability. Can be overridden for ability specific directives
        /// </summary>
        public virtual void ResetInput()
        {
            _horizontalInput = 0f;
            _verticalInput = 0f;
        }

        /// <summary>
        /// Functions as EarlyUpdate in our ability (if it existed).
        /// </summary>
        public virtual void EarlyProcessAbility()
        {
            InternalHandleInput();
        }

        /// <summary>
        /// Functions as Update in our ability.
        /// </summary>
        public virtual void ProcessAbility() { }

        /// <summary>
        /// Functions as LateUpdate in our ability.
        /// </summary>
        public virtual void LateProcessAbility() { }

        /// <summary>
        /// Override this to send parameters to the character's animator. This is called once per cycle, by the Character class, after Early, normal and Late process().
        /// </summary>
        public virtual void UpdateAnimator() { }

        /// <summary>
        /// Changes the status of the ability's permission
        /// </summary>
        /// <param name="abilityPermitted">If set to <c>true</c> ability permitted.</param>
        public virtual void PermitAbility(bool abilityPermitted)
        {
            AbilityPermitted = abilityPermitted;
        }

        /// <summary>
        /// Override this to specify what should happen in this ability when the character flips
        /// </summary>
        public virtual void Flip() { }

        /// <summary>
        /// Override this to reset this ability's parameters. It'll be automatically called when the character gets killed, in anticipation for its respawn.
        /// </summary>
        public virtual void ResetAbility() { }

        #endregion
    }
}

