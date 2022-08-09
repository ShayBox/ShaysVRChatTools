#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static VRLabs.AV3Manager.AV3ManagerFunctions;

namespace ShayBox {

	[ExecuteInEditMode]
	public class ShaysMergeTool : MonoBehaviour {
		public AnimatorController baseLayer;
		public AnimatorController additiveLayer;
		public AnimatorController gestureLayer;
		public AnimatorController actionLayer;
		public AnimatorController fxLayer;
	}

	[CustomEditor(inspectedType: typeof(ShaysMergeTool))]
	public class ShaysMergeToolEditor : Editor {
		public override void OnInspectorGUI() {
			ShaysMergeTool data = target as ShaysMergeTool;

			data.baseLayer = Show(text: "Base Layer", obj: data.baseLayer);
			data.additiveLayer = Show(text: "Additive Layer", obj: data.additiveLayer);
			data.gestureLayer = Show(text: "Gesture Layer", obj: data.gestureLayer);
			data.actionLayer = Show(text: "Action Layer", obj: data.actionLayer);
			data.fxLayer = Show(text: "FX Layer", obj: data.fxLayer);

			if (GUILayout.Button(text: "Merge & Override")) {
				VRCAvatarDescriptor descriptor = data.GetComponent<VRCAvatarDescriptor>();
				Merge(descriptor, controllerToAdd: data.baseLayer, playable: PlayableLayer.Base);
				Merge(descriptor, controllerToAdd: data.additiveLayer, playable: PlayableLayer.Additive);
				Merge(descriptor, controllerToAdd: data.gestureLayer, playable: PlayableLayer.Gesture);
				Merge(descriptor, controllerToAdd: data.actionLayer, playable: PlayableLayer.Action);
				Merge(descriptor, controllerToAdd: data.fxLayer, playable: PlayableLayer.FX);
			}
		}

		public AnimatorController Show(string text, AnimatorController obj) {
			GUILayout.Label(text);
			return (AnimatorController)EditorGUILayout.ObjectField(obj, objType: typeof(AnimatorController), allowSceneObjects: true);
		}

		public void Merge(VRCAvatarDescriptor descriptor, AnimatorController controllerToAdd, PlayableLayer playable) {
			if (controllerToAdd != null) {
				MergeToLayer(descriptor, controllerToAdd, playable,
					directory: GetDirectory(controllerToAdd),
					overwrite: true
				);
			}
		}

		public string GetDirectory(AnimatorController controllerToAdd) {
			return $"Assets/VRLabs/GeneratedAssets/(Merged) {controllerToAdd.name} + ";
		}

	}
}
#endif