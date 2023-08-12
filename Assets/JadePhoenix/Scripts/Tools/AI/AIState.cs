using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    [System.Serializable]
    public class AIActionsList : ReorderableArray<AIAction> { }

    [System.Serializable]
    public class AITransitionsList : ReorderableArray<AITransition> { }

    [System.Serializable]
    public class AIState
    {
        public string StateName;

        [Reorderable(null, "Action", null)]
        public AIActionsList Actions;
        [Reorderable(null, "Transition", null)]
        public AITransitionsList Transitions;

        protected AIBrain _brain;

        /// <summary>
        /// Sets this state's brain to the one specified in parameters
        /// </summary>
        /// <param name="brain"></param>
        public virtual void SetBrain(AIBrain brain)
        {
            _brain = brain;
        }

        /// <summary>
        /// On enter state we pass that info to our actions and decisions
        /// </summary>
        public virtual void EnterState()
        {
            foreach(AIAction action in Actions)
            {
                action.OnEnterState();
            }
            foreach(AITransition transition in Transitions)
            {
                if (transition.Decision != null)
                {
                    transition.Decision.OnEnterState();
                }
            }
        }

        /// <summary>
        /// On exit state we pass that info to our actions and decisions
        /// </summary>
        public virtual void ExitState()
        {
            foreach (AIAction action in Actions)
            {
                action.OnExitState();
            }
            foreach (AITransition transition in Transitions)
            {
                if (transition.Decision != null)
                {
                    transition.Decision.OnExitState();
                }
            }
        }

        public virtual void PerformActions()
        {
            if (Actions.Count == 0) { return; }

            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i] != null)
                {
                    Actions[i].PerformAction();
                }
                else
                {
                    Debug.LogError($"{this.GetType()}.PerformActions: Action[{i}] in {_brain.gameObject.name} is null.");
                }
            }
        }

        public virtual void EvaluateTransitions()
        {
            if (Transitions.Count == 0) { return; }
            for (int i = 0; i < Transitions.Count; i++)
            {
                if (Transitions[i].Decision != null)
                {
                    if (Transitions[i].Decision.Decide())
                    {
                        if (Transitions[i].TrueState != "")
                        {
                            //Debug.Log($"{this.GetType()}.EvaluateTransitions: Transitioning to {Transitions[i].TrueState}.");

                            _brain.TransitionToState(Transitions[i].TrueState);
                        }
                    }
                    else
                    {
                        if (Transitions[i].FalseState != "")
                        {
                            _brain.TransitionToState(Transitions[i].FalseState);
                        }
                    }
                }
            }
        }
    }
}
