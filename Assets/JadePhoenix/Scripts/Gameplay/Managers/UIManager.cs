using JadePhoenix.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JadePhoenix.Gameplay
{
    /// <summary>
    /// Handles all UI effects and changes.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        public Image HealthSegment;
        public Image DamageSegment;

        public GameObject PauseScreen;
        public GameObject DeathScreen;
        public GameObject VictoryScreen;
        public GameObject CreditsScreen;

        public GameObject ChatBox;

        protected virtual void Start()
        {
            SetPauseScreen(false);
            SetDeathScreen(false);
            SetVictoryScreen(false);
        }

        #region PUBLIC METHODS

        #region CORE METHODS

        /// <summary>
        /// Sets the pause screen on or off.
        /// </summary>
        public virtual void SetPauseScreen(bool state)
        {
            if (PauseScreen != null && PauseManager.Instance != null)
            {
                PauseScreen.SetActive(state);
                PauseManager.Instance.SetPause(state);
                EventSystem.current.sendNavigationEvents = state;
            }
        }

        /// <summary>
        /// Sets the death screen on or off.
        /// </summary>
        public virtual void SetDeathScreen(bool state)
        {
            if (DeathScreen != null)
            {
                DeathScreen.SetActive(state);
                PauseManager.Instance.SetPause(state);
                EventSystem.current.sendNavigationEvents = state;
            }
        }

        /// <summary>
        /// Sets the victory screen on or off.
        /// </summary>
        public virtual void SetVictoryScreen(bool state)
        {
            if (VictoryScreen != null)
            {
                VictoryScreen.SetActive(state);
                PauseManager.Instance.SetPause(state);
                EventSystem.current.sendNavigationEvents = state;
            }
        }

        /// <summary>
        /// Sets the victory screen on or off.
        /// </summary>
        public virtual void SetCreditsScreen(bool state)
        {
            if (CreditsScreen != null)
            {
                CreditsScreen.SetActive(state);
            }
        }

        #endregion /// Core methods

        /// <summary>
        /// Updates the health bar based on current, min, and max health values.
        /// </summary>
        public virtual void UpdateHealthBar(float healthPercentage)
        {
            if (HealthSegment == null) { return; }
            if (DamageSegment == null) { return; }

            Debug.Log($"{this.GetType()}.UpdateHealthBar: Health % before clamp = {healthPercentage}.", gameObject);

            healthPercentage = Mathf.Clamp01(healthPercentage);  // Ensure it's between 0 and 1

            Debug.Log($"{this.GetType()}.UpdateHealthBar: Health % after clamp = {healthPercentage}.", gameObject);

            HealthSegment.fillAmount = healthPercentage;

            float damageTaken = 1.0f - healthPercentage;
            DamageSegment.fillAmount = damageTaken;
        }

        public virtual void ToggleChatBox(string text)
        {

        }

        #region BUTTON METHODS

        /// <summary>
        /// Loads the main menu via GameManager.
        /// </summary>
        public void LoadMainMenu()
        {
            GameManager.Instance.LoadScene("MainMenu");
        }

        /// <summary>
        /// Exits the game via GameManager.
        /// </summary>
        public void CloseGame()
        {
            GameManager.Instance.CloseGame();
        }

        /// <summary>
        /// Loads a scene using the BuildIndex value.
        /// </summary>
        public virtual void LoadSceneInt(int sceneBuildIndex)
        {
            GameManager.Instance.LoadScene(sceneBuildIndex);
        }

        #endregion /// Button Methods

        #endregion /// Public Methods
    }
}

