using System;
using System.Linq;

namespace NodeLevelEditor
{
    public static class NodeType
    {
        public const string EMPTY = "empty";
        public const string CUBE = "cube";
        public const string HOLE = "hole";
        public const string QUAD = "quad";

        public static void ValidateNodeType(string nodeType)
        {
            var types = new string[]
            {
                EMPTY,
                CUBE,
                HOLE,
                QUAD
            };

            if (!types.Contains(nodeType))
            {
                throw new Exception(string.Format("Node type '{0}' not found in types array", nodeType));
            }
        }
    }
}