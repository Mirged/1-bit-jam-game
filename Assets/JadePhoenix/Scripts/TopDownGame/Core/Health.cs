using System.Collections;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class Health : MonoBehaviour
    {
        // The model to disable (if set to).
        public GameObject Model;

        // The current health of the character.
        public int CurrentHealth;
        // If true, this object will not take damage.
        public bool Invulnerable = false;

        [Header("Health")]
        public int MaxHealth;
        public int InitialHealth;
        public float HealthPercentage { get { return CurrentHealth / MaxHealth; } }

        [Header("Damage")]
        public bool ImmuneToKnockback = false;

        [Header("Death")]
        public bool DestroyOnDeath = true;
        public float DelayBeforeDestruction = 0f;
        public int PointsWhenDestroyed;
        public bool RespawnAtInitialLocation = false;
        public bool DisableControllerOnDeath = true;
        public bool DisableModelOnDeath = true;
        public bool DisableCollisionsOnDeath = true;

        // hit delegate
        public delegate void OnHitDelegate(GameObject instigator);
        public OnHitDelegate OnHit;

        // respawn delegate
        public delegate void OnReviveDelegate();
        public OnReviveDelegate OnRevive;

        // death delegate
        public delegate void OnDeathDelegate();
        public OnDeathDelegate OnDeath;

        protected Vector3 _initialPosition;
        protected Entity _entity;
        protected TopDownController _controller;
        protected Collider _collider;
        protected bool _initialized = false;

        #region UNITY LIFECYCLE

        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void OnEnable()
        {
            CurrentHealth = InitialHealth;
            if (Model != null)
            {
                Model.SetActive(true);
            }
            DamageEnabled();
        }

        /// <summary>
        /// On Disable, we prevent any delayed destruction from running
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        #endregion

        protected virtual void Initialization()
        {
            _entity = GetComponent<Entity>();
            if (Model != null)
            {
                Model.SetActive(true);
            }

            _controller = GetComponent<TopDownController>();
            _collider = GetComponent<Collider>();

            _initialPosition = transform.position;
            _initialized = true;
            CurrentHealth = InitialHealth;
            DamageEnabled();
        }

        /// <summary>
        /// Destroys the object, or tries to, depending on the character's settings
        /// </summary>
        protected virtual void DestroyObject()
        {
            if (!DestroyOnDeath)
            {
                return;
            }

            gameObject.SetActive(false);
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Called when the object takes damage
        /// </summary>
        /// <param name="damage">The amount of health points that will get lost.</param>
        /// <param name="instigator">The object that caused the damage.</param>
        /// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage.</param>
        /// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
        public virtual void Damage(int damage, GameObject instigator, float invincibilityDuration)
        {
            // If the object is invulnerable, or we're already below zero, we do nothing and exit.
            if (Invulnerable || ((CurrentHealth <= 0) && (InitialHealth != 0)))
            {
                return;
            }

            float previousHealth = CurrentHealth;
            CurrentHealth -= damage;

            OnHit?.Invoke(instigator);

            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }

            // we prevent the character from colliding with Projectiles, Player and Enemies
            if (invincibilityDuration > 0)
            {
                DamageDisabled();
                StartCoroutine(DamageEnabledCoroutine(invincibilityDuration));
            }

            if (_entity != null && _entity is Character)
            {
                // we trigger a damage taken event
                CharacterEvents.DamageTakenEvent.Trigger(_entity as Character, instigator, CurrentHealth, damage, previousHealth);

                if ((_entity as Character).CharacterType == Character.CharacterTypes.Player)
                {
                    UIManager.Instance.UpdateHealthBar(HealthPercentage);
                }
            }

            // if health has reached zero
            if (CurrentHealth <= 0)
            {
                // we set its health to zero (useful for the healthbar)
                CurrentHealth = 0;
                Kill();
            }
        }

        /// <summary>
        /// Kills the character, instantiates death effects, handles points, etc
        /// </summary>
        public virtual void Kill()
        {
            if (_entity != null)
            {
                _entity.Reset();

                if (_entity is Character && (_entity as Character).CharacterType == Character.CharacterTypes.Player)
                {
                    (_entity as Character).ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);

                    if (DelayBeforeDestruction > 0f)
                    {
                        GameManager.Instance.TriggerGameOver(DelayBeforeDestruction);
                    }
                    else
                    {
                        GameManager.Instance.TriggerGameOver();
                    }
                }
            }

            CurrentHealth = 0;
            DamageDisabled();

            if (PointsWhenDestroyed != 0)
            {
                // ADD GAMEMANAGER EVENT FOR GAINING POINTS?
            }

            if (DisableCollisionsOnDeath)
            {
                if (_collider != null)
                {
                    _collider.enabled = false;
                }
                if (_controller != null)
                {
                    _controller.CollisionsOff();
                }
            }

            OnDeath?.Invoke();

            if (DisableControllerOnDeath && (_controller != null))
            {
                _controller.enabled = false;
                //_characterController.SetKinematic(true);
            }

            if (DisableModelOnDeath && (Model != null))
            {
                Model.SetActive(false);
            }

            if (DelayBeforeDestruction > 0f)
            {
                Invoke(nameof(DestroyObject), DelayBeforeDestruction);
            }
            else
            {
                // finally we destroy the object
                DestroyObject();
            }
        }

        /// <summary>
        /// Revive this object.
        /// </summary>
        public virtual void Revive()
        {
            if (!_initialized)
            {
                return;
            }

            if (_collider != null)
            {
                _collider.enabled = true;
            }
            if (_controller != null)
            {
                _controller.enabled = true;
                _controller.CollisionsOn();
                _controller.Reset();
            }
            if (_entity != null && _entity is Character)
            {
                (_entity as Character).ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            }

            if (RespawnAtInitialLocation)
            {
                transform.position = _initialPosition;
            }

            Initialization();

            OnRevive?.Invoke();
        }

        /// <summary>
        /// Called when the character gets health (from a stimpack for example)
        /// </summary>
        /// <param name="amountToHeal">The health the character gets.</param>
        /// <param name="instigator">The thing that gives the character health.</param>
        public virtual void Heal(int amountToHeal, GameObject instigator)
        {
            // this function adds health to the character's Health and prevents it to go above MaxHealth.
            CurrentHealth = Mathf.Min(CurrentHealth + amountToHeal, MaxHealth);

            if (_entity is Character && (_entity as Character).CharacterType == Character.CharacterTypes.Player)
            {
                UIManager.Instance.UpdateHealthBar(HealthPercentage);
            }
        }

        /// <summary>
        /// Resets the character's health to its max value
        /// </summary>
        public virtual void ResetHealthToMaxHealth()
        {
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// Prevents the character from taking any damage.
        /// </summary>
        public virtual void DamageDisabled()
        {
            Invulnerable = true;
        }

        /// <summary>
        /// Allows the character to take damage.
        /// </summary>
        public virtual void DamageEnabled()
        {
            Invulnerable = false;
        }

        #endregion

        #region COROUTINES

        /// <summary>
        /// Makes the character able to take damage again after the specified delay.
        /// </summary>
        /// <returns>The layer collision.</returns>
        public virtual IEnumerator DamageEnabledCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Invulnerable = false;
        }

        #endregion
    }
}

