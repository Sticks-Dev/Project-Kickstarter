using System.Collections.Generic;
using System.Linq;

namespace Kickstarter.InputGenerator
{
    public static class ActionMapManager
    {
        public static InputReceiver PrimaryActionMap { get; private set; }
        private static readonly List<InputReceiver> activeMaps = new();
        public static InputActions InputActions { get; } = new InputActions();

        public static void LoadActionMaps(InputReceiver[] inputs)
        {
            InputActions.Enable();
            foreach (var input in inputs)
            {
                input.Initialize(InputActions);
                input.EnableInputs(false);
            }
        }

        public static void SetActiveActionMaps(InputReceiver[] actionMaps)
        {
            var mapsToEnable = actionMaps.Where(a => !activeMaps.Contains(a)).ToArray();
            for (int i = mapsToEnable.Length - 1; i >= 0; i--)
                EnableActionMap(mapsToEnable[i]);

            var mapsToDisable = activeMaps.Where(a => !actionMaps.Contains(a)).ToArray();
            for (int i = mapsToDisable.Length - 1; i >= 0; i--)
                DisableActionMap(mapsToDisable[i]);
        }

        private static void EnableActionMap(InputReceiver actionMap)
        {
            if (activeMaps.Contains(actionMap))
                return;
            actionMap.EnableInputs();
            activeMaps.Add(actionMap);
        }

        private static void DisableActionMap(InputReceiver actionMap)
        {
            if (!activeMaps.Contains(actionMap))
                return;
            actionMap.EnableInputs(false);
            activeMaps.Remove(actionMap);
        }

        public static bool IsActionMapActive(InputReceiver actionMap)
        {
            return activeMaps.Contains(actionMap);
        }
    }
}
