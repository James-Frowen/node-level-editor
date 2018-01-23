using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

namespace NodeLevelEditor
{
    public class NodeDataManager
    {
        public delegate void OnSave();
        public static event OnSave onSave;

        public delegate void OnAddNode(NodeJson node);
        public static event OnAddNode onAddNode;

        public const bool AUTOLOAD = true;
        public static bool IsLoaded { get { return _instance != null; } }
        private static NodeDataManager _instance;
        private static NodeDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // this may cause issues if loading data other than from ROOM_DATA_FILE
                    //Load(NodeDataName.DataFileName);
                    throw new Exception("NodeDataManager is not loaded, Call Load first");
                }
                return _instance;
            }
        }
        public static void Unload()
        {
            _instance = null;
            DeleteAutoGenObjects();
        }
        public static void DeleteAutoGenObjects()
        {
            var nodes = UnityEngine.Object.FindObjectsOfType<NodeBehaviour>();
            foreach (var node in nodes)
            {
                if (node != null && node.gameObject.activeInHierarchy) // dont destroy disabled objects
                {
                    UnityEngine.Object.DestroyImmediate(node.gameObject);
                }
            }
        }
        public static void Load(string fileName, NodeJsonHolder data)
        {
            _instance = new NodeDataManager(fileName);
            _instance.loadData(data); // maybe need this line
        }
        public static string SaveFilePath()
        {
            return Instance.filePath;
        }
        public static NodeJsonHolder SaveDataHolder()
        {
            return Instance.holder;
        }
        public static void AfterSave()
        {
            Instance.needSave = false;
            if (onSave != null)
            {
                onSave();
            }
        }

        public static void DeleteAll()
        {
            Instance.holder.nodes.Clear();
            Instance.needSave = true;
        }

        public static void AddNode(NodeJson json)
        {
            if (!Instance.holder.nodes.Contains(json))
            {
                if (NodeJsonExist(json))
                {
                    Debug.LogError("node with name already exist but instances are out of sync");
                }

                Instance.holder.nodes.Add(json);
                if (json.behaviour != null)
                {
                    Instance.nodes.Add(json.behaviour);
                }
                Instance.needSave = true;

                if (onAddNode !=null)
                {
                    onAddNode(json);
                }
            }
            else
            {
                Debug.LogWarning("Can not add node already in holder");
            }
        }
        public static void RemoveNode(NodeJson node)
        {
            if (Instance.holder.nodes.Contains(node))
            {
                Instance.holder.nodes.Remove(node);
                Instance.needSave = true;
            }
            else
            {
                Debug.LogWarning(string.Format("Can not remove '{0}', it is not in holder", node.name));
            }
        }
        public static bool NodeJsonExist(NodeJson json)
        {
            return Instance.holder.nodes.Any(n => n.name == json.name);
        }

        public static NodeBehaviour FindNode(string name)
        {
            var node = Instance.nodes.Find(n => n.name == name);
            if (node == null)
            {
                throw new Exception("Node with name could not be found: " + name);
            }
            return node;
        }
        public static void NodeEnabled(NodeBehaviour node)
        {
            Instance.nodes.Add(node);
        }
        public static void NodeDisabled(NodeBehaviour node)
        {
            Instance.nodes.Remove(node);
        }

        //public static bool IsEmpty { get { return Instance.rooms.Count + Instance.holes.Count == 0; } }

        public static NodeJson[] NodeJsons { get { return Instance.holder.nodes.ToArray(); } }
        public static NodeBehaviour[] NodeBehaviours { get { return Instance.nodes.ToArray(); } }
        public static int NameIndex
        {
            get { return Instance.holder.nameIndex; }
            set { Instance.holder.nameIndex = value; }
        }
        public static string NextName(string structureType)
        {
            NameIndex++;
            return string.Format("{0} {1}", structureType, NameIndex);
        }
        public static bool NeedSave { get { return Instance.needSave; } }


        private static bool _finished = false;
        public static bool Finished { get { return _finished; } }
        public static void Finish()
        {
            _instance = null;
            _finished = true;
        }

        private NodeJsonHolder holder;
        private List<NodeBehaviour> nodes;
        private bool needSave = false;
        private string filePath;
        private NodeDataManager(string filePath)
        {
            this.nodes = new List<NodeBehaviour>();
            this.filePath = filePath;
        }
        private void loadData(NodeJsonHolder holder)
        {
            this.holder = holder;
        }
    }
}
