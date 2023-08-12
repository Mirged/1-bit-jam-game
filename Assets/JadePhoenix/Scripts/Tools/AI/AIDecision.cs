using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    public abstract class AIDecision : MonoBehaviour
    {
        public abstract bool Decide();
        public string Label;
        public string DefaultLabel { get { return GetType().Name; } }
        public bool DecisionInProgress { get; set; }

        protected AIBrain _brain;

        protected virtual void Start()
        {
            _brain = this.gameObject.GetComponent<AIBrain>();
            Initialization();
        }

        public virtual void Initialization() { }

        public virtual void OnEnterState()
        {
            DecisionInProgress = true;
        }

        public virtual void OnExitState()
        {
            DecisionInProgress = false;
        }
    }
}
