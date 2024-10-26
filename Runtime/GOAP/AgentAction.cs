using System.Collections.Generic;
using UnityEngine;

namespace Kickstarter.GOAP
{
    public abstract class AgentAction : ScriptableObject
    {
        [SerializeField] private float cost;
        [SerializeField] private string[] preconditions;
        [SerializeField] private string[] effects;

        public float Cost => cost;

        public HashSet<AgentBelief> Preconditions { get; } = new HashSet<AgentBelief>();
        public HashSet<AgentBelief> Effects { get; } = new HashSet<AgentBelief>();

        public abstract bool CanPerform();
        public abstract bool IsComplete();

        public void TickAction(float deltaTime)
        {
            if (CanPerform())
                UpdateAction(deltaTime);

            if (!IsComplete())
                return;

            foreach (var effect in Effects)
                effect.Evaluate();
        }

        public virtual void Start()
        {

        }
        protected virtual void UpdateAction(float deltatime)
        {

        }
        public virtual void Stop()
        {

        }

        public virtual void Initialize(Dictionary<string, AgentBelief> beliefs, Behaviour[] dependencies)
        {
            foreach (var precondition in preconditions)
                Preconditions.Add(beliefs[precondition]);
            foreach (var effect in effects)
                Effects.Add(beliefs[effect]);

            InsertDependencies(dependencies);
            InsertBeliefs(beliefs);
        }

        protected virtual void InsertDependencies(Behaviour[] dependencies)
        {

        }
        protected virtual void InsertBeliefs(Dictionary<string, AgentBelief> beliefs)
        {

        }
    }
}
