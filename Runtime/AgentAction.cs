﻿using System.Collections.Generic;
using UnityEngine;

namespace Kickstarter.GOAP
{
    public abstract class AgentAction : MonoBehaviour
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

        public virtual void StartAction()
        {

        }
        protected virtual void UpdateAction(float deltatime)
        {

        }
        public virtual void StopAction()
        {

        }

        public virtual void Initialize(Dictionary<string, AgentBelief> beliefs)
        {
            foreach (var precondition in preconditions)
                Preconditions.Add(beliefs[precondition]);
            foreach (var effect in effects)
                Effects.Add(beliefs[effect]);

            InsertBeliefs(beliefs);
        }

        protected virtual void InsertBeliefs(Dictionary<string, AgentBelief> beliefs)
        {

        }
    }
}
