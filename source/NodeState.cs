using UnityEngine;

namespace NodeLevelEditor
{
    [System.Serializable]
    public class NodeState
    {
        [System.NonSerialized] public INodeStateHolder holder;
        public Vector3 position;
        public Vector3 scale;
        public NodeState(Vector3 position, Vector3 scale)
        {
            this.position = position;
            this.scale = scale;
        }
        public NodeState(Transform transform)
        {
            this.position = transform.localPosition;
            this.scale = transform.localScale;
        }


        /// <summary>
        /// Checks the current position and scale agaist the state
        /// </summary>
        /// <returns></returns>
        public bool StateChanged(INodeStateHolder holder)
        {
            var current = new NodeState(holder.transform);
            return current != this;
        }
        /// <summary>
        /// Checks the current position and scale agaist the state
        /// </summary>
        /// <returns></returns>
        public bool StateChanged()
        {
            return this.StateChanged(this.holder);
        }
        /// <summary>
        /// Checking if the given pos and sca is the same as the current state
        /// </summary>
        /// <returns></returns>
        public bool SameState(Vector3 pos, Vector3 sca)
        {
            return this == new NodeState(pos, sca);
        }
        public void UpdateState(INodeStateHolder holder)
        {
            this.position = holder.transform.localPosition;
            this.scale = holder.transform.localScale;
        }
        public void UpdateState()
        {
            this.UpdateState(this.holder);
        }



        public override bool Equals(object obj)
        {
            var state = obj as NodeState;

            return (
                !ReferenceEquals(state, null) &&
                state.position == this.position &&
                state.scale == this.scale
            );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(NodeState obj1, NodeState obj2)
        {
            return (
                (ReferenceEquals(obj1, null) && ReferenceEquals(obj2, null)) ||
                (!ReferenceEquals(obj1, null) && obj1.Equals(obj2))
            );

        }
        public static bool operator !=(NodeState obj1, NodeState obj2)
        {
            return !(obj1 == obj2);
        }
    }
    public interface INodeStateHolder
    {
        NodeState NodeState { get; set; }
        Transform transform { get; }
    }
}