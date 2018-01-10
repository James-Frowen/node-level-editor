using UnityEngine;

namespace NodeLevelEditor
{
    [ExecuteInEditMode]
    public class NodeDataName : MonoBehaviour
    {
        public const string DEFAULT_DATA_FILE_NAME = "node-level-editor/data/nodes.json";

        private static NodeDataName _instance;
        private static NodeDataName Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = getNodeDataName();
                }

                return _instance;
            }
        }
        private static NodeDataName getNodeDataName()
        {
            var go = FindObjectOfType<NodeDataName>();
            if (go != null) { return go; }

            var newGo = new GameObject("NodeDataName");
            var comp = newGo.AddComponent<NodeDataName>();
            comp.dataFileName = DEFAULT_DATA_FILE_NAME;
            return comp;
        }

        public static string DataFileName
        {
            get
            {
                return Instance.dataFileName;
            }
            set
            {
                Instance.dataFileName = value;
            }
        }

        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                DestroyImmediate(this);
            }
        }
        public string dataFileName;
        private void Update()
        {
            if (!this.dataFileName.EndsWith(".json"))
            {
                this.dataFileName = this.dataFileName + ".json";
            }
        }
    }
}