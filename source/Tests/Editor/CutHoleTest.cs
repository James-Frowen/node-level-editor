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
        private void runCutHoleTest(Vector3 cutterPos, Vector3 cutterSca, TestHoleJson testHoleJson, int expecteHoleCount = 2)
        {
            this.cutter.transform.position = cutterPos;
            this.cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(this.cutter);

            Assert.AreEqual(expecteHoleCount, this.nodesCreated.Count, string.Format("There are not {0} holes", expecteHoleCount));
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
                Assert.AreApproximatelyEqual(holePos.x, hole.position.x, "hole has wrong X pos");
                Assert.AreApproximatelyEqual(holePos.y, hole.position.y, "hole has wrong Y pos");
                Assert.AreApproximatelyEqual(holeSca.x, hole.scale.x, "hole has wrong X sca");
                Assert.AreApproximatelyEqual(holeSca.y, hole.scale.y, "hole has wrong Y sca");
            });
        }
        private void runCutOneHoletest(Vector3 cutterPos, Vector3 cutterSca, TestHoleJson testHoleJson)
        {
            Init();
            createCube(0, 0, 0);
            createCube(6, 0, 0);
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
                        Assert.AreApproximatelyEqual(1, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(0, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(-1, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(0, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has wrong Y sca");
                    }
                });
            runCutOneHoletest(
                new Vector3(3, -1, 1),
                new Vector3(2, 2, 2),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(-1, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-1, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(1, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-1, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has wrong Y sca");
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
                        Assert.AreApproximatelyEqual(-1.2f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(1.2f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has wrong Y sca");
                    }
                });
            runCutOneHoletest(
                new Vector3(3, -0.75f, -0.1f),
                new Vector3(3, 3.2f, 4f),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(0.1f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-0.75f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(4f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(3.2f, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(-0.1f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-0.75f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(4f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(3.2f, hole.scale.y, "hole has wrong Y sca");
                    }
                });
        }

        [NUnit.Framework.Test]
        public void CutHolesWhenCubeCornerOutSideOfQuad()
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

            Assert.AreEqual(3, this.nodesCreated.Count, "There are not 3 holes");

            var expectedPosLong = new Vector2(0, 0.15f);
            var expectedPosShort = new Vector2(0.15f, 0.35f);

            var expectedScaleLong = new Vector2(1, 0.7f);
            var expectedScaleShort = new Vector2(0.7f, 0.3f);

            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);
                // json's hole centre will still be outside of quad, but when hole is created it is scaled to fit inside


                if (hole.behaviour.name.Contains("right"))
                {
                    foreach (var child in hole.behaviour.Children)
                    {
                        if (child.name.Contains("right bottom"))
                        {
                            Assert.AreApproximatelyEqual(expectedPosLong.x, child.transform.localPosition.x, "hole behaviour has wrong X pos");
                            Assert.AreApproximatelyEqual(-expectedPosLong.y, child.transform.localPosition.y, "hole behaviour has wrong Y pos");
                            Assert.AreApproximatelyEqual(expectedScaleLong.x, child.transform.localScale.x, "hole behaviour has wrong X sca");
                            Assert.AreApproximatelyEqual(expectedScaleLong.y, child.transform.localScale.y, "hole behaviour has wrong Y sca");
                        }
                        if (child.name.Contains("right right"))
                        {
                            Assert.AreApproximatelyEqual(expectedPosShort.x, child.transform.localPosition.x, "hole behaviour has wrong X pos");
                            Assert.AreApproximatelyEqual(expectedPosShort.y, child.transform.localPosition.y, "hole behaviour has wrong Y pos");
                            Assert.AreApproximatelyEqual(expectedScaleShort.x, child.transform.localScale.x, "hole behaviour has wrong X sca");
                            Assert.AreApproximatelyEqual(expectedScaleShort.y, child.transform.localScale.y, "hole behaviour has wrong Y sca");
                        }
                    }

                    Assert.AreApproximatelyEqual(-3f, hole.position.x, "hole has wrong X pos");
                    Assert.AreApproximatelyEqual(3f, hole.position.y, "hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(4f, hole.scale.x, "hole has wrong X sca");
                    Assert.AreApproximatelyEqual(4f, hole.scale.y, "hole has wrong Y sca");
                }
                if (hole.behaviour.name.Contains("front"))
                {
                    foreach (var child in hole.behaviour.Children)
                    {
                        if (child.name.Contains("front bottom"))
                        {
                            Assert.AreApproximatelyEqual(expectedPosLong.x, child.transform.localPosition.x, "hole behaviour has wrong X pos");
                            Assert.AreApproximatelyEqual(-expectedPosLong.y, child.transform.localPosition.y, "hole behaviour has wrong Y pos");
                            Assert.AreApproximatelyEqual(expectedScaleLong.x, child.transform.localScale.x, "hole behaviour has wrong X sca");
                            Assert.AreApproximatelyEqual(expectedScaleLong.y, child.transform.localScale.y, "hole behaviour has wrong Y sca");
                        }
                        if (child.name.Contains("front left"))
                        {
                            Assert.AreApproximatelyEqual(-expectedPosShort.x, child.transform.localPosition.x, "hole behaviour has wrong X pos");
                            Assert.AreApproximatelyEqual(expectedPosShort.y, child.transform.localPosition.y, "hole behaviour has wrong Y pos");
                            Assert.AreApproximatelyEqual(expectedScaleShort.x, child.transform.localScale.x, "hole behaviour has wrong X sca");
                            Assert.AreApproximatelyEqual(expectedScaleShort.y, child.transform.localScale.y, "hole behaviour has wrong Y sca");
                        }
                    }

                    Assert.AreApproximatelyEqual(3f, hole.position.x, "hole has wrong X pos");
                    Assert.AreApproximatelyEqual(3f, hole.position.y, "hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(4f, hole.scale.x, "hole has wrong X sca");
                    Assert.AreApproximatelyEqual(4f, hole.scale.y, "hole has wrong Y sca");
                }
                if (hole.behaviour.name.Contains("top"))
                {
                    foreach (var child in hole.behaviour.Children)
                    {
                        if (child.name.Contains("top top"))
                        {
                            Assert.AreApproximatelyEqual(expectedPosLong.x, child.transform.localPosition.x, "hole behaviour has wrong X pos");
                            Assert.AreApproximatelyEqual(expectedPosLong.y, child.transform.localPosition.y, "hole behaviour has wrong Y pos");
                            Assert.AreApproximatelyEqual(expectedScaleLong.x, child.transform.localScale.x, "hole behaviour has wrong X sca");
                            Assert.AreApproximatelyEqual(expectedScaleLong.y, child.transform.localScale.y, "hole behaviour has wrong Y sca");
                        }
                        if (child.name.Contains("top left"))
                        {
                            Assert.AreApproximatelyEqual(-expectedPosShort.x, child.transform.localPosition.x, "hole behaviour has wrong X pos");
                            Assert.AreApproximatelyEqual(-expectedPosShort.y, child.transform.localPosition.y, "hole behaviour has wrong Y pos");
                            Assert.AreApproximatelyEqual(expectedScaleShort.x, child.transform.localScale.x, "hole behaviour has wrong X sca");
                            Assert.AreApproximatelyEqual(expectedScaleShort.y, child.transform.localScale.y, "hole behaviour has wrong Y sca");
                        }
                    }

                    Assert.AreApproximatelyEqual(3f, hole.position.x, "hole has wrong X pos");
                    Assert.AreApproximatelyEqual(-3f, hole.position.y, "hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(4f, hole.scale.x, "hole has wrong X sca");
                    Assert.AreApproximatelyEqual(4f, hole.scale.y, "hole has wrong Y sca");
                }
            }

            NodeDataManager.onAddNode -= this.onAddNode;
            CleanUp();  
        }

        [NUnit.Framework.Test]
        public void CutHolesWhenCubeIsBiggerThanQuad()
        {
            Init();
            createCube(0, 0, 0);
            var cutterPos = new Vector3(3, 0, 0);
            var cutterSca = new Vector3(6, 6, 6);
            this.cutter.transform.position = cutterPos;
            this.cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(this.cutter);

            var expectedHoleCount = 5;
            Assert.AreEqual(expectedHoleCount, this.nodesCreated.Count, string.Format("There are not {0} holes", expectedHoleCount));

            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);
                // json's hole centre will still be outside of quad, but when hole is created it is scaled to fit inside

                if (hole.behaviour.name.Contains("right"))
                {
                    var expect = 0;
                    Assert.AreEqual(expect, hole.behaviour.Children.Length, string.Format("There are not {0} children", expect));
                }
                else
                {
                    var expect = 1;
                    Assert.AreEqual(expect, hole.behaviour.Children.Length, string.Format("There are not {0} children", expect));
                }
            }

            NodeDataManager.onAddNode -= this.onAddNode;
            CleanUp();
        }

        [NUnit.Framework.Test]
        public void Cuts2HoleInUniform()
        {
            Init();
            createCube(0, 0, 0);
            createCube(6, 0, 0);
            runCutHoleTest(
                new Vector3(3, 1.2f, 1.2f),
                new Vector3(2, 2, 2),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(-1.2f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(1.2f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(1.2f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(2, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(2, hole.scale.y, "hole has wrong Y sca");
                    }
                });
            this.nodesCreated.Clear();
            runCutHoleTest(
                new Vector3(3, -1.2f, -1.2f),
                new Vector3(2, 2, 2),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(0.24f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-0.01f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(-0.24f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-0.01f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.4f, hole.scale.y, "hole has wrong Y sca");
                    }
                });
            CleanUp();
        }

        [NUnit.Framework.Test]
        public void Cuts2HoleInNonUniformRoom()
        {
            Init();
            var cube1 = new NodeJson(NodeDataManager.NextName(NodeType.CUBE), new Vector3(0, 0, 0), new Vector3(4, 3, 5), "", NodeType.CUBE);
            NodeDataManager.AddNode(cube1);
            NodeFactory.CreateNode(cube1);
            var cube2 = new NodeJson(NodeDataManager.NextName(NodeType.CUBE), new Vector3(6, 0, 0), new Vector3(6, 5, 4), "", NodeType.CUBE);
            NodeDataManager.AddNode(cube2);
            NodeFactory.CreateNode(cube2);

            runCutHoleTest(
                new Vector3(2.5f, -0.5f, 1f),
                new Vector3(2, 0.5f, 0.8f),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(-1f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-0.5f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(0.8f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.5f, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(1f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-0.5f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(0.8f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.5f, hole.scale.y, "hole has wrong Y sca");
                    }
                });
            this.nodesCreated.Clear();
            runCutHoleTest(
                new Vector3(2.5f, 0.75f, -0.5f),
                new Vector3(2, 0.5f, 0.8f),
                (hole) => {
                    if (hole.behaviour.name.Contains("right"))
                    {
                        Assert.AreApproximatelyEqual(0.1f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(0.125f/3f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(0.16f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.5f/3f, hole.scale.y, "hole has wrong Y sca");
                    }
                    if (hole.behaviour.name.Contains("left"))
                    {
                        Assert.AreApproximatelyEqual(-0.125f, hole.position.x, "hole has wrong X pos");
                        Assert.AreApproximatelyEqual(-0.075f, hole.position.y, "hole has wrong Y pos");
                        Assert.AreApproximatelyEqual(0.2f, hole.scale.x, "hole has wrong X sca");
                        Assert.AreApproximatelyEqual(0.1f, hole.scale.y, "hole has wrong Y sca");
                    }
                });
            CleanUp();
        }

        [NUnit.Framework.Test]
        public void CutsHoleWhenInOtherSides()
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

            Assert.AreEqual(6, this.nodesCreated.Count, "There are not 6 holes");
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                if (hole.behaviour.name.Contains("right"))
                {
                    Assert.AreApproximatelyEqual(-1.2f, hole.position.x, "right hole has wrong X pos");
                    Assert.AreApproximatelyEqual(0.8f, hole.position.y, "right hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "right hole has wrong X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "right hole has wrong Y sca");
                }
                if (hole.behaviour.name.Contains("left"))
                {
                    Assert.AreApproximatelyEqual(1.2f, hole.position.x, "left hole has wrong X pos");
                    Assert.AreApproximatelyEqual(0.8f, hole.position.y, "left hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "left hole has wrong X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "left hole has wrong Y sca");
                }
                if (hole.behaviour.name.Contains("front"))
                {
                    Assert.AreApproximatelyEqual(0.8f, hole.position.x, "front hole has wrong X pos");
                    Assert.AreApproximatelyEqual(1.2f, hole.position.y, "front hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "front hole has wrong X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "front hole has wrong Y sca");
                }
                if (hole.behaviour.name.Contains("back"))
                {
                    Assert.AreApproximatelyEqual(-0.8f, hole.position.x, "back hole has wrong X pos");
                    Assert.AreApproximatelyEqual(1.2f, hole.position.y, "back hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "back hole has wrong X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "back hole has wrong Y sca");
                }
                if (hole.behaviour.name.Contains("top"))
                {
                    Assert.AreApproximatelyEqual(0.8f, hole.position.x, "top hole has wrong X pos");
                    Assert.AreApproximatelyEqual(-1.2f, hole.position.y, "top hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "top hole has wrong X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "top hole has wrong Y sca");
                }
                if (hole.behaviour.name.Contains("floor"))
                {
                    Assert.AreApproximatelyEqual(0.8f, hole.position.x, "floor hole has wrong X pos");
                    Assert.AreApproximatelyEqual(1.2f, hole.position.y, "floor hole has wrong Y pos");
                    Assert.AreApproximatelyEqual(2f, hole.scale.x, "floor hole has wrong X sca");
                    Assert.AreApproximatelyEqual(2f, hole.scale.y, "floor hole has wrong Y sca");
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
            var empty2Json = new NodeJson("empty2Json", new Vector3(0, 0, 0), new Vector3(1, 2, 2), "", NodeType.EMPTY) { parentName= "empty1Json" };
            NodeDataManager.AddNode(empty2Json);
            var cubeJson = new NodeJson("cubeJson", new Vector3(0, 0, 0), new Vector3(1, 1.2f, 2.4f), "", NodeType.CUBE) { parentName = "empty2Json" };
            NodeDataManager.AddNode(cubeJson);
            NodeFactory.CreateNodes(NodeDataManager.NodeJsons);

            var cutterPos = new Vector3(3, 0.5f, -1);
            var cutterSca = new Vector3(2, 2, 2);
            this.cutter.transform.position = cutterPos;
            this.cutter.transform.localScale = cutterSca;
            Debug.Log(string.Format("testing cutter with pos:{0} sca:{1}", cutterPos, cutterSca));

            NodeDataManager.onAddNode += this.onAddNode;
            NodeHoleCutter.CutHoles(this.cutter);

            var holeCount = 1;
            Assert.AreEqual(holeCount, this.nodesCreated.Count, string.Format("There are not {0} holes", holeCount));
            foreach (var hole in this.nodesCreated)
            {
                Assert.AreEqual(NodeType.HOLE, hole.nodeType);

                Assert.AreApproximatelyEqual(0.2f, hole.position.x, "hole has wrong X pos");
                Assert.AreApproximatelyEqual(0.125f, hole.position.y, "hole has wrong Y pos");
                Assert.AreApproximatelyEqual(0.4f, hole.scale.x, "hole has wrong X sca");
                Assert.AreApproximatelyEqual(0.5f, hole.scale.y, "hole has wrong Y sca");
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
            NodeHoleCutter.ResetIgnores();
        }
        public void CleanUp()
        {
            Object.DestroyImmediate(this.cutter);
            NodeDataManager.Unload();
            this.nodesCreated = null;
        }
    }
}