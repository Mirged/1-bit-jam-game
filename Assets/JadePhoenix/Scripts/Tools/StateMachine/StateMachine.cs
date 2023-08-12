using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    /// <summary>
    /// Struct representing a state change event in a state machine.
    /// </summary>
    /// <typeparam name="T">The type of the state enum.</typeparam>
    public struct StateChangeEvent<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        /// The target GameObject associated with the state change.
        public GameObject Target;        
        /// The target state machine associated with the state change.
        public StateMachine<T> TargetStateMachine;
        /// The new state after the change.
        public T NewState;
        /// The old state before the change.
        public T OldState;

        /// <summary>
        /// Constructor for creating a state change event.
        /// </summary>
        /// <param name="stateMachine">The state machine associated with the event.</param>
        public StateChangeEvent(StateMachine<T> stateMachine)
        {
            Target = stateMachine.Target;
            TargetStateMachine = stateMachine;
            NewState = stateMachine.CurrentState;
            OldState = stateMachine.PreviousState;
        }
    }

    /// <summary>
    /// Public interface for a state machine.
    /// </summary>
    public interface IStateMachine
    {
        bool TriggerEvents { get; set; }
    }

    /// <summary>
    /// A generic state machine manager.
    /// </summary>
    /// <typeparam name="T">The type of the state enum.</typeparam>
    [Serializable]
    public class StateMachine<T> : IStateMachine where T : struct, IComparable, IConvertible, IFormattable
    {
        /// Determines whether events should be triggered on state changes.
        public bool TriggerEvents { get; set; }
        /// The target GameObject associated with the state machine.
        [ReadOnly]
        public GameObject Target;
        /// Shows the current state in the inpector for Debugging purposes
        [ReadOnly]
        public T DebugCurrentState;
        /// The current state of the state machine.
        public T CurrentState { get; private set; }
        /// The previous state of the state machine.
        public T PreviousState { get; private set; }

        /// <summary>
        /// Creates a new instance of the state machine.
        /// </summary>
        /// <param name="target">The target GameObject associated with the state machine.</param>
        /// <param name="triggerEvents">Determines whether events should be triggered on state changes.</param>
        public StateMachine(GameObject target, bool triggerEvents)
        {
            this.Target = target;
            this.TriggerEvents = triggerEvents;
        }

        /// <summary>
        /// Changes the current state of the state machine.
        /// </summary>
        /// <param name="newState">The new state to change to.</param>
        public virtual void ChangeState(T newState)
        {
            //Debug.Log($"{this.GetType()}.ChangeState: Changing state to {newState}.");

            if (newState.Equals(CurrentState)) { return; }

            PreviousState = CurrentState;
            CurrentState = newState;
            DebugCurrentState = CurrentState;

            if (TriggerEvents)
            {
                EventManager.TriggerEvent(new StateChangeEvent<T>(this));
            }
        }

        /// <summary>
        /// Returns the state machine to its previous state.
        /// </summary>
        public virtual void RestorePreviousState()
        {
            // we restore our previous state
            CurrentState = PreviousState;

            if (TriggerEvents)
            {
                EventManager.TriggerEvent(new StateChangeEvent<T>(this));
            }
        }
    }
}
