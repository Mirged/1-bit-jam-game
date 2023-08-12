using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace JadePhoenix.Tools
{
    public class JP_Debug : MonoBehaviour
    {
        /// <summary>
        /// Draws a Gizmo cube at a specific offset and size relative to a given transform.
        /// </summary>
        /// <param name="transform">The transform representing the position and rotation of the Gizmo cube.</param>
        /// <param name="offset">The offset from the transform's position to the center of the Gizmo cube.</param>
        /// <param name="cubeSize">The size of the Gizmo cube along the three axes.</param>
        /// <param name="wireOnly">If true, draws the Gizmo cube as wireframe; otherwise, draws it as solid.</param>
        public static void DrawGizmoCube(Transform transform, Vector3 offset, Vector3 cubeSize, bool wireOnly)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;

            if (wireOnly)
            {
                Gizmos.DrawWireCube(offset, cubeSize);
            }
            else
            {
                Gizmos.DrawCube(offset, cubeSize);
            }
        }

        /// <summary>
        /// Draws a debug ray in 3D and does the actual raycast
        /// </summary>
        /// <returns>The raycast hit.</returns>
        /// <param name="rayOriginPoint">Ray origin point.</param>
        /// <param name="rayDirection">Ray direction.</param>
        /// <param name="rayDistance">Ray distance.</param>
        /// <param name="mask">Mask.</param>
        /// <param name="debug">If set to <c>true</c> debug.</param>
        /// <param name="color">Color.</param>
        /// <param name="drawGizmo">If set to <c>true</c> draw gizmo.</param>
        public static RaycastHit Raycast3D(Vector3 rayOriginPoint, Vector3 rayDirection, float rayDistance, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo)
            {
                Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);
            }
            RaycastHit hit;
            Physics.Raycast(rayOriginPoint, rayDirection, out hit, rayDistance, mask);
            return hit;
        }

        /// <summary>
        /// Performs a box cast and visualizes the results using Unity's Debug.DrawRay function.
        /// </summary>
        /// <param name="origin">The starting point of the box cast.</param>
        /// <param name="size">The size of the box used in the cast.</param>
        /// <param name="angle">The angle in degrees of the box used in the cast.</param>
        /// <param name="direction">The direction in which to cast the box.</param>
        /// <param name="distance">The distance of the box cast.</param>
        /// <param name="layerMask">The layer mask filter of the box cast.</param>
        public static void DebugBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers)
        {
            bool isHit = Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask);

            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            Gizmos.DrawWireCube(Vector2.zero, size);

            Gizmos.matrix = Matrix4x4.TRS(origin + (direction.normalized * distance), Quaternion.identity, Vector3.one);
            Gizmos.DrawWireCube(Vector2.zero, size);

            Gizmos.color = isHit ? Color.red : Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            Gizmos.DrawLine(Vector2.zero, direction.normalized * distance);
        }

        /// <summary>
        /// Debug visualizer for a 2D circle cast.
        /// </summary>
        /// <param name="origin">The center of the circle cast.</param>
        /// <param name="radius">The radius of the circle cast.</param>
        /// <param name="direction">The direction of the circle cast.</param>
        /// <param name="distance">The distance of the circle cast.</param>
        /// <param name="layerMask">The layer mask for collision checking.</param>
        public static void DebugCircleCast2D(Vector2 origin, float radius, Vector2 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, int numSegments = 10)
        {
            Color gizmoColor = Color.red;

            // Perform the initial circle cast
            RaycastHit2D hit = Physics2D.CircleCast(origin, radius, direction, distance, layerMask);

            if (hit.collider != null)
            {
                // If the circle cast hits something, change the Gizmo color to green
                gizmoColor = Color.green;

                // Visualize the hit point and normal
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.1f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(hit.point, hit.point + hit.normal);
            }

            // Draw the circle cast Gizmo
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(origin, radius);

            // Draw the path of the circle cast
            Gizmos.color = gizmoColor;
            Vector2 previousPoint = origin;
            float segmentDistance = distance / numSegments;

            for (int i = 1; i <= numSegments; i++)
            {
                Vector2 segmentOrigin = origin + direction * (segmentDistance * i);
                hit = Physics2D.CircleCast(segmentOrigin, radius, direction, 0f, layerMask);

                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPoint, hit.point);
                    Gizmos.DrawSphere(hit.point, 0.1f);
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal);

                    break; // Stop drawing if there's a hit along the path
                }

                Gizmos.DrawLine(previousPoint, segmentOrigin);
                previousPoint = segmentOrigin;
            }

            // Draw the end point of the circle cast
            Vector2 endPoint = origin + direction * distance;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(endPoint, radius);
        }
    }
}
