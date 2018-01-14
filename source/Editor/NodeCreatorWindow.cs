using System;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public class NodeCreatorWindow : EditorWindow
    {
        public bool showNodeInScene;
        public NodeJson node;
        public NodeBehaviour parent;

        public bool accepted = false;

        public void Update()
        {
            if (this.showNodeInScene)
            {
                this.showNode();
            }
        }

        private void showNode()
        {
            if (this.node == null) { return; }

            if (this.node.behaviour == null) { this.createNodeBehaviour(); }
           
            if (this.node.behaviour.NodeState.StateChanged())
            {
                this.node.behaviour.UpdateJson();
            }
            else
            {
                this.node.behaviour.UpdateFromJson();
            }
            this.node.behaviour.NodeState.UpdateState();
        }

        private void createNodeBehaviour()
        {
            NodeFactory.CreateNode(this.node);
        }

        public void OnEnable()
        {
            this.node = null;
        }
        private void createNewNode(string nodeType)
        {
            this.node = new NodeJson(NodeDataManager.NextName(nodeType), Vector3.zero, Vector3.one, "", nodeType);
        }

        public void OnGUI()
        {
            this.showNodeInScene = GUILayout.Toggle(this.showNodeInScene, "show node");
            if (this.node == null)
            {
                this.createNewNodeGUI();
            }
            else
            {
                switch (this.node.nodeType)
                {
                    case NodeType.EMPTY: 
                    case NodeType.QUAD:
                    case NodeType.CUBE:
                        this.regularNodeGUI();
                        break;
                    case NodeType.HOLE:
                        this.holeNodeGUI();
                        break;
                }
                GUIHelper.HorizontalLine();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Accept"))
                {
                    this.acceptNode();
                }
                if (GUILayout.Button("Discard"))
                {
                    this.discardNode();
                }
                GUILayout.EndHorizontal();
            }
               
        }

       
        private void createNewNodeGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(NodeType.EMPTY))
            {
                this.createNewNode(NodeType.EMPTY);
            }
            if (GUILayout.Button(NodeType.CUBE))
            {
                this.createNewNode(NodeType.CUBE);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(NodeType.QUAD))
            {
                this.createNewNode(NodeType.QUAD);
            }
            if (GUILayout.Button(NodeType.HOLE))
            {
                this.createNewNode(NodeType.HOLE);
            }
            GUILayout.EndHorizontal();
        }

        private void regularNodeGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.TextField("name", this.node.name);
            GUI.enabled = true;
            this.node.position = EditorGUILayout.Vector3Field("position", this.node.position);
            this.node.scale = EditorGUILayout.Vector3Field("scale", this.node.scale);
            this.parent = EditorGUILayout.ObjectField("parent", this.parent, typeof(NodeBehaviour), true) as NodeBehaviour;
            this.node.parentName = this.parent != null ? this.parent.name : "";
        }

        private void holeNodeGUI()
        {
            this.parent = EditorGUILayout.ObjectField("target", this.parent, typeof(NodeBehaviour), true) as NodeBehaviour;

            if (this.parent != null)
            {
                this.node.name = this.parent.name;
            }
            GUI.enabled = false;
            EditorGUILayout.TextField("name", this.node.name);
            GUI.enabled = true;
            this.node.position = EditorGUILayout.Vector3Field("position", this.node.position);
            this.node.scale = EditorGUILayout.Vector3Field("scale", this.node.scale);
        }   

        private void acceptNode()
        {
            NodeDataManager.AddNode(this.node);
            if (this.node.behaviour == null)
            {
                this.createNodeBehaviour();
            }
            this.accepted = true;
            this.Close();
        }
        private void discardNode()
        {
            this.Close();
        }   

        void OnDestroy()
        {
            if (!this.accepted && this.node.behaviour != null)
            {
                DestroyImmediate(this.node.behaviour.gameObject);
            }
            this.node = null;
        }

        public static void ShowWindow()
        {
            var window = (NodeCreatorWindow)GetWindow(typeof(NodeCreatorWindow));
            window.titleContent = new GUIContent("Create node");
            window.minSize = new Vector2(200, 270);
            window.Show();
        }
    }
}
