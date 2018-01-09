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

        private delegate void TestHoleJson(NodeJson hole);
        private void runCutHoleTest(Vector3 cutterPos, Vector3 cutterSca, TestHoleJson testHoleJson)
        {
            createCube(0, 0, 0);
            createCube(6, 0, 0);
            var cutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cutter.transform.position = cutterPos;
            cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(cutter);

            Assert.AreEqual(2, this.nodesCreated.Count, "There are 2 holes");
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                testHoleJson(hole);
            }

            NodeDataManager.onAddNode -= this.onAddNode;
        }
        private void runCutOneHoletest(Vector3 cutterPos, Vector3 cutterSca, Vector2 holePos, Vector2 holeSca)
        {
            runCutOneHoletest(cutterPos, cutterSca, (hole) => {
                Assert.AreApproximatelyEqual(holePos.x, hole.position.x, "hole has right X pos");
                Assert.AreApproximatelyEqual(holePos.y, hole.position.y, "hole has right Y pos");
                Assert.AreApproximatelyEqual(holeSca.x, hole.scale.x, "hole has right X sca");
                Assert.AreApproximatelyEqual(holeSca.y, hole.scale.y, "hole has right Y sca");
            });
        }
        private void runCutOneHoletest(Vector3 cutterPos, Vector3 cutterSca, TestHoleJson testHoleJson)
        {
            Init();
            runCutHoleTest(cutterPos, cutterSca, testHoleJson);
            CleanUp();
        }
        [NUnit.Framework.Test]
        public void CutsHoleWhenCubeIsInCenter()
        {
            runCutOneHoletest(
                new Vector3(3, 0, 0),
                new Vector3(2, 2, 2),
                new Vector2(0, 0),
                new Vector2(2, 2)
                );
        }
        [NUnit.Framework.Test]
        public void CutsHoleWhenCuboidIsInCenter()
        {
            runCutOneHoletest(
                new Vector3(3, 0, 0),
                new Vector3(1.5f, 3, 2),
                new Vector2(0, 0),
                new Vector2(2, 3)
                );

            runCutOneHoletest(
                new Vector3(3, 0, 0),
                new Vector3(3, 1.5f, 2),
                new Vector2(0, 0),
                new Vector2(2, 1.5f)
                );

            runCutOneHoletest(
                new Vector3(3, 0, 0),
                new Vector3(2, 3, 1),
                new Vector2(0, 0),
                new Vector2(1, 3)
                );
        }

        [NUnit.Framework.Test]
        public void CutsHoleWhenCubeIsNotInCenter()
        {
            runCutOneHoletest(
                new Vector3(3, 1, 0),
                new Vector3(2, 2, 2),
                new Vector2(0, 1),
                new Vector2(2, 2)
                );
            runCutOneHoletest(
                new Vector3(3, 0, -1),
                new Vector3(2, 2, 2),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(1, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(0, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has right Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(-1, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(0, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has right Y sca");
                    }
                });
            runCutOneHoletest(
                new Vector3(3, -1, 1),
                new Vector3(2, 2, 2),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(-1, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(-1, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has right Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(1, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(-1, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has right Y sca");
                    }
                });
        }

        [NUnit.Framework.Test]
        public void Cuts2HoleWhenCubeIsNotInCenter()
        {
            Init();
            runCutHoleTest(
                new Vector3(3, 1.2f, 1.2f),
                new Vector3(2, 2, 2),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(-1.2f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has right Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(1.2f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has right Y sca");
                    }
                });
            this.nodesCreated.Clear();
            runCutHoleTest(
                new Vector3(3, -1.2f, -1.2f),
                new Vector3(2, 2, 2),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(0.24f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(-0.01f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has right Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(-0.24f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(-0.01f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has right Y sca");
                    }
                });
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