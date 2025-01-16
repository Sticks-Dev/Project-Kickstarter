using System;
using UnityEngine;
using Kickstarter.Extensions;

namespace Kickstarter.GOAP
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Sensor2D : MonoBehaviour, ISensor
    {
        [SerializeField] float detectionRadius = 5f;
        [SerializeField] float timerInterval = 1f;
        [SerializeField] private string targetTag = "Player";

        CircleCollider2D detectionRange;

        public event Action OnTargetChanged = delegate { };

        public Vector3 TargetPosition => Target ? Target.transform.position : Vector3.zero;
        public bool IsTargetInRange => TargetPosition != Vector3.zero;

        public GameObject Target => target;
        private GameObject target;
        Vector3 lastKnownPosition;
        CountdownTimer timer;

        #region UnityEvents
        private void Awake()
        {
            detectionRange = GetComponent<CircleCollider2D>();
            detectionRange.isTrigger = true;
            detectionRange.radius = detectionRadius;
        }

        private void Start()
        {
            timer = new CountdownTimer(timerInterval);
            timer.OnTimerStop += () => {
                UpdateTargetPosition(Target.OrNull());
                timer.Start();
            };
            timer.Start();
        }

        private void Update()
        {
            timer.Tick(Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag))
                return;
            UpdateTargetPosition(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag))
                return;
            UpdateTargetPosition();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsTargetInRange ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
        #endregion

        private void UpdateTargetPosition(GameObject target = null)
        {
            this.target = target;
            if (IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
            {
                lastKnownPosition = TargetPosition;
                OnTargetChanged.Invoke();
            }
        }
    }
}
