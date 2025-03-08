using Kickstarter.Singleton;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kickstarter.InputGenerator
{
    public class ActionMapLoader : PersistentSignleton<ActionMapLoader>
    {
        [SerializeField, EnumData(typeof(InputScheme))] private ActionMapScheme[] schemes;
        [SerializeField] private InputScheme initialInputScheme;

        private readonly Dictionary<InputScheme, ActionMapScheme> actionMapSchemes = new();

        protected override void Awake()
        {
            base.Awake();
            LoadDictionary(actionMapSchemes, schemes);

            var inputReceivers = schemes.SelectMany(s => s.InputReceivers).Distinct().ToArray();

            ActionMapManager.LoadActionMaps(inputReceivers);
            ActionMapManager.SetActiveActionMaps(actionMapSchemes[initialInputScheme].InputReceivers);
        }

        public void SetActiveActionMaps(InputScheme inputScheme)
        {
            ActionMapManager.SetActiveActionMaps(actionMapSchemes[inputScheme].InputReceivers);
        }

        [System.Serializable]
        private struct ActionMapScheme
        {
            [SerializeField] private InputReceiver[] inputReceivers;

            public InputReceiver[] InputReceivers => inputReceivers;
        }

        public static void LoadDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue[] values) where TKey : Enum
        {
            TKey[] keys = (TKey[]) Enum.GetValues(typeof(TKey));
            for (int i = 0; i < keys.Length; i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
        }
    }
}
