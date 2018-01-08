using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace NodeLevelEditor
{
    public class NodeDataManager
    {
        private static class NodeDataLoader
        {
            public static string GetFullPath(string fileName)
            {
                return Path.Combine(Application.dataPath, fileName);
            }
            public static NodeJsonHolder LoadAll(string fileName)
            {
                if (File.Exists(GetFullPath(fileName)))
                {
                    var steam = new StreamReader(GetFullPath(fileName));
                    var input = steam.ReadToEnd();
                    steam.Close();

                    var json = JsonUtility.FromJson<NodeJsonHolder>(input);

                    return json;
                }
                else
                {
                    return new NodeJsonHolder();
                }
            }
            public static void SaveAll(string fileName, NodeJsonHolder data)
            {
                backup(GetFullPath(fileName));

                var text = JsonUtility.ToJson(data, true);

                createFolders(GetFullPath(fileName));
                var steam = new StreamWriter(GetFullPath(fileName));
                steam.Write(text);
                steam.Close();
            }
            private static void createFolders(string path)
            {
                var folders = path.Split(new char[] { '/', '\\' });
                var current = "";
                var previous = "";
                var folderCount = folders.Length - 1; // -1 because last stirng in path is file
                for (int i = 0; i < folderCount; i++)
                {
                    current += folders[i];
                    var exist = AssetDatabase.IsValidFolder(current);

                    if (!exist)
                    {
                        AssetDatabase.CreateFolder(previous, folders[i]);
                    }

                    previous = current;
                    current += "/";
                }
            }
            private static void backup(string fullPath)
            {
                if (!File.Exists(fullPath)) { return; }

                var timeString = DateTime.Now.ToString("MM-dd-HH-mm-ss");

                var reader = new StreamReader(fullPath);
                var input = reader.ReadToEnd();
                reader.Close();

                var backupName = string.Format("{0}.{1}.tmp", fullPath, timeString);
                var writer = new StreamWriter(backupName);
                writer.Write(input);
                writer.Close();
            }
        }
        public delegate void OnSave();
        public static event OnSave onSave;

        public const string DEFAULT_DATA_FILE_NAME = "node-level-editor/data/nodes.json";
        public const bool AUTOLOAD = true;
        public static bool IsLoaded { get { return _instance != null; } }
        private static NodeDataManager _instance;
        private static NodeDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (AUTOLOAD) // this may cause issues if loading data other than from ROOM_DATA_FILE
                    {
                        Load();
                    }
                    else
                    {
                        throw new Exception("StructureManager is not loaded, Call Load first");
                    }
                }
                return _instance;
            }
        }
        public static void Unload()
        {
            _instance = null;
        }
        public static void Load(string fileName)
        {
            _instance = new NodeDataManager();
            var json = NodeDataLoader.LoadAll(fileName);
            _instance.loadData(json); // maybe need this line
        }
        public static void Load()
        {
            Load(DEFAULT_DATA_FILE_NAME);
        }
        public static void Save()
        {
            NodeDataLoader.SaveAll(DEFAULT_DATA_FILE_NAME, Instance.holder);
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
            }
            else
            {
                Debug.LogWarning("node already in holder");
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
                Debug.LogWarning("node is not in holder");
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

        private NodeJsonHolder holder;
        private List<NodeBehaviour> nodes;
        private bool needSave = false;
        private NodeDataManager()
        {
            this.nodes = new List<NodeBehaviour>();
        }
        private void loadData(NodeJsonHolder holder)
        {
            this.holder = holder;
        }
    }
}
