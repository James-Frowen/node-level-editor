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
                var parentExist = currentNodes.Any(n => n.name == json.parentName);

                if (parentExist)
                {
                    var node = CreateNode(json);
                    var parent = currentNodes.Find(n => n.name == json.parentName);
                    node.SetParent(parent);

                    currentNodes.Add(node);
                }
                else
                {
                    nextLoop.Add(json);
                }
            }

            if (nextLoop.Count > 0 && lastLoopCount != nextLoop.Count)
            {
                createNodesWithParent(nextLoop, currentNodes, nextLoop.Count);
            }
        }

        private static NodeCreator.Creator getNodeCreator(NodeJson json)
        {
            switch (json.nodeType)
            {
                case NodeType.EMPTY:
                    return new NodeCreator.EmptyCreator(json.name, json.position);
                case NodeType.CUBE:
                    return new NodeCreator.CubeCreator(json.name, json.position, json.scale);
                case NodeType.HOLE:
                    return new NodeCreator.HoleCreator(json.name, json.position, json.scale);
                default:
                    throw new System.Exception("Node type not found: " + json.nodeType);
            }
        }
    }
}