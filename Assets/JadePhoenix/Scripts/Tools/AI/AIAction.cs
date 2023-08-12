using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    public abstract class AIAction : MonoBehaviour
    {
        public string Label;
        public string DefaultLabel { get { return GetType().Name; } }
        public abstract void PerformAction();
        public bool ActionInProgress { get; set; }
        
        protected AIBrain _brain;

        protected virtual void Start()
        {
            _brain = this.gameObject.GetComponent<AIBrain>();
            Initialization();
        }

        protected virtual void Initialization() { }

        public virtual void OnEnterState()
        {
            ActionInProgress = true;
        }

        public virtual void OnExitState()
        {
            ActionInProgress = false;
        }
    }
}
