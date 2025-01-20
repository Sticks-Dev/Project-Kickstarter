using UnityEngine;

namespace Kickstarter
{
    public abstract class InputReceiver : MonoBehaviour
    {
        protected static InputActions inputs;

        private void Awake()
        {
            if (inputs == null)
            {
                inputs = new InputActions();
            }
        }

        public abstract void EnableInputs(bool enable = true);
    }
}
