using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;

namespace ShayBox {

	public class AvatarScaleDictionary : Dictionary<string, double> {
		public new void Add(string id, double scale) => Add(key: id, value: scale);
		public new double this[string id] {
			get { return base[key: id]; }
			set { base[key: id] = value; }
		}
	}

	public class Avatar {
		public string name;
		public AvatarScaleDictionary scaleDict;
		public Avatar(string name, AvatarScaleDictionary scaleDict) {
			this.name = name + ShaysAvatarTool.SUFFIX;
			this.scaleDict = scaleDict;
		}
	}

	public class ShaysAvatarTool : EditorWindow {

#if UNITY_STANDALONE_WIN
		public static readonly string PLATFORM = "PC";
		public static readonly string SUFFIX = " (PC)";
#elif UNITY_ANDROID
		public static readonly string PLATFORM = "Quest";
		public static readonly string SUFFIX = " (Quest)";
#endif

		public static readonly Avatar[] AVATARS = Avatars.EXTERNAL_AVATARS;
		public static readonly string[] AVATAR_NAMES = AVATARS.Select(selector: avi => avi.name).ToArray();

		[MenuItem(itemName: "Window/Shays VRC Avatar Tool")]
		public static void ShowWindow() => GetWindow<ShaysAvatarTool>(title: "Shays Avatar Tool");

		public void OnEnable() => EditorApplication.playModeStateChanged += PlayModeStateChanged;
		public void OnDisable() => EditorApplication.playModeStateChanged -= PlayModeStateChanged;

		public bool isWaitingForPlaymode = false;
		public void PlayModeStateChanged(PlayModeStateChange state) {
			if (state is PlayModeStateChange.EnteredEditMode) {
				isWaitingForPlaymode = false;
			}
		}

		public GameObject uploadObject = null;

