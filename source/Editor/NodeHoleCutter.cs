using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public static class NodeHoleCutter
    {
        public static void CutHoles(HoleCutterBehaviour cutter)
        {
            if (cutter == null)
            {
                Debug.LogError("cutter is null");
                return;
            }

            var wallhit = findIntersecting(cutter.boxCollider);
            foreach (var wall in wallhit)
            {
                cutHole(cutter.transform, wall);
            }
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
            var wallhit = findIntersecting(collder);
            foreach (var wall in wallhit)
            {
                cutHole(cutter.transform, wall);
            }
        }
        public static void CutHolesWithSelected()
        {
            CutHoles(Selection.activeGameObject);
        }
       
        private static Transform[] findIntersecting(Collider cutter)
        {
            var bounds = cutter.bounds;
            return NodeDataManager.NodeBehaviours
                .Select(n => n.GetComponent<Collider>())
                .Where(c => c != null)
                .Where(c => bounds.Intersects(c.bounds))
                .Select(c => c.transform)
                .ToArray();
        }
        private static void cutHole(Transform cutter, Transform wall)
        {
            var position = calculateHolePosition(cutter, wall);
            var scale = calculateHoleScale(cutter, wall);

            var parentScale = getParentScale(wall);
            var scaledPos = Helper.InverseScale(position, parentScale);
            var scaledSca = Helper.InverseScale(scale, parentScale);

            var hole = new NodeJson(wall.name, scaledPos, scaledSca, "", NodeType.HOLE);

            NodeDataManager.AddNode(hole);
            NodeFactory.CreateNode(hole);
        }

        private static Vector3 calculateHolePosition(Transform cutter, Transform wall)
        {
            var wPos = wall.position;
            var cPos = cutter.position;

            var pos = Quaternion.Inverse(wall.rotation) * (cPos - wPos);
            pos.z = 0;
            return pos;
        }

        private static Vector3 calculateHoleScale(Transform cutter, Transform wall)
        {
            var cSca = wall.rotation * cutter.localScale;

            return new Vector2(Mathf.Abs(cSca.x), Mathf.Abs(cSca.y));
        }

        private static Vector3 getParentScale(Transform wall)
        {
            return wall.parent.lossyScale;
        }
    }
}
