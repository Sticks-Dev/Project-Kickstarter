using System;
using UnityEngine;

namespace Kickstarter.SerializedTypes
{
    [Serializable]
    public class SerializeableType : ISerializationCallbackReceiver
    {
        [SerializeField] private string assemblyQualifiedName = string.Empty;

        public Type Type { get; private set; }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            assemblyQualifiedName = Type?.AssemblyQualifiedName ?? assemblyQualifiedName;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!TryGetType(assemblyQualifiedName, out var type))
            {
                Debug.LogError($"Failed to find type from assembly qualified name: {assemblyQualifiedName}");
                return;
            }
            Type = type;
        }

        static bool TryGetType(string typeString, out Type type)
        {
            type = Type.GetType(typeString);
            return type != null || !string.IsNullOrEmpty(typeString);
        }
    }
}