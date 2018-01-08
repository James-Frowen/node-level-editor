using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace NodeLevelEditor
{
    [ExecuteInEditMode]
    public class NodeBehaviour : MonoBehaviour, INodeStateHolder
    {
        public const string DEFAULT_PARENT_NAME = "ROOT";

        [SerializeField] private NodeBehaviour parent;
        [SerializeField] private List<NodeBehaviour> children = new List<NodeBehaviour>();
        public string nodeType;
        /// <summary>
        /// true if this node behavior is created as of another node
        /// </summary>
        public bool noJson = false;

        public void UpdateFromJson()
        {
            this.UpdateFromJson(this.json);
        }
        public void UpdateFromJson(NodeJson updatedJson)
        {
            if (updatedJson == null) { return; }

            this.name = updatedJson.name;
            this.transform.localPosition = updatedJson.position;
            if (this.GetScale() != updatedJson.scale)
            {
                var newScale = Helper.InverseScale(updatedJson.scale, this.GetScale());
                this.transform.localScale = newScale;
                NodeHelper.NormaliseScale(this);
            }

            var parentName = this.HasParent ? this.parent.name : "";
            if (updatedJson.parentName != parentName)
            {
                this.RemoveParent();
                if (parentName != "")
                {
                    var newParent = NodeDataManager.FindNode(parentName);
                    this.SetParent(newParent);
                }
            }
        }

        [NonSerialized] private NodeState _nodeState;
        public NodeState NodeState
        {
            get
            {
                if (this._nodeState == null)
                {
                    this._nodeState = new NodeState(this.transform)
                    {
                        holder = this
                    };
                }
                return this._nodeState;
            }
            set
            {
                this._nodeState = value;
            }
        }

        public NodeBehaviour Parent { get { return this.parent; } }
        public NodeBehaviour[] Children { get { return this.children.ToArray(); } }

        [NonSerialized] public NodeJson json;

        public bool HasParent { get { return this.parent != null && this.parent.name != DEFAULT_PARENT_NAME; } }
        public bool ScaleFromChild { get { return this.nodeType == NodeType.CUBE; } }
        public Vector3 GetScale()
        {
            if (!this.ScaleFromChild)
            {
                return this.transform.localScale;
            }
            else
            {
                var front = this.children.Find(n => n.name == this.name + " front");
                var top = this.children.Find(n => n.name == this.name + " top");

                if (front == null)
                {
                    front = this.children.Find(n => n.name == this.name + " back");
                }
                if (top == null)
                {
                    top = this.children.Find(n => n.name == this.name + " floor");
                }

                return new Vector3(
                    front.transform.localScale.x,
                    front.transform.localScale.y,
                    top.transform.localScale.y
                );
            }
        }
        public Vector3 GetPosition()
        {
            return this.transform.localPosition;
        }

        public void AddChild(NodeBehaviour child)
        {
            Debug.Assert(child.parent == null, "child already has a parent");
            this.children.Add(child);
            child.parent = this;
            Helper.SetParentKeepLocals(child.transform, child.parent.transform);
        }
        public void SetParent(NodeBehaviour parent)
        {
            Debug.Assert(this.parent == null, "this already has a parent");
            parent.AddChild(this);
        }

        public void RemoveChild(NodeBehaviour child)
        {
            Debug.Assert(child.parent == this, "childs parent is not this");
            this.children.Remove(child);
            child.parent = null;
            Helper.SetParentKeepLocals(child.transform, null);
        }
        public void RemoveParent()
        {
            this.parent.RemoveChild(this);
        }

        public void ChangeParent(NodeBehaviour newParent)
        {
            this.RemoveParent();
            this.SetParent(newParent);
        }


        public bool HasChild(NodeBehaviour checkChild)
        {
            foreach (var child in this.children)
            {
                if (child == checkChild) { return true; }
            }
            return false;
        }
        public bool HasDescendant(NodeBehaviour checkChild)
        {
            if (this.HasChild(checkChild)) { return true; }
            foreach (var child in this.children)
            {
                if (child.HasDescendant(checkChild)) { return true; }
            }
            return false;
        }

        public NodeJson ToJson()
        {
            if (this.json == null)
            {
                var parentName = this.HasParent ? this.parent.name : "";
                this.json = new NodeJson(this.name, this.transform.localPosition, this.GetScale(), parentName, this.nodeType);
            }
            else
            {
                this.UpdateJson();
            }

            return this.json;
        }
        public void UpdateJson()
        {
            if (this.json != null)
            {
                var parentName = this.HasParent ? this.parent.name : "";

                this.json.position = this.GetPosition();
                this.json.scale = this.GetScale();
                this.json.parentName = parentName;
            }
        }

        public void OnEnable()
        {
            NodeDataManager.NodeEnabled(this);
        }
        public void OnDisable()
        {
            NodeDataManager.NodeDisabled(this);
        }
        public void OnDestroy()
        {
            if (!this.noJson && this.json != null)
            {
                NodeDataManager.RemoveNode(this.json);
            }
        }

        public static void MoveNodeChildren(NodeBehaviour oldNode, NodeBehaviour newNode)
        {
            foreach (var child in oldNode.children)
            {
                newNode.children.Add(child);
                child.parent = newNode;
            }
        }
    }
}