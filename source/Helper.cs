using System;
using UnityEngine;

namespace NodeLevelEditor
{
    public static class Helper
    {
        /// <summary>
        /// Divide each part of A by each part of B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector3 InverseScale(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.x / b.x,
                a.y / b.y,
                a.z / b.z
                );
        }
        /// <summary>
        /// Divide each part of A by each part of B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector2 InverseScale(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x / b.x,
                a.y / b.y
                );
        }

        /// <summary>
        /// Multiply each part of A by each part of B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector3 Scale(Vector3 A, Vector3 B)
        {
            return new Vector3(
                A.x * B.x,
                A.y * B.y,
                A.z * B.z
                );
        }
        /// <summary>
        /// Multiply each part of A by each part of B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector2 Scale(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x * b.x,
                a.y * b.y
                );
        }

        /// <summary>
        /// Sets the partent of child without changing the local position, scale and rotation
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        public static void SetParentKeepLocals(Transform child, Transform parent)
        {
            var pos = child.localPosition;
            var sca = child.localScale;
            var rot = child.localRotation;
            child.parent = parent;

            child.localPosition = pos;
            child.localScale = sca;
            child.localRotation = rot;
        }
    }
}