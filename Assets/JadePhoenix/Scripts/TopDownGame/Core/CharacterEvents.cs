using JadePhoenix.Tools;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    public class CharacterEvents : MonoBehaviour
    {
        public enum CharacterEventTypes
        {
            ButtonActivation
        }

        public struct CharacterEvent
        {
            public Character TargetCharacter;
            public CharacterEventTypes EventType;

            /// <summary>
            /// Initializes a new instance of the <see cref="Gameplay.CharacterEvent"/> struct.
            /// </summary>
            /// <param name="character">Character.</param>
            /// <param name="eventType">Event type.</param>
            public CharacterEvent(Character character, CharacterEventTypes eventType)
            {
                TargetCharacter = character;
                EventType = eventType;
            }

            static CharacterEvent e;
            public static void Trigger(Character character, CharacterEventTypes eventType)
            {
                e.TargetCharacter = character;
                e.EventType = eventType;
                EventManager.TriggerEvent(e);
            }
        }

        public struct DamageTakenEvent
        {
            public Character DamagedCharacter;
            public GameObject Attacker;
            public float CurrentHealth;
            public float DamageCaused;
            public float PreviousHealth;

            /// <summary>
            /// Initializes a new instance the <see cref="DamageTakenEvent"/> struct.
            /// </summary>
		    /// <param name="affectedCharacter">Affected character.</param>
		    /// <param name="instigator">Instigator.</param>
		    /// <param name="currentHealth">Current health.</param>
		    /// <param name="damageCaused">Damage caused.</param>
		    /// <param name="previousHealth">Previous health.</param>
            public DamageTakenEvent(Character damagedCharacter, GameObject attacker, float currentHealth, float damageCaused, float previousHealth)
            {
                DamagedCharacter = damagedCharacter;
                Attacker = attacker;
                CurrentHealth = currentHealth;
                DamageCaused = damageCaused;
                PreviousHealth = previousHealth;
            }

            static DamageTakenEvent e;
            public static void Trigger(Character damagedCharacter, GameObject attacker, float currentHealth, float damageCaused, float previousHealth)
            {
                e.DamagedCharacter = damagedCharacter;
                e.Attacker = attacker;
                e.CurrentHealth = currentHealth;
                e.DamageCaused = damageCaused;
                e.PreviousHealth = previousHealth;
                EventManager.TriggerEvent(e);
            }
        }
    }
}

