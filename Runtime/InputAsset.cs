using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kickstarter.Inputs
{
    /// <summary>
    /// Abstract base class for handling input actions in Unity.
    /// </summary>
    public abstract class InputAsset : ScriptableObject
    {
        [SerializeField] protected InputAction inputAction;

        /// <summary>
        /// Enables the input action associated with this asset.
        /// </summary>
        public void EnableInput() => inputAction.Enable();
        
        /// <summary>
        /// Initializes the input asset with devices and players.
        /// </summary>
        /// <param name="devices">Array of input devices.</param>
        /// <param name="players">Array of players.</param>
        public abstract void Initialize(InputDevice[] devices);
    }

    /// <summary>
    /// Abstract base class for handling typed input actions in Unity.
    /// </summary>
    /// <typeparam name="TType">The type of input action.</typeparam>
    public abstract class InputAsset<TType> : InputAsset where TType : struct
    {
        private Dictionary<InputDevice, Player.PlayerIdentifier> playerDevices;
        private Dictionary<Player.PlayerIdentifier, List<Action<TType>>> actionMap;

        /// <summary>
        /// Enables the InputAsset to register input listeners associated with specific players.
        /// </summary>
        /// <param name="devices">Array of input devices.</param>
        /// <param name="players">Array of player components to be assigned an input device.</param>
        public override void Initialize(InputDevice[] devices)
        {
            CreateMaps(devices);
            AddRegistration();
        }

        /// <summary>
        /// Registers a listener to handle input changes associated with a specific PlayerIdentifier.
        /// </summary>
        /// <param name="listener">Handler to listen to input changes.</param>
        /// <param name="playerIdentifier">Specifies the player with which the input is associated.</param>
        public void RegisterInput(Action<TType> listener, Player.PlayerIdentifier playerIdentifier)
        {
            if (!actionsRegistered)
                return;
            if (!actionMap.ContainsKey(playerIdentifier))
                actionMap.Add(playerIdentifier, new List<Action<TType>>());
            actionMap.TryGetValue(playerIdentifier, out var listeners);
            if (listeners.Contains(listener))
                return;
            listeners.Add(listener);
        }

        /// <summary>
        /// Deregisters a listener associated with a specific PlayerIdentifier.
        /// </summary>
        /// <param name="listener">Handler to deregister from input changes.</param>
        /// <param name="playerIdentifier">Specifies the player with which the input is associated.</param>
        public void DeregisterInput(Action<TType> listener, Player.PlayerIdentifier playerIdentifier)
        {
            if (!actionsRegistered)
                return;
            if (!actionMap.ContainsKey(playerIdentifier))
                actionMap.Add(playerIdentifier, new List<Action<TType>>());
            actionMap.TryGetValue(playerIdentifier, out var listeners);
            if (!listeners.Contains(listener))
                return;
            listeners.Remove(listener);
        }

        private static readonly Player.PlayerIdentifier[] playerIdentifiers = Enum.GetValues(typeof(Player.PlayerIdentifier))
            .Cast<Player.PlayerIdentifier>()
            .ToArray();

        private bool actionsRegistered;

        private void OnEnable() => actionsRegistered = false;

        private void CreateMaps(IReadOnlyCollection<InputDevice> devices)
        {
            playerDevices = new Dictionary<InputDevice, Player.PlayerIdentifier>();
            var inputDevices = devices.Where(d => d is not Mouse && d is not Keyboard).ToArray();
            playerDevices.Add(Keyboard.current, Player.PlayerIdentifier.Player1);
            for (int i = 0; i < Math.Min(inputDevices.Length, playerIdentifiers.Length); i++)
                playerDevices.Add(inputDevices[i], playerIdentifiers[i]);

            actionMap = new Dictionary<Player.PlayerIdentifier, List<Action<TType>>>();
        }

        private void AddRegistration()
        {
            inputAction.performed += SendInputsToListeners;
            inputAction.canceled += SendInputsToListeners;
            actionsRegistered = true;
        }

        private void SendInputsToListeners(InputAction.CallbackContext context)
        {
            var device = context.control.device is Mouse ? Keyboard.current : context.control.device;
            if (!playerDevices.TryGetValue(device, out var player))
                return;
            var value = context.ReadValue<TType>();
            actionMap.TryGetValue(player, out var listeners);
            if (listeners == null)
                return;
            for (int i = 0; i < listeners.Count; i++)
                listeners[i](value); // Avoiding foreach to allow deregistration during iteration but when deregistering, the total count is reduced preventing null reference exception
        }
    }
}
