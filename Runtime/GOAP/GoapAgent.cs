using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Kickstarter.GOAP
{
    public class GoapAgent : MonoBehaviour
    {
        private AgentGoal lastGoal;
        private AgentGoal currentGoal;
        private AgentAction currentAction;
        private ActionPlan actionPlan;

        public Dictionary<string, AgentBelief> beliefs;
        public HashSet<AgentAction> actions;
        public HashSet<AgentGoal> goals;

        private IGoapPlanner gPlanner;

        #region UnityEvents
        private void Awake()
        {
            gPlanner = new GoapPlanner();
        }

        protected virtual void Start()
        {
            SetupBeliefs();
            SetupActions();
            SetupGoals();
        }

        private void Update()
        {
            if (currentAction == null)
            {
                CalculatePlan();

                if (actionPlan != null && actionPlan.Actions.Count > 0)
                {
                    currentGoal = actionPlan.AgentGoal;
                    currentAction = actionPlan.Actions.Pop();
                    if (currentAction.Preconditions.All(b => b.Evaluate()))
                        currentAction.StartAction();
                    else
                    {
                        currentAction = null;
                        currentGoal = null;
                    }
                }
            }

            UpdateStats(Time.deltaTime);

            if (actionPlan != null && currentAction != null)
            {
                currentAction.TickAction(Time.deltaTime);

                if (currentAction.IsComplete())
                {
                    currentAction.StopAction();
                    currentAction = null;

                    if (actionPlan.Actions.Count == 0)
                    {
                        lastGoal = currentGoal;
                        currentGoal = null;
                    }
                }
            }
        }
        #endregion

        #region Agent Behaviors
        [SerializeField] private GameObject statBeliefData;
        [SerializeField] private LocationBeliefData[] locationBeliefData;
        [SerializeField] private SensorBeliefData[] sensorBeliefData;
        [SerializeField] private GameObject actionData;
        [SerializeField] private GoalData[] goalData;

        private IStatBelief[] statBeliefs;

        private void SetupBeliefs()
        {
            beliefs = new Dictionary<string, AgentBelief>();
            var factory = new BeliefFactory(this, beliefs);

            statBeliefs = statBeliefData.GetComponents<IStatBelief>();

            foreach (var belief in statBeliefs)
                factory.AddStatBelief(belief.Name, belief.Evaluate);

            foreach (var locationBelief in locationBeliefData)
                factory.AddLocationBelief(locationBelief.Name, locationBelief.Distance, locationBelief.Location);

            foreach (var belief in sensorBeliefData)
            {
                factory.AddSensorBelief(belief.Name, belief.Sensor);
                belief.Sensor.OnTargetChanged += HandleTargetChange;
            }
        }

        private void SetupActions()
        {
            actions = new HashSet<AgentAction>();

            var actionData = this.actionData.GetComponents<AgentAction>();

            foreach (var action in actionData)
            {
                action.Initialize(beliefs);
                actions.Add(action);
            }
        }

        private void SetupGoals()
        {
            goals = new HashSet<AgentGoal>();

            foreach (var goal in goalData)
            {
                goals.Add(new AgentGoal.Builder(goal.Name)
                    .WithPriority(goal.Priority)
                    .WithDesiredEffect(beliefs[goal.DesiredEffect])
                    .Build());
            }
        }

        private void UpdateStats(float deltaTime)
        {
            foreach (var belief in statBeliefs)
            {
                if (belief is IStatUpdate updateBelief)
                    updateBelief.UpdateStatistic(deltaTime);
            }
        }
        #endregion

        private void HandleTargetChange()
        {
            currentAction = null;
            currentGoal = null;
        }

        private bool InRangeOf(Vector3 position, float range)
            => Vector3.Distance(transform.position, position) < range;

        private void CalculatePlan()
        {
            var priorityLevel = currentGoal?.Priority ?? 0;

            var goalsToCheck = goals;

            if (currentGoal != null)
            {
                goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
            }

            var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
            if (potentialPlan != null)
            {
                actionPlan = potentialPlan;
            }
        }

        #region Serialized Types
        [Serializable]
        private struct LocationBeliefData
        {
            [SerializeField] private string name;
            [SerializeField] private float distance;
            [SerializeField] private Transform location;

            public string Name => name;
            public float Distance => distance;
            public Transform Location => location;
        }
        
        [Serializable]
        private struct SensorBeliefData
        {
            [SerializeField] private string name;
            [SerializeField] private Sensor sensor;

            public string Name => name;
            public Sensor Sensor => sensor;
        }

        [Serializable]
        private struct GoalData
        {
            [SerializeField] private string name;
            [SerializeField] private float priority;
            [SerializeField] private string desiredEffect;

            public string Name => name;
            public float Priority => priority;
            public string DesiredEffect => desiredEffect;
        }
        #endregion
    }
}
