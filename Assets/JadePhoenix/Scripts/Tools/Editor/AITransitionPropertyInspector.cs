#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace JadePhoenix.Tools
{
    [CustomPropertyDrawer(typeof(AITransition))]
    public class AITransitionPropertyInspector : PropertyDrawer
    {
        const float LineHeight = 16f;

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect position = rect;
            GameObject selectedGO = Selection.activeGameObject;
            AIBrain brain = selectedGO?.GetComponent<AIBrain>();

            if (brain != null)
            {
                string[] stateNamesWithDefault = brain.States.Select(s => s.StateName).Prepend("[None]").ToArray();

                foreach (SerializedProperty a in prop)
                {
                    var height = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(a));
                    position.height = height;

                    if (a.name == "Decision")
                    {
                        AIDecision[] decisions = selectedGO.GetComponents<AIDecision>();
                        string[] decisionLabels = decisions.Select(d =>
                            FormatDecisionLabel(d.DefaultLabel, d.Label)
                        ).ToArray();
                        int currentIndex = Array.IndexOf(decisions, a.objectReferenceValue);

                        currentIndex = EditorGUI.Popup(position, "Select AI Decision", currentIndex, decisionLabels);
                        if (currentIndex >= 0)
                        {
                            a.objectReferenceValue = decisions[currentIndex];
                        }

                        position.y += height;
                    }
                    else if (a.name == "TrueState" || a.name == "FalseState")
                    {
                        int currentIndex = Array.IndexOf(stateNamesWithDefault, a.stringValue);
                        if (currentIndex == -1) currentIndex = 0;  // Set to [None] if not found

                        currentIndex = EditorGUI.Popup(position, a.displayName, currentIndex, stateNamesWithDefault);

                        if (currentIndex == 0)
                        {
                            a.stringValue = "";  // Set to blank if [None] is selected
                        }
                        else
                        {
                            a.stringValue = stateNamesWithDefault[currentIndex];
                        }

                        position.y += height;
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, a, new GUIContent(a.name));
                        position.y += height;
                    }
                }
            }
        }

        private string FormatDecisionLabel(string defaultLabel, string customLabel)
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
            input = input.Replace("AIDecision", ""); // Remove these substrings
            return Regex.Replace(input, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            foreach (SerializedProperty a in property)
            {
                var h = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(a));
                height += h;
            }
            return height;
        }
    }
}
#endif
