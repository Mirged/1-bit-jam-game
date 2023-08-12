using JadePhoenix.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JadePhoenix.Gameplay
{
    /// <summary>
    /// Manages the game's overall state and scene transitions.
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        // Current scene's index in the build settings.
        public int CurrentSceneIndex = 0;
        public int CurrentPoints = 0;

        protected virtual void OnEnable()
        {
            // Subscribe to the scene loaded event.
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected virtual void OnDisable()
        {
            // Unsubscribe from the scene loaded event.
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // Called when a new scene is loaded.
        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Initialization(scene);
        }

        // Initialize necessary components or settings after a scene is loaded.
        protected virtual void Initialization(Scene scene)
        {
            CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            if (scene.name == "MainMenu")
            {
                CurrentPoints = 0;
            }
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Load the next scene in the build settings.
        /// </summary>
        public virtual void NextScene()
        {
            CurrentSceneIndex++;

            if (CurrentSceneIndex > SceneManager.sceneCountInBuildSettings)
            {
                CurrentSceneIndex = 0;
            }

            SceneManager.LoadScene(CurrentSceneIndex);
        }

        public virtual void LoadScene(int sceneBuildIndex)
        {
            SceneManager.LoadScene(sceneBuildIndex);
        }

        public virtual void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Quit the application.
        /// </summary>
        public virtual void CloseGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Trigger game over behavior, displaying either victory or defeat UI.
        /// </summary>
        /// <param name="victory">If true, trigger victory. Otherwise and by default, trigger defeat.</param>
        public virtual void TriggerGameOver(float delay = 0, bool victory = false)
        {
            StartCoroutine(TriggerGameOverCoroutine(delay, victory));
        }

        protected virtual IEnumerator TriggerGameOverCoroutine(float delay, bool victory)
        {
            yield return new WaitForSeconds(delay);

            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.SetPause(true);
                PauseManager.Instance.GameOver = true;
            }

            if (UIManager.Instance != null)
            {
                if (victory)
                {
                    UIManager.Instance.SetVictoryScreen(true);
                }
                else
                {
                    UIManager.Instance.SetDeathScreen(true);
                }
            }
        }

        #endregion
    }
}

