using UnityEngine;

namespace Kickstarter.InputGenerator
{
    public abstract class InputReceiver : ScriptableObject
    {
        protected InputActions inputs;

        public abstract bool IsActive { get; }

        public abstract string ActionMapName { get; }

        public abstract void Initialize(InputActions inputs);

        public abstract void EnableInputs(bool enable = true);
    }
}
