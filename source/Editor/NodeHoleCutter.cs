using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public static class NodeHoleCutter
    {
        public static void CutHoles(GameObject cutter)
        {
            var wallhit = findIntersecting(cutter);
            foreach (var wall in wallhit)
            {
                cutHole(cutter.transform, wall);
            }
        }
        public static void CutHolesWithSelected()
        {
            CutHoles(Selection.activeGameObject);
        }
       
        private static Transform[] findIntersecting(GameObject cutter)
        {
            var bounds = cutter.GetComponent<Collider>().bounds;
            return NodeDataManager.NodeBehaviours
                .Select(n => n.GetComponent<Collider>())
                .Where(c => c != null)
                .Where(c => bounds.Intersects(c.bounds))
                .Select(c => c.transform)
                .ToArray();
        }
        private static void cutHole(Transform cutter, Transform wall)
        {
            var wPos = wall.position;
            var cPos = cutter.position;
            var cSca = wall.rotation * cutter.localScale;

            var pos = wall.rotation * (cPos - wPos);
            pos.x = -pos.x;
            pos.z = 0;
            var sca = new Vector2(Mathf.Abs(cSca.x), Mathf.Abs(cSca.y));

            var hole = new NodeJson(wall.name, pos, sca, "", NodeType.HOLE);

            NodeDataManager.AddNode(hole);
            NodeFactory.CreateNode(hole);
        }
    }
}
