//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/_data/Gameplay Actions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Coolball.Input
{
    public partial class @GameplayInput: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @GameplayInput()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""Gameplay Actions"",
    ""maps"": [
        {
            ""name"": ""GameplayActions"",
            ""id"": ""7a939f79-c2d1-4bbf-922b-8e464ac2134c"",
            ""actions"": [
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""64775aea-2c40-4231-a285-f564a3043bde"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Touch"",
                    ""type"": ""Button"",
                    ""id"": ""ed45627d-9d76-49c4-86a6-a773ea7ae590"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ca417d1e-4aa6-43d3-b4d4-cd57e9f73083"",
                    ""path"": ""<Pointer>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""eaf0bdd5-d60c-4f7e-8ccc-58584548fdc0"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Touch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""031e80a4-f36b-4c9d-81c8-d621ec5d4b7c"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Touch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // GameplayActions
            m_GameplayActions = asset.FindActionMap("GameplayActions", throwIfNotFound: true);
            m_GameplayActions_MousePosition = m_GameplayActions.FindAction("MousePosition", throwIfNotFound: true);
            m_GameplayActions_Touch = m_GameplayActions.FindAction("Touch", throwIfNotFound: true);
        }

        ~@GameplayInput()
        {
            UnityEngine.Debug.Assert(!m_GameplayActions.enabled, "This will cause a leak and performance issues, GameplayInput.GameplayActions.Disable() has not been called.");
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // GameplayActions
        private readonly InputActionMap m_GameplayActions;
        private List<IGameplayActionsActions> m_GameplayActionsActionsCallbackInterfaces = new List<IGameplayActionsActions>();
        private readonly InputAction m_GameplayActions_MousePosition;
        private readonly InputAction m_GameplayActions_Touch;
        public struct GameplayActionsActions
        {
            private @GameplayInput m_Wrapper;
            public GameplayActionsActions(@GameplayInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @MousePosition => m_Wrapper.m_GameplayActions_MousePosition;
            public InputAction @Touch => m_Wrapper.m_GameplayActions_Touch;
            public InputActionMap Get() { return m_Wrapper.m_GameplayActions; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(GameplayActionsActions set) { return set.Get(); }
            public void AddCallbacks(IGameplayActionsActions instance)
            {
                if (instance == null || m_Wrapper.m_GameplayActionsActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_GameplayActionsActionsCallbackInterfaces.Add(instance);
                @MousePosition.started += instance.OnMousePosition;
                @MousePosition.performed += instance.OnMousePosition;
                @MousePosition.canceled += instance.OnMousePosition;
                @Touch.started += instance.OnTouch;
                @Touch.performed += instance.OnTouch;
                @Touch.canceled += instance.OnTouch;
            }

            private void UnregisterCallbacks(IGameplayActionsActions instance)
            {
                @MousePosition.started -= instance.OnMousePosition;
                @MousePosition.performed -= instance.OnMousePosition;
                @MousePosition.canceled -= instance.OnMousePosition;
                @Touch.started -= instance.OnTouch;
                @Touch.performed -= instance.OnTouch;
                @Touch.canceled -= instance.OnTouch;
            }

            public void RemoveCallbacks(IGameplayActionsActions instance)
            {
                if (m_Wrapper.m_GameplayActionsActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IGameplayActionsActions instance)
            {
                foreach (var item in m_Wrapper.m_GameplayActionsActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_GameplayActionsActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public GameplayActionsActions @GameplayActions => new GameplayActionsActions(this);
        public interface IGameplayActionsActions
        {
            void OnMousePosition(InputAction.CallbackContext context);
            void OnTouch(InputAction.CallbackContext context);
        }
    }
}
