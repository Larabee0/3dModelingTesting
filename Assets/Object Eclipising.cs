//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.2.0
//     from Assets/Object Eclipising.inputactions
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

public partial class @ObjectEclipising : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @ObjectEclipising()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Object Eclipising"",
    ""maps"": [
        {
            ""name"": ""MeshControls"",
            ""id"": ""d075ce7f-aa7e-42fc-acbc-c20d13ae9a13"",
            ""actions"": [
                {
                    ""name"": ""VertexMode"",
                    ""type"": ""Button"",
                    ""id"": ""07f395fe-8b2b-4b11-8e1c-0d0cbe03bc88"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""EdgeMode"",
                    ""type"": ""Button"",
                    ""id"": ""eba63fc0-96c5-4699-975d-caa78fa06fce"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""TriangleMode"",
                    ""type"": ""Button"",
                    ""id"": ""4f53a53a-2cda-4cc8-a139-873b49bd37d4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ObjectMode"",
                    ""type"": ""Button"",
                    ""id"": ""1d1cd43e-a42d-4154-8195-e52dc778ab7c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""LeftMouse"",
                    ""type"": ""Button"",
                    ""id"": ""ca9c3884-d8c5-4436-bc17-2de13b2f74b1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""VertexExtrude"",
                    ""type"": ""Button"",
                    ""id"": ""95c6e818-6f6d-40df-9df5-e72671a65d6a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""NormalExtrude"",
                    ""type"": ""Button"",
                    ""id"": ""4f20c38f-fc2d-44dc-8de3-30d7e38430c9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""TriangleFromVerts"",
                    ""type"": ""Button"",
                    ""id"": ""2bdaf2bf-0382-4899-a832-e43ab6bdca48"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Delete"",
                    ""type"": ""Button"",
                    ""id"": ""f9207091-e3db-4275-9e9b-b2f720238f9e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ModifierCtrl"",
                    ""type"": ""Button"",
                    ""id"": ""8503eeb0-6e37-4523-b4cc-6a587ef9cfe3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)"",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c3f82a39-fd19-4dc6-a8ff-cdad7d2b887c"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""VertexMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""704af207-ff61-4df1-bf2b-0883d3fc0f4c"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""EdgeMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""20fd57d9-43be-4e1b-aa9d-f556682e1b65"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""TriangleMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""795fa80d-1366-46a2-a0a0-dab018ffc871"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""LeftMouse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8727fe32-d5f1-43ad-b4c4-a43f1ec87afb"",
                    ""path"": ""<Keyboard>/v"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""VertexExtrude"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""90845c06-879d-4a3d-bb8a-19d2214aa9d7"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""NormalExtrude"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""112e9a8f-820e-4c72-b07b-1418d12369d4"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""TriangleFromVerts"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dadfec1b-6580-4a20-af3a-a2633d9370b0"",
                    ""path"": ""<Keyboard>/delete"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Delete"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7ed02904-1654-48cf-8be6-c02445e5c11d"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""ModifierCtrl"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ab1ae7de-42d4-48d7-818e-78fc8a0acb95"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""ObjectMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Touch"",
            ""bindingGroup"": ""Touch"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Joystick"",
            ""bindingGroup"": ""Joystick"",
            ""devices"": [
                {
                    ""devicePath"": ""<Joystick>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""XR"",
            ""bindingGroup"": ""XR"",
            ""devices"": [
                {
                    ""devicePath"": ""<XRController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // MeshControls
        m_MeshControls = asset.FindActionMap("MeshControls", throwIfNotFound: true);
        m_MeshControls_VertexMode = m_MeshControls.FindAction("VertexMode", throwIfNotFound: true);
        m_MeshControls_EdgeMode = m_MeshControls.FindAction("EdgeMode", throwIfNotFound: true);
        m_MeshControls_TriangleMode = m_MeshControls.FindAction("TriangleMode", throwIfNotFound: true);
        m_MeshControls_ObjectMode = m_MeshControls.FindAction("ObjectMode", throwIfNotFound: true);
        m_MeshControls_LeftMouse = m_MeshControls.FindAction("LeftMouse", throwIfNotFound: true);
        m_MeshControls_VertexExtrude = m_MeshControls.FindAction("VertexExtrude", throwIfNotFound: true);
        m_MeshControls_NormalExtrude = m_MeshControls.FindAction("NormalExtrude", throwIfNotFound: true);
        m_MeshControls_TriangleFromVerts = m_MeshControls.FindAction("TriangleFromVerts", throwIfNotFound: true);
        m_MeshControls_Delete = m_MeshControls.FindAction("Delete", throwIfNotFound: true);
        m_MeshControls_ModifierCtrl = m_MeshControls.FindAction("ModifierCtrl", throwIfNotFound: true);
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

    // MeshControls
    private readonly InputActionMap m_MeshControls;
    private IMeshControlsActions m_MeshControlsActionsCallbackInterface;
    private readonly InputAction m_MeshControls_VertexMode;
    private readonly InputAction m_MeshControls_EdgeMode;
    private readonly InputAction m_MeshControls_TriangleMode;
    private readonly InputAction m_MeshControls_ObjectMode;
    private readonly InputAction m_MeshControls_LeftMouse;
    private readonly InputAction m_MeshControls_VertexExtrude;
    private readonly InputAction m_MeshControls_NormalExtrude;
    private readonly InputAction m_MeshControls_TriangleFromVerts;
    private readonly InputAction m_MeshControls_Delete;
    private readonly InputAction m_MeshControls_ModifierCtrl;
    public struct MeshControlsActions
    {
        private @ObjectEclipising m_Wrapper;
        public MeshControlsActions(@ObjectEclipising wrapper) { m_Wrapper = wrapper; }
        public InputAction @VertexMode => m_Wrapper.m_MeshControls_VertexMode;
        public InputAction @EdgeMode => m_Wrapper.m_MeshControls_EdgeMode;
        public InputAction @TriangleMode => m_Wrapper.m_MeshControls_TriangleMode;
        public InputAction @ObjectMode => m_Wrapper.m_MeshControls_ObjectMode;
        public InputAction @LeftMouse => m_Wrapper.m_MeshControls_LeftMouse;
        public InputAction @VertexExtrude => m_Wrapper.m_MeshControls_VertexExtrude;
        public InputAction @NormalExtrude => m_Wrapper.m_MeshControls_NormalExtrude;
        public InputAction @TriangleFromVerts => m_Wrapper.m_MeshControls_TriangleFromVerts;
        public InputAction @Delete => m_Wrapper.m_MeshControls_Delete;
        public InputAction @ModifierCtrl => m_Wrapper.m_MeshControls_ModifierCtrl;
        public InputActionMap Get() { return m_Wrapper.m_MeshControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MeshControlsActions set) { return set.Get(); }
        public void SetCallbacks(IMeshControlsActions instance)
        {
            if (m_Wrapper.m_MeshControlsActionsCallbackInterface != null)
            {
                @VertexMode.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnVertexMode;
                @VertexMode.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnVertexMode;
                @VertexMode.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnVertexMode;
                @EdgeMode.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnEdgeMode;
                @EdgeMode.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnEdgeMode;
                @EdgeMode.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnEdgeMode;
                @TriangleMode.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnTriangleMode;
                @TriangleMode.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnTriangleMode;
                @TriangleMode.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnTriangleMode;
                @ObjectMode.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnObjectMode;
                @ObjectMode.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnObjectMode;
                @ObjectMode.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnObjectMode;
                @LeftMouse.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnLeftMouse;
                @LeftMouse.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnLeftMouse;
                @LeftMouse.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnLeftMouse;
                @VertexExtrude.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnVertexExtrude;
                @VertexExtrude.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnVertexExtrude;
                @VertexExtrude.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnVertexExtrude;
                @NormalExtrude.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnNormalExtrude;
                @NormalExtrude.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnNormalExtrude;
                @NormalExtrude.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnNormalExtrude;
                @TriangleFromVerts.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnTriangleFromVerts;
                @TriangleFromVerts.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnTriangleFromVerts;
                @TriangleFromVerts.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnTriangleFromVerts;
                @Delete.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnDelete;
                @Delete.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnDelete;
                @Delete.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnDelete;
                @ModifierCtrl.started -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnModifierCtrl;
                @ModifierCtrl.performed -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnModifierCtrl;
                @ModifierCtrl.canceled -= m_Wrapper.m_MeshControlsActionsCallbackInterface.OnModifierCtrl;
            }
            m_Wrapper.m_MeshControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @VertexMode.started += instance.OnVertexMode;
                @VertexMode.performed += instance.OnVertexMode;
                @VertexMode.canceled += instance.OnVertexMode;
                @EdgeMode.started += instance.OnEdgeMode;
                @EdgeMode.performed += instance.OnEdgeMode;
                @EdgeMode.canceled += instance.OnEdgeMode;
                @TriangleMode.started += instance.OnTriangleMode;
                @TriangleMode.performed += instance.OnTriangleMode;
                @TriangleMode.canceled += instance.OnTriangleMode;
                @ObjectMode.started += instance.OnObjectMode;
                @ObjectMode.performed += instance.OnObjectMode;
                @ObjectMode.canceled += instance.OnObjectMode;
                @LeftMouse.started += instance.OnLeftMouse;
                @LeftMouse.performed += instance.OnLeftMouse;
                @LeftMouse.canceled += instance.OnLeftMouse;
                @VertexExtrude.started += instance.OnVertexExtrude;
                @VertexExtrude.performed += instance.OnVertexExtrude;
                @VertexExtrude.canceled += instance.OnVertexExtrude;
                @NormalExtrude.started += instance.OnNormalExtrude;
                @NormalExtrude.performed += instance.OnNormalExtrude;
                @NormalExtrude.canceled += instance.OnNormalExtrude;
                @TriangleFromVerts.started += instance.OnTriangleFromVerts;
                @TriangleFromVerts.performed += instance.OnTriangleFromVerts;
                @TriangleFromVerts.canceled += instance.OnTriangleFromVerts;
                @Delete.started += instance.OnDelete;
                @Delete.performed += instance.OnDelete;
                @Delete.canceled += instance.OnDelete;
                @ModifierCtrl.started += instance.OnModifierCtrl;
                @ModifierCtrl.performed += instance.OnModifierCtrl;
                @ModifierCtrl.canceled += instance.OnModifierCtrl;
            }
        }
    }
    public MeshControlsActions @MeshControls => new MeshControlsActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    private int m_TouchSchemeIndex = -1;
    public InputControlScheme TouchScheme
    {
        get
        {
            if (m_TouchSchemeIndex == -1) m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
            return asset.controlSchemes[m_TouchSchemeIndex];
        }
    }
    private int m_JoystickSchemeIndex = -1;
    public InputControlScheme JoystickScheme
    {
        get
        {
            if (m_JoystickSchemeIndex == -1) m_JoystickSchemeIndex = asset.FindControlSchemeIndex("Joystick");
            return asset.controlSchemes[m_JoystickSchemeIndex];
        }
    }
    private int m_XRSchemeIndex = -1;
    public InputControlScheme XRScheme
    {
        get
        {
            if (m_XRSchemeIndex == -1) m_XRSchemeIndex = asset.FindControlSchemeIndex("XR");
            return asset.controlSchemes[m_XRSchemeIndex];
        }
    }
    public interface IMeshControlsActions
    {
        void OnVertexMode(InputAction.CallbackContext context);
        void OnEdgeMode(InputAction.CallbackContext context);
        void OnTriangleMode(InputAction.CallbackContext context);
        void OnObjectMode(InputAction.CallbackContext context);
        void OnLeftMouse(InputAction.CallbackContext context);
        void OnVertexExtrude(InputAction.CallbackContext context);
        void OnNormalExtrude(InputAction.CallbackContext context);
        void OnTriangleFromVerts(InputAction.CallbackContext context);
        void OnDelete(InputAction.CallbackContext context);
        void OnModifierCtrl(InputAction.CallbackContext context);
    }
}
