#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRLabs.AV3Manager;
using static VRLabs.AV3Manager.AV3ManagerFunctions;

namespace ShayBox {

	[ExecuteInEditMode]
	public class ShaysMassMergeTool : MonoBehaviour {
		public AnimatorController baseController;
		public AnimatorController additiveController;
		public AnimatorController gestureController;
		public AnimatorController actionController;
		public AnimatorController fxController;
		public AnimatorController[] mainControllers;
		public AnimatorController[] controllersToAdd;
	}

	[CustomEditor(inspectedType: typeof(ShaysMassMergeTool))]
	public class ShaysMassMergeToolEditor : Editor {
		public override void OnInspectorGUI() {
			ShaysMassMergeTool data = target as ShaysMassMergeTool;

			ShowAnimControllersField(propertyPath: "mainControllers");
			ShowAnimControllersField(propertyPath: "controllersToAdd");
			if (GUILayout.Button(new GUIContent("Mass Merge", "Merges the two above animation controller lists."))) {
				foreach (AnimatorController mainController in data.mainControllers) {
					foreach (AnimatorController controllerToMerge in data.controllersToAdd) {
						AnimatorCloner.MergeControllers(mainController, controllerToMerge, saveToNew: true);
					}
				}
			}
		}

		public void ShowAnimControllersField(string propertyPath) {
			SerializedObject serializedObject = new SerializedObject(obj: target);
			SerializedProperty property = serializedObject.FindProperty(propertyPath);
			serializedObject.Update();
			EditorGUILayout.PropertyField(property, includeChildren: true);
			serializedObject.ApplyModifiedProperties();
		}

		public string GetDirectory(AnimatorController controllerToAdd) {
			return $"Assets/ShaysVRChatTools/GeneratedAssets/(Merged) {controllerToAdd.name} + ";
		}

	}
}
#endif