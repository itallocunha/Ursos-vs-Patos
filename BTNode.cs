using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BT
{
    public enum State { Running, Success, Failure }

    public abstract class Node
    {
        public State state { get; protected set; } = State.Running;
        public readonly List<Node> children = new List<Node>();
        public Blackboard blackboard;

        protected Node() { }
        protected Node(params Node[] nodes) { children.AddRange(nodes); }

        public void BindBlackboard(Blackboard bb)
        {
            blackboard = bb;
            foreach (var c in children) c.BindBlackboard(bb);
        }

        public State Tick()
        {
            if (state == State.Running) OnStart();
            state = OnUpdate();
            if (state != State.Running) OnStop();
            return state;
        }

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected abstract State OnUpdate();
    }

    public class Sequence : Node
    {
        int index = 0;
        public Sequence(params Node[] nodes) : base(nodes) { }
        protected override void OnStart() { index = 0; }
        protected override State OnUpdate()
        {
            for (; index < children.Count; index++)
            {
                var s = children[index].Tick();
                if (s != State.Success) return s;
            }
            return State.Success;
        }
    }

    public class Selector : Node
    {
        int index = 0;
        public Selector(params Node[] nodes) : base(nodes) { }
        protected override void OnStart() { index = 0; }
        protected override State OnUpdate()
        {
            for (; index < children.Count; index++)
            {
                var s = children[index].Tick();
                if (s != State.Failure) return s;
            }
            return State.Failure;
        }
    }

    public class WaitSeconds : Node
    {
        float duration, t;
        public WaitSeconds(float seconds) { duration = seconds; }
        protected override void OnStart() { t = 0f; }
        protected override State OnUpdate()
        {
            t += Time.deltaTime;
            return (t >= duration) ? State.Success : State.Running;
        }
    }
}
