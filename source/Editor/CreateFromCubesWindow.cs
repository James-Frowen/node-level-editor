using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public class CreateFromCubesWindow : EditorWindow
    {
        public Transform container;
        public NodeDataName nodeDataName;
        public bool hasCreated = false;

        public void OnGUI()
        {
            this.container = EditorGUILayout.ObjectField("container", this.container, typeof(Transform), true) as Transform;
            NodeDataName.DataFileName = EditorGUILayout.TextField("Data File Name", NodeDataName.DataFileName);


            GUI.enabled = this.container != null && !NodeDataManager.IsLoaded && !this.hasCreated;
            if (GUILayout.Button("Create"))
            {
                Debug.Log("Creating new room layout from cubes, old data will be deleted");
                NodeDataManager.Load(NodeDataName.DataFileName); 
                NodeDataManager.DeleteAll();

                var cubes = this.container.GetComponentsInChildren<Transform>();
                foreach (var cube in cubes)
                {
                    if (cube == this.container) { continue; }
                    if (cube.parent != this.container) { Debug.LogWarning("Container has 2nd level a child that will be ignored"); continue; }

                    var node = new NodeJson(NodeDataManager.NextName(NodeType.CUBE), cube.position, cube.localScale, "", NodeType.CUBE);

                    NodeDataManager.AddNode(node);
                }

                NodeFactory.CreateNodes(NodeDataManager.NodeJsons);

                this.container.gameObject.SetActive(false);
                this.hasCreated = true;
            }
            GUI.enabled = true;
            if (NodeDataManager.IsLoaded)
            {
                GUILayout.Label("Please Unload first");
            }


            GUIHelper.HorizontalLine();
            GUI.skin.label.wordWrap = true;
            GUILayout.Label("** This Action will overrite current data **");
            GUILayout.Label("If you dont want to lose any data please change the data file that is being used");

            GUI.enabled = this.hasCreated;
            if (GUILayout.Button("Save"))
            {
                NodeDataManager.Save();
            }
            GUI.enabled = true;
        }


        [MenuItem("Window/Node Level Editor/Create From Cubes")]
        public static void ShowWindow()
        {
            var window = (CreateFromCubesWindow)GetWindow(typeof(CreateFromCubesWindow));
            window.minSize = new Vector2(50, 50);
            window.Show();
        }
    }
}
