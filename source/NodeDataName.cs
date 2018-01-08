using UnityEngine;

namespace NodeLevelEditor
{
    [ExecuteInEditMode]
    public class NodeDataName : MonoBehaviour {
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
            comp.dataFileName = NodeDataManager.ROOM_DATA_FILE;
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

        private void Update()
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
    }
}