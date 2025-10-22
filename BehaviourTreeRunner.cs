using UnityEngine;

namespace BT
{
    public abstract class BehaviourTreeRunner : MonoBehaviour
    {
        protected Node root;
        protected Blackboard bb;

        protected virtual void Awake()
        {
            bb = new Blackboard();
            root = BuildTree();
            root.BindBlackboard(bb);
        }

        protected virtual void Update()
        {
            if (root != null) root.Tick();
        }

        protected abstract Node BuildTree();
    }
}
