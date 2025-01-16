using System;
using UnityEngine;

namespace Kickstarter.GOAP
{
    public interface ISensor
    {
        public event Action OnTargetChanged;
        public Vector3 TargetPosition { get; }
        public bool IsTargetInRange { get => TargetPosition != Vector3.zero; }
        public GameObject Target { get; }
    }
}
