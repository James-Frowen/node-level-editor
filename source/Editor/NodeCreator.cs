using System;
using UnityEngine;

namespace NodeLevelEditor
{
    public static class NodeCreator
    {
        public delegate void OnNodeCreation(NodeBehaviour node);
        /// <summary>
        /// Use this event to modify new nodes. For example add tag or set layer
        /// </summary>
        public static event OnNodeCreation onNodeCreation;
        public static NodeBehaviour ProcessNewObject(GameObject go)
        {
            Debug.Assert(go.GetComponent<NodeBehaviour>() == null, "Component already has NodeBehaviour");
            var node = go.AddComponent<NodeBehaviour>();
            if (onNodeCreation !=null)
            {
                onNodeCreation(node);
            }
            return node;
        }
        public abstract class Creator
        {
            public string name;
            public Vector3 position;
            public Vector3 scale;
            public Quaternion rotation;


            public Creator(string name, Vector3 position)
            {
                this.name = name;
                this.position = position;
                this.scale = Vector3.one;
            }
            public Creator(string name, Vector3 position, Vector3 scale)
            {
                this.name = name;
                this.position = position;
                this.scale = scale;
            }
            public Creator(NodeJson json)
            {
                this.name = json.name;
                this.position = json.position;
                this.scale = json.scale;
            }

            protected virtual void setValues(GameObject go)
            {
                go.transform.localPosition = this.position;
                go.transform.localScale = this.scale;
                go.name = this.name;
            }
            public virtual NodeBehaviour Create()
            {
                var go = this.createGameObject();
                this.setValues(go);

                var node = ProcessNewObject(go);
                this.setNodeType(node);
                return node;
            }

            protected abstract GameObject createGameObject();
            protected abstract void setNodeType(NodeBehaviour node);
        }

        public class EmptyCreator : Creator
        {
            public EmptyCreator(string name, Vector3 position) : base(name, position) { }
            public EmptyCreator(string name, Vector3 position, Vector3 scale) : base(name, position, scale) { }
            public EmptyCreator(Vector3 position, Vector3 scale) : base(NodeDataManager.NextName("empty"), position, scale) { }
            public EmptyCreator(NodeJson json): base (json) { }

            protected override GameObject createGameObject()
            {
                return new GameObject();
            }

            protected override void setNodeType(NodeBehaviour node)
            {
                node.nodeType = NodeType.EMPTY;
            }
        }

        public class QuadCreator : Creator
        {
            public QuadCreator(string name, Vector3 position) : base(name, position) { }
            public QuadCreator(string name, Vector3 position, Vector3 scale) : base(name, position, scale) { }
            public QuadCreator(Vector3 position, Vector3 scale) : base(NodeDataManager.NextName("Quad"), position, scale) { }
            public QuadCreator(NodeJson json): base (json) { }

            protected override GameObject createGameObject()
            {
                return GameObject.CreatePrimitive(PrimitiveType.Quad);
            }

            protected override void setValues(GameObject go)
            {
                base.setValues(go);
                go.transform.localRotation = this.rotation;
            }

            protected override void setNodeType(NodeBehaviour node)
            {
                node.nodeType = NodeType.QUAD;
            }
        }

        public class CubeCreator : Creator
        {
            public CubeCreator(string name, Vector3 position) : base(name, position) { }
            public CubeCreator(string name, Vector3 position, Vector3 scale) : base(name, position, scale) { }
            public CubeCreator(Vector3 position, Vector3 scale) : base(NodeDataManager.NextName("cube") ,position, scale) { }
            public CubeCreator(NodeJson json): base (json) { }

