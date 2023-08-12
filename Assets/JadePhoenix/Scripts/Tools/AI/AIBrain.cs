using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    /// <summary>
    /// the AI brain is responsible from going from one state to the other based on the defined transitions. It's basically just a collection of states, and it's where you'll link all the actions, decisions, states and transitions together.
    /// </summary>
    public class AIBrain : MonoBehaviour
    {
        public List<AIState> States;
        public bool BrainActive = true;
        public AIState CurrentState { get; protected set; }
        public float TimeInThisState;
        public Transform Target;

        [Header("Frequencies")]
        /// the frequency (in seconds) at which to perform actions (lower values : higher frequency, high values : lower frequency but better performance)
        public float ActionsFrequency = 0f;
        /// the frequency (in seconds) at which to evaluate decisions
        public float DecisionFrequency = 0f;

        protected AIDecision[] _decisions;
        protected float _lastActionsUpdate = 0f;
        protected float _lastDecisionsUpdate = 0f;

        protected virtual void Awake()
        {
            foreach (AIState state in States)
            {
                state.SetBrain(this);
            }
            _decisions = this.gameObject.GetComponents<AIDecision>();
        }

        protected virtual void Start()
        {
            if (States.Count > 0)
            {
                CurrentState = States[0];
            }
        }

        protected virtual void Update()
        {
            if (!BrainActive || CurrentState == null) { return; }

            if (Time.time - _lastActionsUpdate > ActionsFrequency)
            {
                CurrentState.PerformActions();
                _lastActionsUpdate = Time.time;
            }

            if (Time.time - _lastDecisionsUpdate > DecisionFrequency)
            {
                CurrentState.EvaluateTransitions();
                _lastDecisionsUpdate = Time.time;
            }

            TimeInThisState += Time.deltaTime;
        }

        public virtual void TransitionToState(string newStateName)
        {
            //Debug.Log($"{this.GetType()}.TransitionToState: Brain transitioning to state [{newStateName}].", gameObject);

            if (newStateName != CurrentState.StateName)
            {
                CurrentState.ExitState();
                OnExitState();

                CurrentState = FindState(newStateName);
                if (CurrentState != null)
                {
                    CurrentState.EnterState();
                }
            }
        }

        /// <summary>
        /// When exiting a state we reset our time counter
        /// </summary>
        protected virtual void OnExitState()
        {
            TimeInThisState = 0f;
        }

        /// <summary>
        /// Returns a state based on the specified state name
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        protected AIState FindState(string stateName)
        {
            foreach (AIState state in States)
            {
                if (state.StateName == stateName)
                {
                    return state;
                }
            }
            Debug.LogError($"{this.gameObject.GetType()}.FindState: You're trying to transition to state '" + stateName + "' in " + this.gameObject.name + "'s AI Brain, but no state of this name exists. Make sure your states are named properly, and that your transitions states match existing states.");
            return null;
        }

        /// <summary>
        /// Initializes all decisions
        /// </summary>
        protected virtual void InitializeDecisions()
        {
            foreach (AIDecision decision in _decisions)
            {
                decision.Initialization();
            }
        }
    }
}
