using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace NodeLevelEditor.Tests
{
    public class CutHoleTest
    {
        private List<NodeJson> nodesCreated;

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator CutsHoleWhenCubeIsInCenter()
        {
            Init();

            createCube(0, 0, 0);
            createCube(6, 0, 0);
            var cutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cutter.transform.position = new Vector3(3, 0, 0);
            cutter.transform.localScale = new Vector3(2, 2, 2);
            yield return null;
            
            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(cutter);
            yield return null;

            Assert.AreEqual(2, this.nodesCreated.Count);
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                Assert.AreApproximatelyEqual(0, hole.position.x);
                Assert.AreApproximatelyEqual(0, hole.position.y);
                Assert.AreApproximatelyEqual(2, hole.scale.x);
                Assert.AreApproximatelyEqual(2, hole.scale.y);
            }

            NodeDataManager.onAddNode -= this.onAddNode;
            CleanUp();
        }
        [UnityTest]
        public IEnumerator CutsHoleWhenCuboidIsInCenter()
        {
            Init();

            createCube(0, 0, 0);
            createCube(6, 0, 0);
            var cutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cutter.transform.position = new Vector3(3, 0, 0);
            cutter.transform.localScale = new Vector3(1.5f, 3, 2);
            yield return null;

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(cutter);
            yield return null;

            Assert.AreEqual(2, this.nodesCreated.Count);
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                Assert.AreApproximatelyEqual(0, hole.position.x);
                Assert.AreApproximatelyEqual(0, hole.position.y);
                Assert.AreApproximatelyEqual(2, hole.scale.x);
                Assert.AreApproximatelyEqual(3, hole.scale.y);
            }

            NodeDataManager.onAddNode -= this.onAddNode;
            CleanUp();
        }

        private void onAddNode(NodeJson node)
        {
            this.nodesCreated.Add(node);
        }

        private void createCube(int x, int y, int z)
        {
            var json = new NodeJson(NodeDataManager.NextName(NodeType.CUBE), new Vector3(x, y, z), Vector3.one * 5, "", NodeType.CUBE);
            NodeDataManager.AddNode(json);
            NodeFactory.CreateNode(json);
        }

        public void Init()
        {
            NodeDataManager.Load("");
            this.nodesCreated = new List<NodeJson>();
        }
        public void CleanUp()
        {
            NodeDataManager.Unload();
            this.nodesCreated = null;
        }
    }
}