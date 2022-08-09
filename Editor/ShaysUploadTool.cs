using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRC.SDKBase.Editor;

namespace ShayBox {

	public class ShaysUploadTool : EditorWindow {

#if UNITY_STANDALONE_WIN
		public static readonly string PLATFORM = "PC";
		public static readonly string SUFFIX = " (PC)";
#elif UNITY_ANDROID
		public static readonly string PLATFORM = "Quest";
		public static readonly string SUFFIX = " (Quest)";
#endif

		[MenuItem(itemName: "Window/Shays Upload Tool")]
		public static void ShowWindow() => GetWindow<ShaysUploadTool>(title: "Shays Upload Tool");

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

			// Pre-conditions
			if (sceneAvatarObjects.Length < 1) {
				GUILayout.Label(text: "No Avatars Found");
				return;
			}

			foreach (GameObject obj in sceneAvatarObjects) {
				if (GUILayout.Button(text: "Build & Upload " + obj.name)) {
					isWaitingForPlaymode = true;
					uploadObject = obj;
					VRC_SdkBuilder.RunExportAndUploadAvatarBlueprint(obj);
				}
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