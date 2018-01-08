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

            LevelCreatorWindow.onHoleCreate += this.levelCreator_onHoleCreate;
            NodeDataManager.onSave += this.nodeDataManager_onSave;
        }

        public void OnDisable()
        {
            LevelCreatorWindow.onHoleCreate -= this.levelCreator_onHoleCreate;
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
        private void levelCreator_onHoleCreate(HoleCreator holeCreator)
        {
            var hole = new NodeJson(
                holeCreator.accecptedHoleParent.name,
                holeCreator.accecptedPosition,
                holeCreator.accecptedScale,
                "",
                NodeType.HOLE
            );
            NodeDataManager.AddNode(hole);
            this.sceneChanged = true;
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
            if (GUILayout.Button("Load"))
            {
                this.Load();
            }
        }
        private void loadedGUI()
        {
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
            Debug.Log("Load Nodes");
            this.sceneChanged = false;

            this.DeleteAutoGenObjects();

            NodeDataManager.Load();
            var nodes = NodeFactory.CreateNodes(NodeDataManager.NodeJsons);
            this.setRootParent(nodes);
        }
        private void setRootParent(NodeBehaviour[] nodes)
        {
            var root = new NodeCreator.EmptyCreator(NodeBehaviour.DEFAULT_PARENT_NAME, Vector3.zero).Create();
            foreach (var node in nodes)
            {
                if (node.Parent == null)
                {
                    node.SetParent(root);
                }
            }
        }
        public void UnLoad()
        {
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
            var gos = GameObject.FindGameObjectsWithTag("EditorAutoGen");
            foreach (var go in gos)
            {
                if (go != null && go.activeInHierarchy) // dont destroy disabled objects
                {
                    DestroyImmediate(go);
                }
            }
        }


        [MenuItem("Window/Level Editor/Node Manager")]
        public static void ShowWindow()
        {
            var window = (NodeManagerWindow)GetWindow(typeof(NodeManagerWindow));
            window.minSize = new Vector2(50, 50);
            window.name = "Node manager";
            window.Show();
        }
    }
}
