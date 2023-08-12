using JadePhoenix.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class MeleeWeapon : Weapon
    {
        public enum MeleeDamageAreaShapes { Box, Circle }

        [Header("DamageArea")]
        public MeleeDamageAreaShapes DamageAreaShape = MeleeDamageAreaShapes.Box;
        public Vector2 AreaSize = new Vector2(1, 1);
        public Vector2 AreaOffset = new Vector2(1, 0);

        [Header("Damage Area Timing")]
        public float InitialDelay = 0f;
        public float BaseActiveDuration = 2f;
        public float ActiveDuration;

        [Header("Damage Caused")]
        public LayerMask TargetLayerMask;
        public int DamageCaused = 10;
        public DamageOnTouch.KnockbackStyles Knockback;
        public Vector2 KnockbackForce = new Vector2(10, 2);
        public float InvincibilityDuration = 0.5f;
        public bool CanDamageOwner = false;

        [Header("Attack Animator")]
        public AnimatorOverrideController AttackOverrideController;

        public DamageOnTouch DamageOnTouch { get { return _damageOnTouch; } }

        protected Collider2D _damageAreaCollider2D;
        protected bool _attackInProgress = false;
        protected Color _gizmosColor;
        protected Vector2 _gizmoSize;
        protected BoxCollider2D _boxCollider2D;
        protected CircleCollider2D _circleCollider2D;
        protected Vector2 _gizmoOffset;
        protected DamageOnTouch _damageOnTouch;
        protected GameObject _damageArea;

        protected Animator _attackAnimator;
        protected List<int> _animatorParameters = new List<int>();

        protected const string _attackAnimationParameterName = "Attack1";
        protected int _attackAnimationParameter;

        public override void Initialization()
        {
            base.Initialization();

            if (_damageArea == null)
            {
                CreateDamageArea();
                DisableDamageArea();
            }
            if (Owner != null)
            {
                _damageOnTouch.Owner = Owner.gameObject;
            }

            ActiveDuration = BaseActiveDuration;
        }

        protected virtual void CreateDamageArea()
        {
            _damageArea = new GameObject();
            _damageArea.name = $"{this.name}DamageArea";
            _damageArea.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
            _damageArea.transform.SetParent(this.transform);
            _damageArea.layer = this.gameObject.layer;

            if (AttackOverrideController != null)
            {
                GameObject animatorHolder = new GameObject();
                animatorHolder.name = $"{this.name}DamageAreaAnimatorHolder";
                animatorHolder.transform.SetPositionAndRotation((Vector2)_damageArea.transform.position + AreaOffset, _damageArea.transform.rotation);
                animatorHolder.transform.SetParent(_damageArea.transform);

                animatorHolder.AddComponent<SpriteRenderer>().sortingLayerName = "VFX";
                _attackAnimator = animatorHolder.AddComponent<Animator>();
                _attackAnimator.runtimeAnimatorController = AttackOverrideController;

                RegisterAnimatorParameter(_attackAnimationParameterName, AnimatorControllerParameterType.Trigger, out _attackAnimationParameter);
            }

            if (DamageAreaShape == MeleeDamageAreaShapes.Box)
            {
                _boxCollider2D = _damageArea.AddComponent<BoxCollider2D>();
                _boxCollider2D.offset = AreaOffset;
                _boxCollider2D.size = AreaSize;
                _damageAreaCollider2D = _boxCollider2D;
                _damageAreaCollider2D.isTrigger = true;
            }

            if (DamageAreaShape == MeleeDamageAreaShapes.Circle)
            {
                _circleCollider2D = _damageArea.AddComponent<CircleCollider2D>();
                _circleCollider2D.offset = AreaOffset;
                _circleCollider2D.radius = AreaSize.x / 2;
                _damageAreaCollider2D = _circleCollider2D;
                _damageAreaCollider2D.isTrigger = true;
            }

            Rigidbody2D rigidbody2D = _damageArea.AddComponent<Rigidbody2D>();
            rigidbody2D.isKinematic = true;

            _damageOnTouch = _damageArea.AddComponent<DamageOnTouch>();
            _damageOnTouch.SetGizmoSize(AreaSize);
            _damageOnTouch.SetGizmoOffset(AreaOffset);
            _damageOnTouch.TargetLayerMask = TargetLayerMask;
            _damageOnTouch.DamageCaused = DamageCaused;
            _damageOnTouch.DamageCausedKnockbackType = Knockback;
            _damageOnTouch.DamageCausedKnockbackForce = KnockbackForce;
            _damageOnTouch.InvincibilityDuration = InvincibilityDuration;

            if (!CanDamageOwner)
            {
                _damageOnTouch.IgnoreGameObject(Owner.gameObject);
            }
        }

        protected virtual void EnableDamageArea()
        {
            if (_damageAreaCollider2D != null)
            {
                _damageAreaCollider2D.enabled = true;
                AnimatorExtensions.UpdateAnimatorTrigger(_attackAnimator, _attackAnimationParameter, _animatorParameters);
            }
        }

        protected virtual void DisableDamageArea()
        {
            if (_damageAreaCollider2D != null)
            {
                _damageAreaCollider2D.enabled = false;
            }
            else
            {
                Debug.LogError($"{this.GetType()}.DisableDamageArea: _damageAreaCollider2D not found.", gameObject);
            }
        }

        public override void WeaponUse()
        {
            base.WeaponUse();
            StartCoroutine(MeleeWeaponAttackCoroutine());
        }

        protected virtual IEnumerator MeleeWeaponAttackCoroutine()
        {
            if (_attackInProgress) { yield break; }

            _attackInProgress = true;

            yield return new WaitForSeconds(InitialDelay);
            EnableDamageArea();

            yield return new WaitForSeconds(ActiveDuration);
            DisableDamageArea();

            _attackInProgress = false;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                DrawGizmos();
            }
        }

        protected virtual void DrawGizmos()
        {
            switch (DamageAreaShape)
            {
                case MeleeDamageAreaShapes.Box:
                    Gizmos.DrawWireCube((Vector2)transform.position + AreaOffset, AreaSize);
                    break;

                case MeleeDamageAreaShapes.Circle:
                    Gizmos.DrawWireSphere((Vector2)transform.position + AreaOffset, AreaSize.x / 2);
                    break;
            }
        }

        protected virtual void RegisterAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType, out int parameter)
        {
            parameter = Animator.StringToHash(parameterName);

            if (_attackAnimator == null)
            {
                return;
            }
            if (_attackAnimator.HasParameterOfType(parameterName, parameterType))
            {
                _animatorParameters.Add(parameter);
            }
        }
    }
}

