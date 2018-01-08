using UnityEngine;

namespace NodeLevelEditor
{
    public static class NodeHelper
    {
        public static void NormaliseScale(NodeBehaviour node)
        {
            if (node == null || node.GetScale() == Vector3.one) { return; }

            foreach (var child in node.Children)
            {
                child.transform.localScale = Helper.Scale(child.transform.localScale, node.transform.localScale);
                child.transform.localPosition = Helper.Scale(child.transform.localPosition, node.transform.localScale);

                if (child.ScaleFromChild)
                {
                    NormaliseScale(child);
                }
            }
            node.transform.localScale = Vector3.one;
        }
    }
}