            public override NodeBehaviour Create()
            {
                var parent = new EmptyCreator(this.name, this.position).Create();
                parent.nodeType = NodeType.CUBE;

                createQuad("top", Vector3.up, new Vector3(this.scale.x, this.scale.z, 1), Quaternion.Euler(-90, 0, 0), parent);
                createQuad("floor", Vector3.down, new Vector3(this.scale.x, this.scale.z, 1), Quaternion.Euler(90, 0, 0), parent);
                createQuad("left", Vector3.left, new Vector3(this.scale.z, this.scale.y, 1), Quaternion.Euler(0, -90, 0), parent);
                createQuad("right", Vector3.right, new Vector3(this.scale.z, this.scale.y, 1), Quaternion.Euler(0, 90, 0), parent);
                createQuad("front", Vector3.forward, new Vector3(this.scale.x, this.scale.y, 1), Quaternion.Euler(0, 0, 0), parent);
                createQuad("back", Vector3.back, new Vector3(this.scale.x, this.scale.y, 1), Quaternion.Euler(0, 180, 0), parent);

                return parent;
            }
            private NodeBehaviour createQuad(string nameDirection, Vector3 pos, Vector3 sca, Quaternion rot, NodeBehaviour parent)
            {
                var creator = new QuadCreator(string.Format("{0} {1}", this.name, nameDirection), Helper.Scale(pos, this.scale / 2), sca)
                {
                    rotation = rot
                };
                var node = creator.Create();

                node.SetParent(parent);
                node.noJson = true;
                return node;
            }

            protected override GameObject createGameObject()
            {
                throw new NotImplementedException();
            }

            protected override void setNodeType(NodeBehaviour node)
            {
                node.nodeType = NodeType.CUBE;
            }
        }

        public class HoleCreator : Creator
        {
            private const float SMALLEST_WALL = 0.0001f;
            public HoleCreator(string name, Vector3 position, Vector3 scale) : base(name, position, scale) { }
            public HoleCreator(NodeJson json): base (json) { }

            protected override GameObject createGameObject()
            {
                throw new NotImplementedException();
            }

            protected override void setValues(GameObject go)
            {
                throw new NotImplementedException();
            }

            public override NodeBehaviour Create()
            {
                var oldWall = this.getOldWall();

                var newWall = new EmptyCreator(oldWall.ToJson()).Create();
                newWall.transform.localRotation = oldWall.transform.localRotation;
                newWall.SetParent(oldWall.Parent);
                NodeBehaviour.MoveNodeChildren(oldWall, newWall);

                newWall.nodeType = NodeType.HOLE;

                var top = createWallPart(" top", oldWall, newWall);
                var bottom = createWallPart(" bottom", oldWall, newWall);
                var right = createWallPart(" right", oldWall, newWall);
                var left = createWallPart(" left", oldWall, newWall);

                updateTop(top);
                updateBot(bottom);
                updateRight(right);
                updateLeft(left);

                this.fixHoleOutSideOfQuad(top, bottom, right, left);

                this.deleteNode(oldWall);

                return newWall;
            }

            private void deleteNode(NodeBehaviour node)
            {
                node.Parent.RemoveChild(node);
                UnityEngine.Object.DestroyImmediate(node.gameObject);
            }

            private NodeBehaviour createWallPart(string nodeName, NodeBehaviour oldWall, NodeBehaviour parent)
            {
                var node = new QuadCreator(oldWall.ToJson()).Create();
                node.SetParent(parent);
                node.name = this.name + nodeName;
                node.noJson = true;

                return node;       
            }

