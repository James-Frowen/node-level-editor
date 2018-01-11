using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public static class NodeHoleCutter
    {
        public static void CutHolesWithSelected()
        {
            CutHoles(Selection.activeGameObject);
        }
        public static void CutHoles(GameObject cutter)
        {
            if (cutter == null)
            {
                Debug.LogError("cutter is null");
                return;
            }

            var collder = cutter.GetComponent<Collider>();
            if (collder == null)
            {
                Debug.LogError("cutter needs a collder");
                return;
            }
            CutHoles(collder);
        }
        public static void CutHoles(Collider cutterCollder)
        {
            if (cutterCollder == null)
            {
                Debug.LogError("collider is null");
                return;
            }
            var wallhit = findIntersecting(cutterCollder);
            foreach (var wall in wallhit)
            {
                cutHole(cutterCollder.transform, wall);
            }
        }
        public static void CutHoles(HoleCutterBehaviour cutter)
        {
            if (cutter == null)
            {
                Debug.LogError("cutter is null");
                return;
            }

            CutHoles(cutter.transform.localPosition, cutter.transform.localScale);
        }
        public static void CutHoles(Vector3 pos, Vector3 sca)
        {
            var bounds = new Bounds(pos, sca);
            var wallhit = findIntersecting(bounds);
            foreach (var wall in wallhit)
            {
                cutHole(pos, sca, wall);
            }
        }

        public static bool IgnoreX = false;
        public static bool IgnoreY = false;
        public static bool IgnoreZ = false;
        public static void ResetIgnores()
        {
            IgnoreX = false;
            IgnoreY = false;
            IgnoreZ = false;
        }

        private static Transform[] findIntersecting(Collider cutter)
        {
            var bounds = cutter.bounds;
            return findIntersecting(bounds);
        }
        private static Transform[] findIntersecting(Bounds bounds)
        {
            return NodeDataManager.NodeBehaviours
                .Select(n => n.GetComponent<Collider>())
                .Where(c => c != null)
                .Where(c => bounds.Intersects(c.bounds))
                .Select(c => c.transform)
                .Where(checkIgnoreX)
                .Where(checkIgnoreY)
                .Where(checkIgnoreZ)
                .ToArray();
        }
        private static bool checkIgnoreX(Transform t)
        {
            if (!IgnoreX) { return true; }

            var normal = getNormal(t);

            var isX = Mathf.Abs(Vector3.Dot(normal, Vector3.right)) > 0.5f;

            return !isX; // isX return false
        }
        private static bool checkIgnoreY(Transform t)
        {
            if (!IgnoreY) { return true; }

            var normal = getNormal(t);

            var isY = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.5f;

            return !isY; // isY return false
        }
        private static bool checkIgnoreZ(Transform t)
        {
            if (!IgnoreZ) { return true; }

            var normal = getNormal(t);

            var isZ = Mathf.Abs(Vector3.Dot(normal, Vector3.forward)) > 0.5f;

            return !isZ; // isZ return false
        }
        private static Vector3 getNormal(Transform t)
        {
            return t.rotation * Vector3.forward;
        }

        private static void cutHole(Transform cutter, Transform wall)
        {
            cutHole(cutter.localPosition, cutter.localScale, wall);
        }
        private static void cutHole(Vector3 pos, Vector3 sca, Transform wall)
        {
            var position = calculateHolePosition(pos, wall);
            var scale = calculateHoleScale(sca, wall);

            var parentScale = getParentScale(wall);
            var scaledPos = Helper.InverseScale(position, parentScale);
            var scaledSca = Helper.InverseScale(scale, parentScale);

            var hole = new NodeJson(wall.name, scaledPos, scaledSca, "", NodeType.HOLE);

            NodeDataManager.AddNode(hole);
            NodeFactory.CreateNode(hole);
        }

        private static Vector3 calculateHolePosition(Vector3 cutterPosition, Transform wall)
        {
            var wPos = wall.position;
            var cPos = cutterPosition;

            var pos = Quaternion.Inverse(wall.rotation) * (cPos - wPos);
            pos.z = 0;
            return pos;
        }

        private static Vector3 calculateHoleScale(Vector3 cutterScale, Transform wall)
        {
            var cSca = wall.rotation * cutterScale;

            return new Vector2(Mathf.Abs(cSca.x), Mathf.Abs(cSca.y));
        }

        private static Vector3 getParentScale(Transform wall)
        {
            return wall.parent.lossyScale;
        }
    }
}
