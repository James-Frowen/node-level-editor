using System;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public class NodeManagerWindow : EditorWindow
    {
        public bool savingNodeData = false;
        public bool sceneChanged = false;
        public bool noSelectPrimitive = false;

        public void Update()
        {
            if (!NodeDataManager.IsLoaded) { return; }

            foreach (var node in NodeDataManager.NodeBehaviours)
            {
                if (node.NodeState.StateChanged())
                {
                    this.sceneChanged = true;
                }
            }

            if (this.noSelectPrimitive)
            {
                this.selectPrimativeParent();
            }

        }
        private void selectPrimativeParent()
        {
            var activeGameobject = Selection.activeGameObject;
            if (activeGameobject == null) { return; }
            var selectedNode = activeGameobject.GetComponent<NodeBehaviour>();
            if (selectedNode == null) { return; }

            if (selectedNode.noJson)
            {
                var parent = selectedNode.Parent;
                Selection.activeGameObject = parent.gameObject;
            }
        }

        public void OnEnable()
        {
            this.UnLoad(); // makes sure nothing is loaded at start

            NodeDataManager.onSave += this.nodeDataManager_onSave;
        }

        public void OnDisable()
        {
            NodeDataManager.onSave -= this.nodeDataManager_onSave;

            this.UnLoad();
        }

        private void nodeDataManager_onSave()
        {
            if (this.savingNodeData)
            {
                this.savingNodeData = false;
                return;
            }
        }

        public void OnGUI()
        {
            if (NodeDataManager.IsLoaded)
            {
                this.loadedGUI();
            }
            else
            {
                this.notLoadedGUI();
            }

            GUIHelper.HorizontalLine();
            this.noSelectPrimitive = GUILayout.Toggle(this.noSelectPrimitive, "no Select Primitive");
        }
        private void notLoadedGUI()
        {
            NodeDataName.DataFileName = EditorGUILayout.TextField("Data File Name", NodeDataName.DataFileName);
            if (GUILayout.Button("Load"))
            {
                this.Load();
            }
        }
        private void loadedGUI()
        {
            if (GUILayout.Button("unload"))
            {
                this.UnLoad();
                return;
            }
            if (GUILayout.Button("Save" + (NodeDataManager.NeedSave || this.sceneChanged ? " *" : "")))
            {
                this.Save();
            }
            if (GUILayout.Button("load (Overwrite Scene)"))
            {
                this.Load();
            }

            GUIHelper.HorizontalLine();
            if (GUILayout.Button("Normalise direct children"))
            {
                var node = this.activeNodeBehavior();
                NodeHelper.NormaliseScale(node);
            }
            GUIHelper.HorizontalLine();
            if (GUILayout.Button("New Node"))
            {
                NodeCreatorWindow.ShowWindow(this);
            }

        }

        private NodeBehaviour activeNodeBehavior()
        {
            var active = Selection.activeGameObject;
            if (active == null) { return null; }

            var behaviour = active.GetComponent<NodeBehaviour>();
            return behaviour;
        }

        public void Load()
        {
            Debug.Log("Loading Nodes");
            this.sceneChanged = false;

            this.DeleteAutoGenObjects();

            NodeBehaviour.RootParent = new NodeCreator.EmptyCreator(NodeBehaviour.DEFAULT_PARENT_NAME, Vector3.zero).Create();
            NodeDataManager.Load(NodeDataName.DataFileName);
            var nodes = NodeFactory.CreateNodes(NodeDataManager.NodeJsons);
        }
        public void UnLoad()
        {
            DeleteAutoGenObjects();
            NodeDataManager.Unload();
        }
        public void Save()
        {
            Debug.Log("Save Nodes");

            this.savingNodeData = true;
            this.sceneChanged = false;

            foreach (var node in NodeDataManager.NodeBehaviours)
            {
                if (node.nodeType == NodeType.HOLE) { continue; /* dont save hole? */ }

                node.UpdateJson(); // updates json
                node.NodeState.UpdateState();
            }
            NodeDataManager.Save();
        }
        public void DeleteAutoGenObjects()
        {
            var nodes = FindObjectsOfType<NodeBehaviour>();
            foreach (var node in nodes)
            {
                if (node != null && node.gameObject.activeInHierarchy) // dont destroy disabled objects
                {
                    DestroyImmediate(node.gameObject);
                }
            }
        }


        [MenuItem("Window/Node Level Editor/Node Manager")]
        public static void ShowWindow()
        {
            var window = (NodeManagerWindow)GetWindow(typeof(NodeManagerWindow));
            window.minSize = new Vector2(50, 50);
            window.name = "Node manager";
            window.Show();
        }
    }
}
