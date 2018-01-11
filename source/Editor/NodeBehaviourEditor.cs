using System;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    [CustomEditor(typeof(NodeBehaviour))]
    public class NodeBehaviourEditor : UnityEditor.Editor
    {
        private bool showChangeParent = false;
        private NodeBehaviour newParent;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var node = (NodeBehaviour)this.target;
            GUIHelper.HorizontalLine();

            var dataChanged = node.NodeState.StateChanged();

            GUI.enabled = !node.noJson;
            if (GUILayout.Button("Save Node" + (dataChanged ? " *" : "")))
            {
                var json = node.ToJson();//need to add as well??
                NodeDataManager.AddNode(json);
                NodeDataManager.Save();
                node.NodeState.UpdateState();
            }
            GUI.enabled = true;

            changeParentMenu(node);

            if (GUILayout.Button("Normalise direct children"))
            {
                NodeHelper.NormaliseScale(node);
            }

            GUIHelper.HorizontalLine();


            GUI.enabled = false;
            EditorGUILayout.Vector3Field("Position", node.GetPosition());
            EditorGUILayout.Vector3Field("Scale", node.GetScale());
            GUI.enabled = true;
        }

        private void changeParentMenu(NodeBehaviour node)
        {
            if (this.showChangeParent)
            {
                GUIHelper.HorizontalLine();
                GUILayout.Label("Change Parent");
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Current Parent", node.Parent, typeof(NodeBehaviour), true);
                GUI.enabled = true;
                this.newParent = EditorGUILayout.ObjectField("New Parent", this.newParent, typeof(NodeBehaviour), true) as NodeBehaviour;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Accept Parent"))
                {
                    if (this.validateNewParent(node, this.newParent))
                    {
                        node.ChangeParent(this.newParent);
                    }
                    this.resetChangeParent();
                }
                if (GUILayout.Button("Cancel"))
                {
                    this.resetChangeParent();
                }
                GUILayout.EndHorizontal();
                GUIHelper.HorizontalLine();
            }
            else
            {
                if (GUILayout.Button("Change Parent"))
                {
                    this.showChangeParent = true;
                }
            }
        }
        private void resetChangeParent()
        {
            this.showChangeParent = false;
            this.newParent = null;
        }
        private bool validateNewParent(NodeBehaviour node, NodeBehaviour newParent)
        {
            if (node == newParent)
            {
                Debug.LogError("new parent is this node");
                return false;
            }

            if (node.Parent == newParent)
            {
                Debug.LogError("new parent is already this nodes parent");
                return false;
            }

            if (node.HasDescendant(newParent))
            {
                Debug.LogError("new parent is a descendant of this node");
                return false;
            }


            return true;
        }
    }
}
