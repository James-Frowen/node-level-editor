using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public static class NodeDataLoader
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
                Debug.LogWarning(string.Format("Node data file '{0}' does not exist, creating empty instance instead of loading", fileName));
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
            AssetDatabase.Refresh();
            NodeDataManager.AfterSave();
        }
        private static void createFolders(string path)
        {
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

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
}