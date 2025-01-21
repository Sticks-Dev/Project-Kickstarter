using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Kickstarter
{
    public class InputActionEditorExtension : EditorWindow
    {
        private InputActionAsset inputActionAsset;
        private string[] actionMapNames = new string[0];
        private int selectedActionMapIndex = 0;
        private string classNamespace = "TEMP";
        private string actionMapName = "TEMP";

        [MenuItem("Tools/Kickstarter/Input ActionMap Generator")]
        public static void ShowWindow()
        {
            GetWindow<InputActionEditorExtension>("Input ActionMap Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Input ActionMap Generator", EditorStyles.boldLabel);

            var newInputActionAsset = (InputActionAsset) EditorGUILayout.ObjectField("Input Action Asset", inputActionAsset, typeof(InputActionAsset), false);
            if (newInputActionAsset != inputActionAsset)
            {
                inputActionAsset = newInputActionAsset;
                LoadActionMapNames();
            }

            if (inputActionAsset == null)
            {
                GUILayout.Label("Select an InputActionAsset to begin.", EditorStyles.helpBox);
                return;
            }

            string @namespace = EditorGUILayout.TextField(label: "InputActions Namespace", classNamespace);
            if (@namespace != classNamespace)
                classNamespace = @namespace;

            int newSelectedActionMapIndex = EditorGUILayout.Popup("Action Map", selectedActionMapIndex, actionMapNames);

            if (newSelectedActionMapIndex != selectedActionMapIndex)
            {
                selectedActionMapIndex = newSelectedActionMapIndex;
                actionMapName = actionMapNames[selectedActionMapIndex];
            }

            GUILayout.Label("Selected Action Map: " + actionMapName, EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button($"Create {actionMapName}Inputs.cs"))
                CreateActionMapClass(inputActionAsset);
        }

        private void LoadActionMapNames()
        {
            if (inputActionAsset == null)
            {
                actionMapNames = new string[0];
                selectedActionMapIndex = 0;
                actionMapName = "TEMP";
                return;
            }

            var maps = inputActionAsset.actionMaps;
            actionMapNames = maps.Select(m => m.name).ToArray();
            selectedActionMapIndex = 0;
            actionMapName = actionMapNames.Length > 0 ? actionMapNames[0] : "TEMP";
        }

        private void CreateActionMapClass(InputActionAsset asset)
        {
            string className = $"{actionMapName}Inputs";
            string fileName = EditorUtility.SaveFilePanel("Create New C# Script", "Assets", $"{className}", "cs");

            // If the user cancels or doesn't specify a name, exit
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.Log("Script creation canceled.");
                return;
            }

            // Make sure the file is saved within the Assets folder
            if (!fileName.StartsWith(Application.dataPath))
            {
                Debug.LogError("The file must be saved within the Assets folder.");
                return;
            }

            var actions = GetActions(asset);
            string[] actionsNames = actions.Select(a => a.name)
                                           .ToArray();

            string[] eventVariables = CreateEventVariables(actions);
            string[] interfaceFunctions = CreateInterfaceFunctions(actions);

            string eventVariablesString = string.Join("\n", eventVariables);
            string interfaceFunctionsString = string.Join("\n", interfaceFunctions);

            string rootNamespace = EditorSettings.projectGenerationRootNamespace;

            // Define the basic class template
            string scriptTemplate =
                    $"using {rootNamespace};\n";
            if (rootNamespace == string.Empty)
                scriptTemplate = string.Empty;
            if (classNamespace != string.Empty)
                scriptTemplate += $"using static {classNamespace}.InputActions;\n";
            else
                scriptTemplate +=
                    $"using static InputActions;\n";
            scriptTemplate +=
                    $"using System;\n" +
                    $"using UnityEngine;\n" +
                    $"using UnityEngine.InputSystem;\n" +
                    $"\n";
            if (rootNamespace != string.Empty)
                scriptTemplate +=
                    $"namespace {rootNamespace}\n" +
                    $"{{\n";
            scriptTemplate +=
                    $"    public class {className} : InputReceiver, I{actionMapName}Actions\n" + // Add the appropriate interface
                    $"    {{\n" +
                    $"        public override void EnableInputs(bool enable = true)\n" +
                    $"        {{\n" +
                    $"            inputs.{actionMapName}.SetCallbacks(this);" +
                    $"            if (enable)\n" +
                    $"            {{\n" +
                    $"                inputs.{actionMapName}.Enable();\n" +
                    $"                return;\n" +
                    $"            }}\n" +
                    $"            inputs.{actionMapName}.Disable();\n" +
                    $"        }}\n" +
                    $"        \n" +
                    $"{eventVariablesString}\n" +
                    $"        \n" +
                    $"{interfaceFunctionsString}\n" +
                    $"    }}\n";
            if (rootNamespace != string.Empty)
                scriptTemplate += $"}}\n";

            // Write the script to the specified file
            File.WriteAllText(fileName, scriptTemplate);

            // Refresh the Asset Database to make the new script visible in the Editor
            AssetDatabase.Refresh();

            Debug.Log($"New script created at {fileName}");
        }

        private InputAction[] GetActions(InputActionAsset asset)
        {
            return asset.actionMaps[Array.IndexOf(actionMapNames, actionMapName)].actions.ToArray();
        }

        private string[] CreateEventVariables(InputAction[] actions)
        {
            var variables = new string[actions.Length];
            for (int i = 0; i < actions.Length; i++)
            {
                variables[i] = CreateEventVariable(actions[i]);
            }

            return variables;
        }

        private string CreateEventVariable(InputAction action)
        {
            var type = action.type;

            if (type == InputActionType.Button)
            {
                return
                    $"        public event Action On{action.name.Replace(" ", "")} = delegate {{ }};";
            }

            string inputType = GetActionReturnType(action);
            if (inputType == null)
                throw new Exception($"Unknown control type for action '{action.name.Replace(" ", "")}'");

            return $"        public event Action<{inputType}> On{action.name.Replace(" ", "")} = delegate {{ }};";
        }

        private string[] CreateInterfaceFunctions(InputAction[] actions)
        {
            var functions = new string[actions.Length];
            for (int i = 0; i < actions.Length; i++)
            {
                functions[i] = CreateInterfaceFunction(actions[i]);
            }

            return functions;
        }

        private string CreateInterfaceFunction(InputAction action)
        {
            var type = action.type;

            if (type == InputActionType.Button)
            {
                return
                    $"        void I{actionMapName}Actions.On{action.name.Replace(" ", "")}(InputAction.CallbackContext context)\n" +
                    $"        {{\n" +
                    $"            if (!context.performed)\n" +
                    $"                return;\n" +
                    $"            On{action.name.Replace(" ", "")}?.Invoke();\n" +
                    $"        }}\n";
            }

            string inputType = GetActionReturnType(action).ToString();
            if (inputType == "Unknown")
                throw new Exception($"Unknown control type for action '{action.name.Replace(" ", "")}'");

            return
            $"        void I{actionMapName}Actions.On{action.name.Replace(" ", "")}(InputAction.CallbackContext context)\n" +
            $"        {{\n" +
            $"            if (!context.performed)\n" +
            $"                return;\n" +
            $"            On{action.name.Replace(" ", "")}?.Invoke(context.ReadValue<{inputType}>());\n" +
            $"        }}\n";
        }

        private string GetActionReturnType(InputAction action)
        {
            foreach (var binding in action.bindings)
            {
                if (binding.isComposite)
                {
                    return binding.path switch
                    {
                        "Dpad" => "Vector2",
                        "2DVector" => "Vector2",
                        "3DVector" => "Vector3",
                        _ => null
                    };
                }
            }
            if (action.controls.Count == 0)
            {
                Debug.Log($"{action.name} has no controls");
                return null;
            }
            InputControl control = action.controls[0];

            if (control is Vector2Control)
                return "Vector2";
            if (control is Vector3Control)
                return "Vector3";
            if (control is AxisControl)
                return "float";
            if (control is ButtonControl)
                return "bool";
            if (control is KeyControl)
                return "bool";
            if (control is QuaternionControl)
                return "Quaternion";
            if (control is IntegerControl)
                return "int";
            if (control is StickControl)
                return "Vector2";
            if (control is DpadControl)
                return "Vector2";
            if (control is TouchControl)
                return "Vector2";

            return null; // All controls should be covered, should never return null
        }
    }
}
