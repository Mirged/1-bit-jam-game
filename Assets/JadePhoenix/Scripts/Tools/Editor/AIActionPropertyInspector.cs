#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace JadePhoenix.Tools
{
    [CustomPropertyDrawer(typeof(AIAction))]
    public class AIActionPropertyInspector : PropertyDrawer
    {
        const float LineHeight = 16f;

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            // Get the currently selected GameObject in the Editor
            GameObject selectedGO = Selection.activeGameObject;
            if (selectedGO == null) return;

            // Fetch all AIAction components on the selected GameObject
            AIAction[] actions = selectedGO.GetComponents<AIAction>();
            if (actions.Length == 0) return;

            // Create a list of action labels. If the Label is blank or null, use DefaultLabel.
            string[] actionLabels = actions.Select(a => FormatActionLabel(a.DefaultLabel, a.Label)).ToArray();

            // Current index
            int currentIndex = Array.IndexOf(actions, prop.objectReferenceValue);

            // Create the dropdown menu
            currentIndex = EditorGUI.Popup(rect, "Select AI Action", currentIndex, actionLabels);

            // Update the SerializedProperty to point to the selected AIAction
            if (currentIndex >= 0)
            {
                prop.objectReferenceValue = actions[currentIndex];
            }
        }

        private string FormatActionLabel(string defaultLabel, string customLabel)
        {
            string cleanDefaultLabel = SeparateCamelCase(defaultLabel);
            if (!string.IsNullOrEmpty(customLabel))
            {
                return $"{cleanDefaultLabel} (\"{customLabel}\")";
            }
            return cleanDefaultLabel;
        }

        private string SeparateCamelCase(string input)
        {
            input = input.Replace("AIAction", ""); // Remove these substrings
            return Regex.Replace(input, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return LineHeight;
        }
    }
}
#endif
