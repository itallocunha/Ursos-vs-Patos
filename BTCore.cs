using System.Collections.Generic;

namespace BTMini
{
    public enum State { Running, Success, Failure }

    public abstract class Node
    {
        public State state = State.Running;
        bool started;

        public State Tick()
        {
            if (!started) { OnStart(); started = true; }
            state = OnUpdate();
            if (state != State.Running) { OnStop(); started = false; }
            return state;
        }

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected abstract State OnUpdate();
    }

    public class Sequence : Node
    {
        readonly List<Node> children;
        int index;
        public Sequence(params Node[] nodes) { children = new List<Node>(nodes); }
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
        readonly List<Node> children;
        int index;
        public Selector(params Node[] nodes) { children = new List<Node>(nodes); }
        protected override void OnStart() { index = 0; }
        protected override State OnUpdate()
        {
            for (; index < children.Count; index++)
            {
                var s = children[index].Tick();
                if (s != State.Failure) return s; // running ou success
            }
            return State.Failure;
        }
    }

    public class WaitSeconds : Node
    {
        float t, dur;
        public WaitSeconds(float seconds) { dur = seconds; }
        protected override void OnStart() { t = 0f; }
        protected override State OnUpdate() { t += UnityEngine.Time.deltaTime; return (t >= dur) ? State.Success : State.Running; }
    }

    public class ActionNode : Node
    {
        public System.Func<State> act;
        public ActionNode(System.Func<State> a) { act = a; }
        protected override State OnUpdate() => act != null ? act() : State.Failure;
    }
}
