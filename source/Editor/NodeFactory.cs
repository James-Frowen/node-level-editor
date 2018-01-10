using System.Collections.Generic;
using System.Linq;
namespace NodeLevelEditor
{
    public class NodeFactory
    {
        public static NodeBehaviour CreateNode(NodeJson json)
        {
            json.ValidateNodeType();
            var creator = getNodeCreator(json);
            var node = creator.Create();
            node.json = json;
            json.behaviour = node;
            return node;
        }
        public static NodeBehaviour[] CreateNodes(NodeJson[] nodes)
        {
            foreach (var json in nodes)
            {
                json.ValidateNodeType();
            }

            var createdNodes = new List<NodeBehaviour>();

            var holes = nodes.Where(n => n.nodeType == NodeType.HOLE);
            var noParents = nodes.Where(n => n.nodeType != NodeType.HOLE).Where(n => n.parentName == "");
            var hasParents = nodes.Where(n => n.nodeType != NodeType.HOLE).Where(n => n.parentName != "");

            foreach (var json in noParents)
            {
                createdNodes.Add(CreateNode(json));
            }

            createNodesWithParent(hasParents, createdNodes);

            foreach (var json in holes)
            {
                createdNodes.Add(CreateNode(json));
            }

            return createdNodes.ToArray();
        }
        private static void createNodesWithParent(IEnumerable<NodeJson> nodes, List<NodeBehaviour> currentNodes, int lastLoopCount = -1)
        {
            var nextLoop = new List<NodeJson>();
            foreach (var json in nodes)
            {
                NodeBehaviour node;
                if (findAndSetParent(json, currentNodes, out node))
                {
                    currentNodes.Add(node);
                }
                else{
                    nextLoop.Add(json);
                }
            }

            if (nextLoop.Count > 0 && lastLoopCount != nextLoop.Count)
            {
                createNodesWithParent(nextLoop, currentNodes, nextLoop.Count);
            }
        }

        private static bool findAndSetParent(NodeJson json, List<NodeBehaviour> currentNodes, out NodeBehaviour node)
        {
            var parentExist = currentNodes.Any(n => n.name == json.parentName);

            if (parentExist)
            {
                node = CreateNode(json);
                var parent = currentNodes.Find(n => n.name == json.parentName);
                node.SetParent(parent);

                return true;
            }
            else
            {
                node = null;
                return false;   
            }
        }
        private static bool findAndSetParent(NodeJson json, List<NodeBehaviour> currentNodes)
        {
            NodeBehaviour b;
            return findAndSetParent(json, currentNodes, out b);
        }

        private static NodeCreator.Creator getNodeCreator(NodeJson json)
        {
            switch (json.nodeType)
            {
                case NodeType.EMPTY:
                    return new NodeCreator.EmptyCreator(json);
                case NodeType.CUBE:
                    return new NodeCreator.CubeCreator(json);
                case NodeType.HOLE:
                    return new NodeCreator.HoleCreator(json);
                case NodeType.QUAD:
                    return new NodeCreator.QuadCreator(json);
                default:
                    throw new System.Exception("Node type not found: " + json.nodeType);
            }
        }
    }
}