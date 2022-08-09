using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRC.SDKBase.Editor;

public class ShaysUploadTool : EditorWindow {
	public bool isWaitingForPlaymode = false;
	public GameObject uploadObject = null;

	public void OnEnable() => EditorApplication.playModeStateChanged += PlayModeStateChanged;
	public void OnDisable() => EditorApplication.playModeStateChanged -= PlayModeStateChanged;

	public void PlayModeStateChanged(PlayModeStateChange state) {
		if (state is PlayModeStateChange.EnteredEditMode)
			isWaitingForPlaymode = false;
	}

	[MenuItem(itemName: "Window/Shays Upload Tool")]
	public static void ShowWindow() => GetWindow<ShaysUploadTool>(title: "Shays Upload Tool");

	// GUI Render Loop
	public void OnGUI() {
		Scene scene = SceneManager.GetActiveScene();
		GameObject[] allObjs = scene.GetRootGameObjects();
		GameObject[] aviObjs = allObjs.Where(predicate: o => o.activeInHierarchy && o.GetComponent(type: "VRCAvatarDescriptor") != null).ToArray();
		if (aviObjs.Length < 1) {
			GUILayout.Label(text: "No Avatars Found!");
			return;
		}

		if (GUILayout.Button(text: "Build & Test Avatar(s)"))
			foreach (GameObject obj in aviObjs)
				VRC_SdkBuilder.RunExportAndTestAvatarBlueprint(obj);

		foreach (GameObject obj in aviObjs)
			if (GUILayout.Button(text: "Build & Upload " + obj.name)) {
				isWaitingForPlaymode = true;
				uploadObject = obj;
				VRC_SdkBuilder.RunExportAndUploadAvatarBlueprint(obj);
			}
	}

	// Game Update Loop
	public void Update() {
		if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
			// Play Mode
			// Delay upload and prevent uploading more than once
			if (Time.frameCount > 150 && isWaitingForPlaymode)
				isWaitingForPlaymode = false;
			else
				return;

			Scene scene = SceneManager.GetActiveScene();
			GameObject[] allObjs = scene.GetRootGameObjects();
			GameObject VRCSDK = allObjs.Where(predicate: o => o.name.Equals(value: "VRCSDK")).First();
			if (VRCSDK == null) {
				GUILayout.Label(text: "No VRCSDK Object Found!");
				return;
			}

			// Unnecessary but why not
			Toggle[] toggles = VRCSDK.GetComponentsInChildren<Toggle>();
			Toggle warrantToggle = toggles.First(predicate: t => t.name.Equals(value: "ToggleWarrant"));
			warrantToggle.isOn = true;

			Button[] buttons = VRCSDK.GetComponentsInChildren<Button>(includeInactive: true);
			Button uploadButton = buttons.First(predicate: b => b.name.Equals(value: "UploadButton"));
			if (uploadButton != null)
				uploadButton.onClick.Invoke();
		} else if (uploadObject != null) {
			// Edit Mode
			uploadObject.SetActive(value: false);
			uploadObject = null;
		}
	}
}