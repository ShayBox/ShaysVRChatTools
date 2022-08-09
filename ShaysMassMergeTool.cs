#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static VRLabs.AV3Manager.AV3ManagerFunctions;

namespace ShayBox {

	[ExecuteInEditMode]
	public class ShaysMassMergeTool : MonoBehaviour {
		public AnimatorController[] layers;
	}

	[CustomEditor(inspectedType: typeof(ShaysMassMergeTool))]
	public class ShaysMassMergeToolEditor : Editor {
		public override void OnInspectorGUI() {
			ShaysMassMergeTool data = target as ShaysMassMergeTool;

			SerializedObject serializedObject = new SerializedObject(obj: target);
			SerializedProperty property = serializedObject.FindProperty(propertyPath: "layers");
			serializedObject.Update();
			EditorGUILayout.PropertyField(property, includeChildren: true);
			serializedObject.ApplyModifiedProperties();

			if (GUILayout.Button(text: "Merge & Override")) {
				VRCAvatarDescriptor descriptor = data.GetComponent<VRCAvatarDescriptor>();
				foreach (AnimatorController controllerToAdd in data.layers) {
					Merge(descriptor, controllerToAdd, playable: PlayableLayer.Base);
					Merge(descriptor, controllerToAdd, playable: PlayableLayer.Additive);
					Merge(descriptor, controllerToAdd, playable: PlayableLayer.Gesture);
					Merge(descriptor, controllerToAdd, playable: PlayableLayer.Action);
				}
			}
		}

		public void Merge(VRCAvatarDescriptor descriptor, AnimatorController controllerToAdd, PlayableLayer playable) {
			if (controllerToAdd != null) {
				MergeToLayer(descriptor, controllerToAdd, playable,
					directory: GetDirectory(controllerToAdd),
					overwrite: true,
					change: false
				);
			}
		}

		public string GetDirectory(AnimatorController controllerToAdd) {
			return $"Assets/ShaysTools/GeneratedAssets/{controllerToAdd.name} + ";
		}

	}
}
#endif