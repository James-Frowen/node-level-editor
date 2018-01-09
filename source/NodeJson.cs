using UnityEngine;
using System;
using System.Collections.Generic;

namespace NodeLevelEditor
{
    [Serializable]
    public class NodeJsonHolder
    {
        public int nameIndex;
        public List<NodeJson> nodes;

        public NodeJsonHolder()
        {
            this.nameIndex = 0;
            this.nodes = new List<NodeJson>();
        }
    }

    [Serializable]
    public class NodeJson
    {
        public string name;
        public Vector3 position;
        public Vector3 scale;
        public string parentName;
        public string nodeType;

        [NonSerialized] public NodeBehaviour behaviour;

        public NodeJson(string name, Vector3 position, Vector3 scale, string parentName, string nodeType)
        {
            this.name = name;
            this.position = position;
            this.scale = scale;
            this.parentName = parentName;
            this.nodeType = nodeType;

            ValidateNodeType();
        }

        public void ValidateNodeType()
        {
            NodeType.ValidateNodeType(this.nodeType);
        }
    }
}