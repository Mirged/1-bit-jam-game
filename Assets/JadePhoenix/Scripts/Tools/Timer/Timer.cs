using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    public class Timer
    {
        public string Label { get; private set; }
        /// The total duration of the timer
        public float Duration { get; private set; }
        /// The elapsed time of the timer
        public float ElapsedTime { get; private set; }
        /// Whether the timer is currently running
        public bool IsRunning { get; private set; }

        protected Action _onTimerStarted;
        protected Action _onTimerCompleted;

        /// <summary>
        /// A generic class that allows us to attach methods and functions.
        /// </summary>
        /// <param name="duration">The total duration of the timer.</param>
        public Timer(float duration, Action onTimerStarted = null, Action onTimerCompleted = null)
            : this(string.Empty, duration, onTimerStarted, onTimerCompleted) { }

        public Timer(string label, float duration, Action onTimerStarted = null, Action onTimerCompleted = null)
        {
            Label = label;
            Duration = duration;
            ElapsedTime = 0f;
            IsRunning = false;
            _onTimerStarted = onTimerStarted;
            _onTimerCompleted = onTimerCompleted;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name="invokeStart">If true, wil invoke the OnTimerStarted event.</param>
        /// <param name="resetTimer">If true, will reset ElapsedTime before starting the timer.</param>
        public virtual void StartTimer(bool invokeStart = true, bool resetTimer = false)
        {
            if (resetTimer) ResetTimer();

            if (IsRunning) return;
            IsRunning = true;

            if (invokeStart) _onTimerStarted?.Invoke();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public virtual void StopTimer()
        {
            IsRunning = false;
        }

        /// <summary>
        /// If the timer is running, updates the elapsed time.
        /// Disables the timer when it has reached the set Duration.
        /// This <b>must</b> be called during the Update method of the relevant class.
        /// </summary>
        public virtual void UpdateTimer()
        {
            if (!IsRunning) return;

            ElapsedTime += Time.deltaTime;

            if (ElapsedTime >= Duration)
            {
                ElapsedTime = Duration;
                IsRunning = false;
                _onTimerCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Resets the timer to 0.
        /// </summary>
        public virtual void ResetTimer()
        {
            ElapsedTime = 0f;
            IsRunning = false;
        }

        /// <summary>
        /// Sets the duration of the timer.
        /// </summary>
        /// <param name="duration">Time to set the duration to.</param>
        public virtual void SetDuration(float duration)
        {
            Duration = Mathf.Max(0f, duration);
        }

        /// <summary>
        /// Returns the normalized value of the timer's progress (0 to 1).
        /// </summary>
        /// <returns></returns>
        public virtual float GetNormalisedTime()
        {
            if (Duration <= 0f) return 0f;

            return Mathf.Clamp01(ElapsedTime / Duration);
        }
    }
}
