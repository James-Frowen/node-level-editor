﻿using System;
using UnityEditor;
using UnityEngine;

namespace ProjectGamma.LevelCreator
{
    public class NodeCreatorWindow : EditorWindow
    {
        public NodeManagerWindow parentWindow;
        public bool showNodeInScene;
        public NodeJson node;
        public NodeBehaviour parent;

        public void Update()
        {
            if (this.showNodeInScene)
            {
                this.showNode();
            }
        }

        private void showNode()
        {
            if (this.node.behaviour == null) { this.createNodeBehaviour(); }

            this.node.behaviour.UpdateFromJson();
        }

        private void createNodeBehaviour()
        {
            var behaviour = NodeFactory.CreateNode(this.node);
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
                    this.closeWindow();
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
            this.parentWindow.sceneChanged = true;
            this.closeWindow();
        }
        private void closeWindow()
        {
            this.node = null;
            this.Close();
        }


        public static void ShowWindow(NodeManagerWindow parentWindow)
        {
            var window = (NodeCreatorWindow)GetWindow(typeof(NodeCreatorWindow));
            window.minSize = new Vector2(50, 50);
            window.parentWindow = parentWindow;
            window.Show();
        }
    }
}
