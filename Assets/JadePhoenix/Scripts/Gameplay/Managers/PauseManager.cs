using JadePhoenix.Tools;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    /// <summary>
    /// Manages the pause state of the game, providing methods to toggle, set, and check the current pause state.
    /// The manager also ensures that certain game elements are disabled/enabled based on the pause state.
    /// </summary>
    public class PauseManager : Singleton<PauseManager>
    {
        #region Fields and Properties

        /// <summary>
        /// Flag to determine if the game is over. If true, the game cannot be paused.
        /// </summary>
        public bool GameOver;

        /// <summary>
        /// Current state of the pause: true if the game is paused, false otherwise.
        /// </summary>
        public bool IsPaused = false;

        /// <summary>
        /// Reference to the player character. This reference is used to enable/disable player controls during pause.
        /// </summary>
        public Character Player;

        #endregion

        #region Unity Lifecycle Methods

        /// <summary>
        /// Monitors input for the pause command. If the Escape key is pressed, it toggles the pause state.
        /// </summary>
        protected virtual void Update()
        {
            // Ensure that the InputManager instance is available.
            if (InputManager.Instance == null) { return; }

            // If the Escape key is pressed, toggle the pause state.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        #endregion

        #region Pause Control Methods

        /// <summary>
        /// Toggles the current pause state of the game. 
        /// Disables/enables the player character and displays/hides the pause UI accordingly.
        /// </summary>
        public virtual void TogglePause()
        {
            // If the game is over, the pause operation is not allowed.
            if (GameOver) { return; }

            // Toggle the pause state.
            IsPaused = !IsPaused;

            // If the game is paused, set the time scale to 0, effectively "pausing" all time-based events in the game.
            // Otherwise, set it to 1, resuming the game.
            Time.timeScale = IsPaused ? 0f : 1f;

            // Enable/Disable the player based on the pause state.
            Player.SetEnable(!IsPaused);

            // If UIManager instance is available, set the pause screen visibility based on the pause state.
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetPauseScreen(IsPaused);
            }
        }

        /// <summary>
        /// Sets the pause state to the given state, effectively pausing or unpausing the game based on the input.
        /// </summary>
        /// <param name="state">True to pause the game, false to unpause.</param>
        public virtual void SetPause(bool state)
        {
            // If the game is over, the pause operation is not allowed.
            if (GameOver) { return; }

            IsPaused = state;

            // Set the time scale based on the pause state.
            Time.timeScale = IsPaused ? 0f : 1f;

            // Enable/Disable the player based on the pause state.
            Player.SetEnable(!IsPaused);
        }

        #endregion
    }
}

