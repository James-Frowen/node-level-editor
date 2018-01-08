using UnityEngine;
using UnityEditor;
using NodeLevelEditor;

namespace NodeLevelEditorEvents
{
    [InitializeOnLoad]
    public class NodeLevelEditorEvents
    {
        static NodeLevelEditorEvents()
        {
            NodeCreator.onNodeCreation += onNodeCreation;
        }

        private static void onNodeCreation(NodeBehaviour node)
        {
            var go = node.gameObject;

            go.layer = LayerMask.NameToLayer("AutoGen");
            go.tag = "EditorAutoGen";
        }
    }
}
