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
                .ToArray();
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