            private void updateTop(NodeBehaviour node)
            {
                var w = node.transform;
                w.localRotation = Quaternion.identity;
                var startScale = w.localScale;
                var holeTop = this.position.y + this.scale.y / 2;
                var hs = w.localScale.y / 2;

                var s2 = hs - holeTop;
                var p2 = (hs + holeTop) / 2;

                w.localPosition = new Vector3(0, p2, 0);
                w.localScale += new Vector3(0, s2 - hs * 2, 0);

                w.localPosition = Helper.InverseScale(w.localPosition, startScale);
                w.localScale = Helper.InverseScale(w.localScale, startScale);

                if (Mathf.Abs(s2) < SMALLEST_WALL) {this.deleteNode(node); }
            }
            private void updateBot(NodeBehaviour node)
            {
                var w = node.transform;
                w.localRotation = Quaternion.identity;
                var startScale = w.localScale;
                var holeBot = this.position.y - this.scale.y / 2;
                var hs = w.localScale.y / 2;

                var s2 = hs + holeBot;
                var p2 = (-hs + holeBot) / 2;

                w.localPosition = new Vector3(0, p2, 0);
                w.localScale += new Vector3(0, s2 - hs * 2, 0);

                w.localPosition = Helper.InverseScale(w.localPosition, startScale);
                w.localScale = Helper.InverseScale(w.localScale, startScale);

                if (Mathf.Abs(s2) < SMALLEST_WALL) { this.deleteNode(node); }
            }
            private void updateRight(NodeBehaviour node)
            {
                var w = node.transform;
                w.localRotation = Quaternion.identity;
                var startScale = w.localScale;
                var holeRight = this.position.x + this.scale.x / 2;
                var hs = w.localScale.x / 2;

                var s2 = hs - holeRight;
                var p2 = (hs + holeRight) / 2;

                w.localPosition = new Vector3(p2, this.position.y, 0);
                w.localScale += new Vector3(s2 - hs * 2, this.scale.y - w.localScale.y, 0);

                w.localPosition = Helper.InverseScale(w.localPosition, startScale);
                w.localScale = Helper.InverseScale(w.localScale, startScale);

                if (Mathf.Abs(s2) < SMALLEST_WALL) { this.deleteNode(node); }
            }
            private void updateLeft(NodeBehaviour node)
            {
                var w = node.transform;
                w.localRotation = Quaternion.identity;
                var startScale = w.localScale;
                var holeLeft = this.position.x - this.scale.x / 2;
                var hs = w.localScale.x / 2;

                var s2 = hs + holeLeft;
                var p2 = (-hs + holeLeft) / 2;

                w.localPosition = new Vector3(p2, this.position.y, 0);
                w.localScale += new Vector3(s2 - hs * 2, this.scale.y - w.localScale.y, 0);

                w.localPosition = Helper.InverseScale(w.localPosition, startScale);
                w.localScale = Helper.InverseScale(w.localScale, startScale);

                if (Mathf.Abs(s2) < SMALLEST_WALL) { this.deleteNode(node); }
            }

            private void fixHoleOutSideOfQuad(NodeBehaviour top, NodeBehaviour bot, NodeBehaviour right, NodeBehaviour left)
            {
                if (top.transform.localScale.y < 0)
                {
                    var extraScale = top.transform.localScale.y;

                    right.transform.localPosition += Vector3.up * (extraScale / 2f);
                    right.transform.localScale += Vector3.up * (extraScale);
                    left.transform.localPosition += Vector3.up * (extraScale / 2f);
                    left.transform.localScale += Vector3.up * (extraScale);

                    UnityEngine.Object.DestroyImmediate(top.gameObject);

                }
                if (bot.transform.localScale.y < 0)
                {
                    var extraScale = bot.transform.localScale.y;

                    right.transform.localPosition -= Vector3.up * (extraScale / 2f);
                    right.transform.localScale += Vector3.up * (extraScale);
                    left.transform.localPosition -= Vector3.up * (extraScale / 2f);
                    left.transform.localScale += Vector3.up * (extraScale);

                    UnityEngine.Object.DestroyImmediate(bot.gameObject);
                }


                if (left.transform.localScale.x < 0)
                {
                    UnityEngine.Object.DestroyImmediate(left.gameObject);
                }
                if (right.transform.localScale.x < 0)
                {
                    UnityEngine.Object.DestroyImmediate(right.gameObject);
                }

            }

            private NodeBehaviour getOldWall()
            {
                return NodeDataManager.FindNode(this.name);
            }

            protected override void setNodeType(NodeBehaviour node)
            {
                node.nodeType = NodeType.HOLE;
            }
        }
    }
}
