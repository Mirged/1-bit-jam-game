using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    public static class JP_Math
    {
        /// <summary>
        /// Remaps a value x in interval [A,B], to the proportional value in interval [C,D]
        /// </summary>
        /// <param name="x">The value to remap.</param>
        /// <param name="A">the minimum bound of interval [A,B] that contains the x value</param>
        /// <param name="B">the maximum bound of interval [A,B] that contains the x value</param>
        /// <param name="C">the minimum bound of target interval [C,D]</param>
        /// <param name="D">the maximum bound of target interval [C,D]</param>
        public static float Remap(float x, float A, float B, float C, float D)
        {
            float remappedValue = C + (x - A) / (B - A) * (D - C);
            return remappedValue;
        }

        /// <summary>
        /// Returns a quaternion representing a rotation that faces a random direction away from the given central point, 
        /// constrained within a 180-degree arc.
        /// </summary>
        /// <param name="origin">The point of origin from which the direction is calculated.</param>
        /// <param name="centralPoint">The central point that the rotation will face away from.</param>
        /// <returns>A quaternion representing the rotation.</returns>
        public static Quaternion GetRandomRotationAwayFromPoint2D(Vector2 origin, Vector2 centralPoint, float maxRandomOffset = 10f)
        {
            // Calculate direction from origin to centralPoint.
            Vector2 directionToCenter = (centralPoint - origin).normalized;

            // Calculate the angle between the direction to the center and the positive y-axis.
            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;

            // The object's original "forward" is upwards (positive y). So, we compute the necessary rotation from this reference.
            // We subtract 90 because the default forward in 2D is upwards, which has an angle of 90 degrees in our context.
            // And then we add 180 to make it face away.
            float requiredRotation = angleToCenter - 90f + 180f;

            // Add a random offset to the rotation.
            float randomOffset = Random.Range(-maxRandomOffset, maxRandomOffset);
            requiredRotation += randomOffset;

            return Quaternion.Euler(0f, 0f, requiredRotation);
        }

        public static Quaternion GetRotationAwayFromPoint2D(Vector2 origin, Vector2 centralPoint)
        {
            // Calculate direction from origin to centralPoint.
            Vector2 directionToCenter = (centralPoint - origin).normalized;

            // Calculate the angle between the direction to the center and the positive y-axis.
            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;

            // The object's original "forward" is upwards (positive y). So, we compute the necessary rotation from this reference.
            // We subtract 90 because the default forward in 2D is upwards, which has an angle of 90 degrees in our context.
            // And then we add 180 to make it face away.
            float requiredRotation = angleToCenter - 90f + 180f;

            return Quaternion.Euler(0f, 0f, requiredRotation);
        }
    }
}
