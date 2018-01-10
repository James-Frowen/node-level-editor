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
        private GameObject cutter;

        private delegate void TestHoleJson(NodeJson hole);
        private void runCutHoleTest(Vector3 cutterPos, Vector3 cutterSca, TestHoleJson testHoleJson, int holeCount = 2)
        {
            createCube(0, 0, 0);
            createCube(6, 0, 0);
            this.cutter.transform.position = cutterPos;
            this.cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(this.cutter);

            Assert.AreEqual(holeCount, this.nodesCreated.Count, string.Format("There are {0} holes", holeCount));
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
        public void CutsNoHolesWhenCubeIsFarAway()
        {
            Init();
            runCutHoleTest(
                new Vector3(3, 70, 0),
                new Vector3(2, 2, 2),
                (hole) =>
                {
                    NUnit.Framework.Assert.Fail("Hole was created when none should have been");
                }, 0);
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
        public void CutsHoleWhenCuboidIsNotInCenter()
        {
            runCutOneHoletest(
                new Vector3(3, 1.2f, 1.2f),
                new Vector3(3, 0.4f, 1.2f),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(-1.2f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has right Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(1.2f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has right Y sca");
                    }
                });
            runCutOneHoletest(
                new Vector3(3, -0.75f, -0.1f),
                new Vector3(3, 3.2f, 4f),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(0.1f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(-0.75f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(4f, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(3.2f, hole.scale.y, "hole has right Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(-0.1f, hole.position.x, "hole has right X pos");
                        Assert.AreApproximatelyEqual(-0.75f, hole.position.y, "hole has right Y pos");
                        Assert.AreApproximatelyEqual(4f, hole.scale.x, "hole has right X sca");
                        Assert.AreApproximatelyEqual(3.2f, hole.scale.y, "hole has right Y sca");
                    }
                });
        }

        [NUnit.Framework.Test]
        public void CutsHoleWhenInOtherSides()
        {
            Init();
            createCube(0, 0, 0);
            var cutterPos = new Vector3(3, 3, 3);
            var cutterSca = new Vector3(4, 4, 4);
            this.cutter.transform.position = cutterPos;
            this.cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(this.cutter);

            Assert.AreEqual(3, this.nodesCreated.Count, "There are 3 holes");
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                if (hole.behaviour.name.Contains("right"))
                {
                    Assert.AreApproximatelyEqual(-1.75f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(1.75f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(1.5f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(1.5f, hole.scale.y, "hole has right Y sca");
                }
                if (hole.behaviour.name.Contains("front"))
                {
                    Assert.AreApproximatelyEqual(1.75f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(1.75f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(1.5f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(1.5f, hole.scale.y, "hole has right Y sca");
                }
                if (hole.behaviour.name.Contains("top"))
                {
                    Assert.AreApproximatelyEqual(1.75f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(-1.75f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(1.5f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(1.5f, hole.scale.y, "hole has right Y sca");
                }
            }

            NodeDataManager.onAddNode -= this.onAddNode;
            CleanUp();  
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

        [NUnit.Framework.Test]
        public void CutHolesWhenCubeIsInCornerOutSideOfQuad()
        {
            Init();
            createCube(0, 0, 0);
            NodeDataManager.onAddNode += this.onAddNode;

            var cutterPos = new Vector3(3, 0, 0);
            var cutterSca = new Vector3(2, 2, 2);
            this.cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            this.cutter.transform.position = new Vector3(3, 0.8f, 1.2f);
            NodeHoleCutter.CutHoles(this.cutter);
            this.cutter.transform.position = new Vector3(-3, 0.8f, 1.2f);
            NodeHoleCutter.CutHoles(this.cutter);
            this.cutter.transform.position = new Vector3(0.8f, 3, 1.2f);
            NodeHoleCutter.CutHoles(this.cutter);
            this.cutter.transform.position = new Vector3(0.8f, -3, 1.2f);
            NodeHoleCutter.CutHoles(this.cutter);
            this.cutter.transform.position = new Vector3(0.8f, 1.2f, 3);
            NodeHoleCutter.CutHoles(this.cutter);
            this.cutter.transform.position = new Vector3(0.8f, 1.2f, -3);
            NodeHoleCutter.CutHoles(this.cutter);

            Assert.AreEqual(6, this.nodesCreated.Count, "There are 3 holes");
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                if (hole.behaviour.name.Contains("right"))
                {
                    Assert.AreApproximatelyEqual(-1.2f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(0.8f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "hole has right Y sca");
                }
                if (hole.behaviour.name.Contains("left"))
                {
                    Assert.AreApproximatelyEqual(1.2f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(0.8f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "hole has right Y sca");
                }
                if (hole.behaviour.name.Contains("front"))
                {
                    Assert.AreApproximatelyEqual(0.8f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "hole has right Y sca");
                }
                if (hole.behaviour.name.Contains("back"))
                {
                    Assert.AreApproximatelyEqual(-0.8f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "hole has right Y sca");
                }
                if (hole.behaviour.name.Contains("top"))
                {
                    Assert.AreApproximatelyEqual(0.8f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(-1.2f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "hole has right Y sca");
                }
                if (hole.behaviour.name.Contains("floor"))
                {
                    Assert.AreApproximatelyEqual(0.8f, hole.position.x, "hole has right X pos");
                    Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has right Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "hole has right X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "hole has right Y sca");
                }
            }

            NodeDataManager.onAddNode -= this.onAddNode;
            CleanUp();
        }

        [NUnit.Framework.Test]
        public void CutHoleWhenParentHaveScale()
        {
            Init();
            // create cube inside of empty with scale, but cube is still 5*5*5 to world
            var empty1Json = new NodeJson("empty1Json", new Vector3(0, 0, 0), new Vector3(5, 2, 1), "", NodeType.EMPTY) ;
            NodeDataManager.AddNode(empty1Json);
            NodeFactory.CreateNode(empty1Json);
            var empty2Json = new NodeJson("empty2Json", new Vector3(0, 0, 0), new Vector3(1, 2, 2), "", NodeType.EMPTY) { parentName= "empty1Json" };
            NodeDataManager.AddNode(empty2Json);
            NodeFactory.CreateNode(empty2Json);
            var cubeJson = new NodeJson("cubeJson", new Vector3(0, 0, 0), new Vector3(1, 1.2f, 2.4f), "", NodeType.CUBE) { parentName = "empty2Json" };
            NodeDataManager.AddNode(cubeJson);
            NodeFactory.CreateNode(cubeJson);

            var cutterPos = new Vector3(3, 0, 0);
            var cutterSca = new Vector3(2, 2, 2);
            this.cutter.transform.position = cutterPos;
            this.cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(this.cutter);

            var holeCount = 1;
            Assert.AreEqual(holeCount, this.nodesCreated.Count, string.Format("There are {0} holes", holeCount));
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                Assert.AreApproximatelyEqual(0, hole.position.x, "hole has right X pos");
                Assert.AreApproximatelyEqual(0, hole.position.y, "hole has right Y pos");
                Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has right X sca");
                Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has right Y sca");
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
            this.cutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this.nodesCreated = new List<NodeJson>();
        }
        public void CleanUp()
        {
            Object.DestroyImmediate(this.cutter);
            NodeDataManager.Unload();
            this.nodesCreated = null;
        }
    }
}