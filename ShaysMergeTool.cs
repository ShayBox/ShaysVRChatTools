#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRLabs.AV3Manager;
using static VRLabs.AV3Manager.AV3ManagerFunctions;

namespace ShayBox {

	[ExecuteInEditMode]
	public class ShaysMergeTool : MonoBehaviour {
		public AnimatorController baseController;
		public AnimatorController additiveController;
		public AnimatorController gestureController;
		public AnimatorController actionController;
		public AnimatorController fxController;
		public AnimatorController[] mainControllers;
		public AnimatorController[] controllersToAdd;
	}

	[CustomEditor(inspectedType: typeof(ShaysMergeTool))]
	public class ShaysMergeToolEditor : Editor {
		public override void OnInspectorGUI() {
			ShaysMergeTool data = target as ShaysMergeTool;

			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 115;
			data.baseController = AnimationControllerField(label: "Base", obj: data.baseController);
			data.additiveController = AnimationControllerField(label: "Additive", obj: data.additiveController);
			data.gestureController = AnimationControllerField(label: "Gesture", obj: data.gestureController);
			data.actionController = AnimationControllerField(label: "Action", obj: data.actionController);
			data.fxController = AnimationControllerField(label: "FX", obj: data.fxController);
			EditorGUIUtility.labelWidth = labelWidth;
			if (GUILayout.Button(new GUIContent("Merge", "Merges the above animation controllers with the above avatar descriptors respective base layers."))) {
				VRCAvatarDescriptor descriptor = data.GetComponent<VRCAvatarDescriptor>();
				MergeToLayer(descriptor, data.baseController, playable: PlayableLayer.Base);
				MergeToLayer(descriptor, data.additiveController, playable: PlayableLayer.Additive);
				MergeToLayer(descriptor, data.gestureController, playable: PlayableLayer.Gesture);
				MergeToLayer(descriptor, data.actionController, playable: PlayableLayer.Action);
				MergeToLayer(descriptor, data.fxController, playable: PlayableLayer.FX);
			}
		}

		public AnimatorController AnimationControllerField(string label, Object obj) {
			return (AnimatorController)EditorGUILayout.ObjectField(label, obj, typeof(AnimatorController), allowSceneObjects: true);
		}

		public void ShowAnimControllersField(string propertyPath) {
			SerializedObject serializedObject = new SerializedObject(obj: target);
			SerializedProperty property = serializedObject.FindProperty(propertyPath);
			serializedObject.Update();
			EditorGUILayout.PropertyField(property, includeChildren: true);
			serializedObject.ApplyModifiedProperties();
		}

		public void MergeToLayer(VRCAvatarDescriptor descriptor, AnimatorController controllerToAdd, PlayableLayer playable) {
			AV3ManagerFunctions.MergeToLayer(descriptor, controllerToAdd, playable, GetDirectory(controllerToAdd), overwrite: false);
		}

		public string GetDirectory(AnimatorController controllerToAdd) {
			return $"Assets/VRLabs/GeneratedAssets/Animators/(Merged) {controllerToAdd.name} + ";
		}

	}
}
#endif