		// Window Render Loop
		public void OnGUI() {
			// Variables
			Scene scene = SceneManager.GetActiveScene();
			GameObject[] sceneObjects = scene.GetRootGameObjects();
			GameObject[] sceneAvatarObjects = sceneObjects.Where(predicate: obj => obj.activeInHierarchy && obj.GetComponent(type: "VRCAvatarDescriptor") != null).ToArray();
			GameObject[] validAvatarObjects = sceneAvatarObjects.Where(obj => AVATAR_NAMES.Contains(value: obj.name)).ToArray();
			GameObject[] cloneAvatarObjects = sceneAvatarObjects.Where(obj => !AVATAR_NAMES.Contains(value: obj.name)).ToArray();
			GameObject shaysAvatarTool = sceneObjects.First(predicate: obj => obj.name.Equals(value: "Shays Avatar Tool"));
			VRCAvatarDescriptor shaysAvatarDescriptor = (VRCAvatarDescriptor)shaysAvatarTool.GetComponent(type: "VRCAvatarDescriptor");

			// Pre-conditions
			if (sceneAvatarObjects.Length < 1 || AVATARS.Count() < 1) {
				GUILayout.Label(text: "No Avatars Found");
				return;
			}

			if (shaysAvatarTool == null || shaysAvatarDescriptor == null) {
				GUILayout.Label(text: "No Tool Object or Descriptor Found");
				return;
			}

			if (validAvatarObjects.Length > 0 && GUILayout.Button(text: "Scale Active Avatars")) {
				foreach (GameObject original in validAvatarObjects) {
					Avatar avatar = AVATARS.First(predicate: avi => avi.name.Equals(value: original.name));
					foreach (KeyValuePair<string, double> scale in avatar.scaleDict) {
						GameObject obj = Instantiate(original);
						obj.name = obj.name
							.Replace(oldValue: "(Clone)", newValue: scale.Value.ToString(format: "0.0"))
							.Replace(oldValue: "0.5", newValue: " (Small)")
							.Replace(oldValue: "1.0", newValue: " (Normal)")
							.Replace(oldValue: "1.5", newValue: " (Tall)")
							.Replace(oldValue: "2.0", newValue: " (Large)");
						obj.transform.localScale = new Vector3(
							x: obj.transform.localScale.x * (float)scale.Value,
							y: obj.transform.localScale.y * (float)scale.Value,
							z: obj.transform.localScale.z * (float)scale.Value
						);

						VRCAvatarDescriptor desc = (VRCAvatarDescriptor)obj.GetComponent(type: "VRCAvatarDescriptor");
						desc.ViewPosition = new Vector3(
							x: desc.ViewPosition.x * (float)scale.Value,
							y: desc.ViewPosition.y * (float)scale.Value,
							z: desc.ViewPosition.z * (float)scale.Value
						);

						switch (scale.Value) {
							case 0.5:
								desc.baseAnimationLayers[0].animatorController = shaysAvatarDescriptor.baseAnimationLayers[4].animatorController; // Base
								desc.baseAnimationLayers[4].animatorController = shaysAvatarDescriptor.baseAnimationLayers[0].animatorController; // FX
								break;
							case 1.0:
								desc.baseAnimationLayers[0].animatorController = shaysAvatarDescriptor.specialAnimationLayers[5].animatorController; // Base
								desc.baseAnimationLayers[4].animatorController = shaysAvatarDescriptor.baseAnimationLayers[1].animatorController; // FX
								break;
							case 1.5:
								desc.baseAnimationLayers[0].animatorController = shaysAvatarDescriptor.specialAnimationLayers[6].animatorController; // Base
								desc.baseAnimationLayers[4].animatorController = shaysAvatarDescriptor.baseAnimationLayers[2].animatorController; // FX
								break;
							case 2.0:
								desc.baseAnimationLayers[0].animatorController = shaysAvatarDescriptor.specialAnimationLayers[7].animatorController; // Base
								desc.baseAnimationLayers[4].animatorController = shaysAvatarDescriptor.baseAnimationLayers[3].animatorController; // FX
								break;
							default:
								Debug.Log(message: "Unknown avatar scale value: " + scale.Value.ToString());
								goto case 1.0;
						}

						PipelineManager pipelineManager = (PipelineManager)obj.GetComponent(type: "PipelineManager");
						pipelineManager.blueprintId = scale.Key;
					}
					original.SetActive(value: false);
				}
			}

			foreach (GameObject obj in cloneAvatarObjects) {
				if (GUILayout.Button(text: "Build & Upload " + obj.name)) {
					isWaitingForPlaymode = true;
					uploadObject = obj;
					VRC_SdkBuilder.RunExportAndUploadAvatarBlueprint(obj);
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.Label(text: "Avatar Scale Layers Preview");
			foreach (CustomAnimLayer layer in shaysAvatarDescriptor.baseAnimationLayers) {
				EditorGUILayout.ObjectField(obj: layer.animatorController, objType: typeof(RuntimeAnimatorController), allowSceneObjects: true);
			}
			foreach (CustomAnimLayer layer in shaysAvatarDescriptor.specialAnimationLayers) {
				EditorGUILayout.ObjectField(obj: layer.animatorController, objType: typeof(RuntimeAnimatorController), allowSceneObjects: true);
			}
		}

		// Game Update Loop
		public void Update() {
			if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
				// Play Mode
				// Wait for VRCSDK to load and prevent double-uploading
				if (Time.frameCount > 500 && isWaitingForPlaymode) {
					isWaitingForPlaymode = false;
				} else {
					return;
				}

				// Variables
				Scene scene = SceneManager.GetActiveScene();
				GameObject[] sceneObjects = scene.GetRootGameObjects();
				GameObject VRCSDK = sceneObjects.Where(predicate: obj => obj.name.Equals(value: "VRCSDK")).First();

				// Pre-conditions
				if (VRCSDK == null) {
					GUILayout.Label(text: "No VRCSDK Found");
					return;
				}

				InputField[] inputFields = VRCSDK.GetComponentsInChildren<InputField>();
				string input = uploadObject.name.Replace(oldValue: SUFFIX, newValue: "");
				inputFields.First(predicate: f => f.name.Equals(value: "Name Input Field")).SetTextWithoutNotify(input);
				inputFields.First(predicate: f => f.name.Equals(value: "Description Input Field")).SetTextWithoutNotify(input);

				Toggle[] toggles = VRCSDK.GetComponentsInChildren<Toggle>();
				toggles.First(predicate: t => t.name.Equals(value: "ToggleWarrant")).isOn = true;
				if (PLATFORM == "PC") {
					toggles.First(predicate: t => t.name.Equals(value: "ImageUploadToggle")).isOn = true;

					GameObject gestureManager = sceneObjects.First(predicate: obj => obj.name.Equals(value: "GestureManager"));
					if (gestureManager != null) {
						Selection.SetActiveObjectWithContext(obj: gestureManager, context: null);
					}

					GameObject VRCCam = sceneObjects.First(predicate: obj => obj.name.Equals(value: "VRCCam"));
					if (VRCCam == null) {
						GUILayout.Label(text: "No VRCCam Found");
						return;
					}

					Animator animator = uploadObject.GetComponent<Animator>();
					Transform headTransform = animator.GetBoneTransform(humanBoneId: HumanBodyBones.Head);
					VRCCam.transform.position = new Vector3(x: 0.0f, y: headTransform.position.y, z: headTransform.localScale.z);

					GameObject shaysAvatarTool = sceneObjects.First(predicate: obj => obj.name.Equals(value: "Shays Avatar Tool"));
					shaysAvatarTool.SetActive(value: true);
					shaysAvatarTool.transform.position = new Vector3(x: 0.0f, y: headTransform.position.y, z: VRCCam.transform.position.z - 0.25f);

					TextMeshPro sizeText = shaysAvatarTool.GetComponent(type: "TextMeshPro") as TextMeshPro;
					sizeText.SetText(sourceText: uploadObject.transform.localScale.x.ToString("0.0")
						.Replace(oldValue: "0.5", newValue: "Small")
						.Replace(oldValue: "1.0", newValue: "Normal")
						.Replace(oldValue: "1.5", newValue: "Tall")
						.Replace(oldValue: "2.0", newValue: "Large")
					);
				}

				Button[] buttons = VRCSDK.GetComponentsInChildren<Button>(includeInactive: true);
				buttons.First(predicate: b => b.name.Equals(value: "UploadButton")).onClick.Invoke();
			} else if (uploadObject != null) {
				// Edit Mode
				uploadObject.SetActive(value: false);
				uploadObject = null;
			}
		}

	}

}