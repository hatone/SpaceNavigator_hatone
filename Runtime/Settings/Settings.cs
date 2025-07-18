using System;
using System.Collections.Generic;
using UnityEngine;
using Assembly = System.Reflection.Assembly;
#if UNITY_EDITOR
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif

namespace SpaceNavigatorDriver {

	public enum OperationMode { Fly, Orbit, Telekinesis, GrabMove }
	public enum CoordinateSystem { Camera, World, Parent, Local }
	public enum Axis { X, Y, Z }
	public enum DoF { Translation, Rotation }

	[Serializable]
	public static class Settings {
		public static OperationMode Mode;
		public static CoordinateSystem CoordSys;
		public static bool PresentationMode;
		public static float PresentationDamping = 0.015f;

		// Snapping
		public static bool SnapRotation;
		public static int SnapAngle = 45;
		public static bool SnapTranslation;
		public static float SnapDistance = 0.1f;

		// Locking
		public static bool HorizonLock = true;
		public static bool MacOSCursorLock = true;
		public static Locks NavTranslationLock;
		public static Locks NavRotationLock;
		public static Locks ManipulateTranslationLock;
		public static Locks ManipulateRotationLock;

		// Sensitivity
		private static int Gears = 3;
		public static int CurrentGear = 1;
		public static bool ShowSpeedGearsAsRadioButtons = false;

		public static List<float> TransSensDefault = new List<float> { 50, 1, 0.05f };
		public static List<float> TransSensMinDefault = new List<float>() { 1, 0.1f, 0.01f };
		public static List<float> TransSensMaxDefault = new List<float>() { 100, 10, 1 };
		public static List<float> TransSens = new List<float>(TransSensDefault);
		public static List<float> TransSensMin = new List<float>(TransSensMinDefault);
		public static List<float> TransSensMax = new List<float>(TransSensMaxDefault);

		public const float RotSensDefault = 1, RotSensMinDefault = 0, RotSensMaxDefault = 5f;
		public static float RotSens = RotSensDefault;
		public static float RotSensMin = RotSensMinDefault;
		public static float RotSensMax = RotSensMaxDefault;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	public const float RotDeadDefault = 30, RotDeadMinDefault = 0, RotDeadMaxDefault = 100f;
	public static float RotDead = RotDeadDefault;
	public static float RotDeadMin = RotDeadMinDefault;
	public static float RotDeadMax = RotDeadMaxDefault;

	public const float TransDeadDefault = 30, TransDeadMinDefault = 0, TransDeadMaxDefault = 100f;
	public static float TransDead = TransDeadDefault;
	public static float TransDeadMin = TransDeadMinDefault;
	public static float TransDeadMax = TransDeadMaxDefault;
#endif

		// Focus
		public static bool OnlyNavWhenUnityHasFocus = true;
		public static bool ToggleLedWhenFocusChanged;

		// Runtime editor navigation
		public static bool RuntimeEditorNav = true;

		// Inversion
		public static Vector3 FlyInvertTranslation, FlyInvertRotation;
		public static Vector3 OrbitInvertTranslation, OrbitInvertRotation;
		public static Vector3 TelekinesisInvertTranslation, TelekinesisInvertRotation;
		public static Vector3 GrabMoveInvertTranslation, GrabMoveInvertRotation;

		// Calibration
		public const float TransSensEpsilonDefault = 5f;
		public static float TransSensEpsilon = TransSensEpsilonDefault;
		public const float RotSensEpsilonDefault = 5f;
		public static float RotSensEpsilon = RotSensEpsilonDefault;
		public static Vector3? TranslationDrift;
		public static Vector3? RotationDrift;

		public static bool OnGUI()
		{
			bool triggerToolbarRefresh = false;
#if UNITY_EDITOR
			EditorGUI.BeginChangeCheck();

			// Quick help section
			DrawQuickHelp();

			_scrollPos = GUILayout.BeginScrollView(_scrollPos);
			GUILayout.BeginVertical();

			#region - Sensitivity + gearbox -
			GUILayout.BeginHorizontal();

			#region - Sensitivity -
			GUILayout.BeginVertical();
			GUILayout.Label("Sensitivity");

			GUILayout.BeginVertical();
			
			#region - Translation -
			GUILayout.BeginHorizontal();
			GUILayout.Label("Translation", GUILayout.Width(67));
			TransSens[CurrentGear] = EditorGUILayout.FloatField(TransSens[CurrentGear], GUILayout.Width(30));
			TransSensMin[CurrentGear] = EditorGUILayout.FloatField(TransSensMin[CurrentGear], GUILayout.Width(30));
			TransSens[CurrentGear] = GUILayout.HorizontalSlider(TransSens[CurrentGear], TransSensMin[CurrentGear], TransSensMax[CurrentGear]);
			TransSensMax[CurrentGear] = EditorGUILayout.FloatField(TransSensMax[CurrentGear], GUILayout.Width(30));
			GUILayout.EndHorizontal();
			#endregion - Translation -

			#region - Rotation -
			GUILayout.BeginHorizontal();
			GUILayout.Label("Rotation", GUILayout.Width(67));
			RotSens = EditorGUILayout.FloatField(RotSens, GUILayout.Width(30));
			RotSensMin = EditorGUILayout.FloatField(RotSensMin, GUILayout.Width(30));
			RotSens = GUILayout.HorizontalSlider(RotSens, RotSensMin, RotSensMax);
			RotSensMax = EditorGUILayout.FloatField(RotSensMax, GUILayout.Width(30));
			GUILayout.EndHorizontal();
			#endregion - Rotation -

			#region - Radio buttons -
			GUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			ShowSpeedGearsAsRadioButtons = GUILayout.Toggle(ShowSpeedGearsAsRadioButtons, "Show as radio buttons");
			triggerToolbarRefresh = EditorGUI.EndChangeCheck();
			GUILayout.EndHorizontal();
			#endregion - Radio buttons -
			
			GUILayout.EndVertical();

			GUILayout.EndVertical();
			#endregion - Sensitivity -

			#region - Gearbox -
			GUILayout.BeginVertical();
			GUILayout.Label("Scale", GUILayout.Width(65));
			GUIContent[] modes = new GUIContent[] {
				new GUIContent("Huge", "Galactic scale"),
				new GUIContent("Human", "What people consider 'normal'"),
				new GUIContent("Minuscule", "Itsy-bitsy-scale")
			};
			CurrentGear = GUILayout.SelectionGrid(CurrentGear, modes, 1, GUILayout.Width(67));
			GUILayout.EndVertical();
			#endregion - Gearbox -

			GUILayout.EndHorizontal();
			#endregion - Sensitivity + gearbox -

			#region - Presentation Mode -

			GUILayout.Space(10);
			GUILayout.Label("Presentation Mode");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Damping", GUILayout.Width(120));
			PresentationDamping = GUILayout.HorizontalSlider(PresentationDamping, 0, 0.1f);
			GUILayout.EndHorizontal();

			#endregion

			#region - Deadzone -
			GUILayout.BeginVertical();
			GUILayout.Space(10);
			GUILayout.Label("Deadzone");

			#region - Translation Epsilon -
			GUILayout.BeginHorizontal();
			GUILayout.Label("Translation", GUILayout.Width(120));
			int epsilonMax = 12;
			int deadzone = epsilonMax - Mathf.RoundToInt(TransSensEpsilon);
			TransSensEpsilon = epsilonMax - EditorGUILayout.IntSlider(deadzone, 0, epsilonMax);
			GUILayout.EndHorizontal();			
			#endregion - Translation Epsilon -
			
			#region - Rotation Epsilon -
			GUILayout.BeginHorizontal();
			GUILayout.Label("Rotation", GUILayout.Width(120));
			deadzone = epsilonMax - Mathf.RoundToInt(RotSensEpsilon);
			RotSensEpsilon = epsilonMax - EditorGUILayout.IntSlider(deadzone, 0, epsilonMax); 
			GUILayout.EndHorizontal();
			#endregion - Rotation Epsilon -

			if (GUILayout.Button("Recalibrate Drift"))
			{
				TranslationDrift = SpaceNavigatorHID.current.Translation.ReadValue();
				RotationDrift = SpaceNavigatorHID.current.Rotation.ReadValue();
			}
			
			GUILayout.EndVertical();			
			#endregion - Deadzone -

			#region - Dead Zone -
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			GUILayout.BeginVertical();
			GUILayout.Label("Dead Zone");

			#region - Translation + rotation -
			GUILayout.BeginVertical();
			#region - Translation -
			GUILayout.BeginHorizontal();
			GUILayout.Label("Translation", GUILayout.Width(67));
			TransDead = EditorGUILayout.FloatField(TransDead, GUILayout.Width(30));
			TransDeadMin = EditorGUILayout.FloatField(TransDeadMin, GUILayout.Width(30));
			TransDead = GUILayout.HorizontalSlider(TransDead, TransDeadMin, TransDeadMax);
			TransDeadMax = EditorGUILayout.FloatField(TransDeadMax, GUILayout.Width(30));
			GUILayout.EndHorizontal();
			#endregion - Translation -

			#region - Rotation -
			GUILayout.BeginHorizontal();
			GUILayout.Label("Rotation", GUILayout.Width(67));
			RotDead = EditorGUILayout.FloatField(RotDead, GUILayout.Width(30));
			RotDeadMin = EditorGUILayout.FloatField(RotDeadMin, GUILayout.Width(30));
			RotDead = GUILayout.HorizontalSlider(RotDead, RotDeadMin, RotDeadMax);
			RotDeadMax = EditorGUILayout.FloatField(RotDeadMax, GUILayout.Width(30));
			GUILayout.EndHorizontal();
			#endregion - Rotation -

			if (GUILayout.Button("Recalibrate Drift"))
			{
				TranslationDrift = SpaceNavigatorHID.current.Translation.ReadValue();
				RotationDrift = SpaceNavigatorHID.current.Rotation.ReadValue();
			}

			GUILayout.EndVertical();
			#endregion - Translation + rotation -
			GUILayout.EndVertical();
#endif
			#endregion - Deadzone -

			#region - Snapping -

			GUILayout.Space(10);
			GUILayout.Label("Snap");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Grid Snap size", GUILayout.Width(120));
			SnapDistance = EditorGUILayout.FloatField(SnapDistance);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Angle Snap angle", GUILayout.Width(120));
			SnapAngle = EditorGUILayout.IntField(SnapAngle);
			GUILayout.EndHorizontal();

			#endregion- Snapping -

			#region - Locking -

			GUILayout.Space(10);
			GUILayout.Label("Lock");

			#region - Translation -
			GUILayout.BeginHorizontal();
			if (Mode == OperationMode.Fly || Mode == OperationMode.Orbit) {
				NavTranslationLock.All = GUILayout.Toggle(NavTranslationLock.All, "Translation", GUILayout.Width(120));
				GUI.enabled = !NavTranslationLock.All;
				NavTranslationLock.X = GUILayout.Toggle(NavTranslationLock.X, "X", GUILayout.Width(60));
				NavTranslationLock.Y = GUILayout.Toggle(NavTranslationLock.Y, "Y", GUILayout.Width(60));
				NavTranslationLock.Z = GUILayout.Toggle(NavTranslationLock.Z, "Z", GUILayout.Width(60));
				GUI.enabled = true;
			} else {
				ManipulateTranslationLock.All = GUILayout.Toggle(ManipulateTranslationLock.All, "Translation", GUILayout.Width(120));
				GUI.enabled = !ManipulateTranslationLock.All;
				ManipulateTranslationLock.X = GUILayout.Toggle(ManipulateTranslationLock.X, "X", GUILayout.Width(60));
				ManipulateTranslationLock.Y = GUILayout.Toggle(ManipulateTranslationLock.Y, "Y", GUILayout.Width(60));
				ManipulateTranslationLock.Z = GUILayout.Toggle(ManipulateTranslationLock.Z, "Z", GUILayout.Width(60));
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();
			#endregion - Translation -

			#region - Rotation -
			GUILayout.BeginHorizontal();
			if (Mode == OperationMode.Fly || Mode == OperationMode.Orbit) {
				NavRotationLock.All = GUILayout.Toggle(NavRotationLock.All, "Rotation", GUILayout.Width(120));
				GUI.enabled = !NavRotationLock.All;
				NavRotationLock.X = GUILayout.Toggle(NavRotationLock.X, "X", GUILayout.Width(60));
				NavRotationLock.Y = GUILayout.Toggle(NavRotationLock.Y, "Y", GUILayout.Width(60));
				NavRotationLock.Z = GUILayout.Toggle(NavRotationLock.Z, "Z", GUILayout.Width(60));
				GUI.enabled = true;
			} else {
				ManipulateRotationLock.All = GUILayout.Toggle(ManipulateRotationLock.All, "Rotation", GUILayout.Width(120));
				GUI.enabled = !ManipulateRotationLock.All;
				ManipulateRotationLock.X = GUILayout.Toggle(ManipulateRotationLock.X, "X", GUILayout.Width(60));
				ManipulateRotationLock.Y = GUILayout.Toggle(ManipulateRotationLock.Y, "Y", GUILayout.Width(60));
				ManipulateRotationLock.Z = GUILayout.Toggle(ManipulateRotationLock.Z, "Z", GUILayout.Width(60));
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();
			if (Application.platform == RuntimePlatform.OSXEditor)
				MacOSCursorLock = GUILayout.Toggle(MacOSCursorLock, "Lock mouse pointer while navigating", GUILayout.Width(300));
			
			#endregion - Rotation -

			#endregion - Locking -

			#region - Axes inversion per mode -

			GUILayout.Space(10);
			GUILayout.Label("Invert axes in " + Settings.Mode.ToString() + " mode");
			bool tx, ty, tz, rx, ry, rz;
			switch (Settings.Mode) {
				case OperationMode.Fly:
					tx = FlyInvertTranslation.x < 0; ty = FlyInvertTranslation.y < 0; tz = FlyInvertTranslation.z < 0;
					rx = FlyInvertRotation.x < 0; ry = FlyInvertRotation.y < 0; rz = FlyInvertRotation.z < 0;
					break;
				case OperationMode.Orbit:
					tx = OrbitInvertTranslation.x < 0; ty = OrbitInvertTranslation.y < 0; tz = OrbitInvertTranslation.z < 0;
					rx = OrbitInvertRotation.x < 0; ry = OrbitInvertRotation.y < 0; rz = OrbitInvertRotation.z < 0;
					break;
				case OperationMode.Telekinesis:
					tx = TelekinesisInvertTranslation.x < 0; ty = TelekinesisInvertTranslation.y < 0; tz = TelekinesisInvertTranslation.z < 0;
					rx = TelekinesisInvertRotation.x < 0; ry = TelekinesisInvertRotation.y < 0; rz = TelekinesisInvertRotation.z < 0;
					break;
				case OperationMode.GrabMove:
					tx = GrabMoveInvertTranslation.x < 0; ty = GrabMoveInvertTranslation.y < 0; tz = GrabMoveInvertTranslation.z < 0;
					rx = GrabMoveInvertRotation.x < 0; ry = GrabMoveInvertRotation.y < 0; rz = GrabMoveInvertRotation.z < 0;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Translation", GUILayout.Width(120));
			EditorGUI.BeginChangeCheck();
			tx = GUILayout.Toggle(tx, "X", GUILayout.Width(60));
			ty = GUILayout.Toggle(ty, "Y", GUILayout.Width(60));
			tz = GUILayout.Toggle(tz, "Z", GUILayout.Width(60));
			if (EditorGUI.EndChangeCheck()) {
				switch (Settings.Mode) {
					case OperationMode.Fly:
						FlyInvertTranslation = new Vector3(tx ? -1 : 1, ty ? -1 : 1, tz ? -1 : 1);
						break;
					case OperationMode.Orbit:
						OrbitInvertTranslation = new Vector3(tx ? -1 : 1, ty ? -1 : 1, tz ? -1 : 1);
						break;
					case OperationMode.Telekinesis:
						TelekinesisInvertTranslation = new Vector3(tx ? -1 : 1, ty ? -1 : 1, tz ? -1 : 1);
						break;
					case OperationMode.GrabMove:
						GrabMoveInvertTranslation = new Vector3(tx ? -1 : 1, ty ? -1 : 1, tz ? -1 : 1);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Rotation", GUILayout.Width(120));
			EditorGUI.BeginChangeCheck();

			rx = GUILayout.Toggle(rx, "X", GUILayout.Width(60));
			ry = GUILayout.Toggle(ry, "Y", GUILayout.Width(60));
			rz = GUILayout.Toggle(rz, "Z", GUILayout.Width(60));
			if (EditorGUI.EndChangeCheck()) {
				switch (Settings.Mode) {
					case OperationMode.Fly:
						FlyInvertRotation = new Vector3(rx ? -1 : 1, ry ? -1 : 1, rz ? -1 : 1);
						break;
					case OperationMode.Orbit:
						OrbitInvertRotation = new Vector3(rx ? -1 : 1, ry ? -1 : 1, rz ? -1 : 1);
						break;
					case OperationMode.Telekinesis:
						TelekinesisInvertRotation = new Vector3(rx ? -1 : 1, ry ? -1 : 1, rz ? -1 : 1);
						break;
					case OperationMode.GrabMove:
						GrabMoveInvertRotation = new Vector3(rx ? -1 : 1, ry ? -1 : 1, rz ? -1 : 1);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			GUILayout.EndHorizontal();
			#endregion - Axes inversion per mode -

			#region - Focus behavior -

			GUILayout.Space(10);
			GUILayout.Label("Focus Behavior");
			OnlyNavWhenUnityHasFocus = GUILayout.Toggle(OnlyNavWhenUnityHasFocus, "Only navigate when Unity has focus");
			EditorGUI.BeginDisabledGroup(!OnlyNavWhenUnityHasFocus);
			ToggleLedWhenFocusChanged = GUILayout.Toggle(ToggleLedWhenFocusChanged, "Toggle LED when Unity gains/loses focus");
			EditorGUI.EndDisabledGroup();

			#endregion
			
			#region - Runtime editor navigation -

			GUILayout.Space(10);
			GUILayout.Label("Runtime Behavior");
			RuntimeEditorNav = GUILayout.Toggle(RuntimeEditorNav, "Runtime Editor Navigation");
			EditorGUI.BeginDisabledGroup(true);
			GUILayout.TextArea("Only works when scene view has focus and requires\n'Project Settings/Input System Package/Settings/Play Mode Input Behavior'\nto be set to\n'All Devices Respect Game View Focus'");
			EditorGUI.EndDisabledGroup();

			#endregion
			
			#region - Version number -

			GUILayout.Space(10);
			Assembly assembly = typeof(SpaceNavigatorHID).Assembly;
			PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
			string version = packageInfo.version;
			GUILayout.Label($"Version {version}");

			#endregion
			
			GUILayout.EndVertical();
			GUILayout.EndScrollView();

			if (EditorGUI.EndChangeCheck())
				Write();
#endif
			return triggerToolbarRefresh;
		}

		private static bool _showHelp = false;

		private static void DrawQuickHelp()
		{
			// Show help by default for first-time users
			if (!_showHelp && EditorPrefs.GetBool("SpaceNavigator_ShowHelp", false))
			{
				_showHelp = true;
				EditorPrefs.DeleteKey("SpaceNavigator_ShowHelp"); // Only show once
			}
			
			_showHelp = EditorGUILayout.Foldout(_showHelp, "Quick Help", true);
			if (_showHelp)
			{
				EditorGUILayout.HelpBox(
					"Device Status: " + (SpaceNavigatorHID.current != null ? "✓ Connected" : "⚠ Disconnected") + "\n\n" +
					"Quick Start:\n" +
					"• Use Ctrl+Shift+M to cycle navigation modes\n" +
					"• Use Ctrl+Shift+H to toggle horizon lock\n" +
					"• Use Ctrl+Shift+R to toggle rotation lock\n" +
					"• Use Ctrl+Shift+C to recalibrate drift\n\n" +
					"Navigation Modes:\n" +
					"• Object/Orbit (Cyan): Camera orbits around selection\n" +
					"• Camera/Fly (Green): Free camera movement\n" +
					"• Telekinesis (Magenta): Move selected objects\n" +
					"• Walk/Helicopter/Drone (Yellow): Specialized movement",
					MessageType.Info
				);

				if (SpaceNavigatorHID.current == null)
				{
					EditorGUILayout.HelpBox(
						"SpaceMouse not detected. Check:\n" +
						"• Device is connected via USB\n" +
						"• 3DxWare driver is installed\n" +
						"• Device is not used by another app",
						MessageType.Warning
					);

					if (GUILayout.Button("Open 3Dconnexion Support"))
					{
						Application.OpenURL("https://help.3dconnexion.com/");
					}
				}

				EditorGUILayout.Space();
			}
		}

		private static Vector2 _scrollPos;

		static Settings() {
			NavTranslationLock = new Locks("Navigation Translation");
			NavRotationLock = new Locks("Navigation Rotation");
			ManipulateTranslationLock = new Locks("Manipulation Translation");
			ManipulateRotationLock = new Locks("Manipulation Rotation");
		}

		/// <summary>
		/// Write settings to PlayerPrefs.
		/// </summary>
		public static void Write() {
			// Navigation Mode
			PlayerPrefs.SetInt("Navigation mode", (int)Mode);
			// Coordinate System
			PlayerPrefs.SetInt("Coordinate System", (int)CoordSys);
			// Presentation Mode
			PlayerPrefs.SetInt("Presentation Mode", PresentationMode ? 1 : 0);
			PlayerPrefs.SetFloat("Presentation Damping", PresentationDamping);
			// Snap
			PlayerPrefs.SetInt("Snap Translation", SnapTranslation ? 1 : 0);
			PlayerPrefs.SetFloat("Snap Distance", SnapDistance);
			PlayerPrefs.SetInt("Snap Rotation", SnapRotation ? 1 : 0);
			PlayerPrefs.SetInt("Snap Angle", SnapAngle);
			// Lock Horizon
			PlayerPrefs.SetInt("LockHorizon", HorizonLock ? 1 : 0);
			// Lock Cursor on MacOS
			PlayerPrefs.SetInt("MacOSCursorLock", MacOSCursorLock ? 1 : 0);
			// Lock Axis
			NavTranslationLock.Write();
			NavRotationLock.Write();
			ManipulateTranslationLock.Write();
			ManipulateRotationLock.Write();
			// Sensitivity
			for (int gear = 0; gear < Gears; gear++) {
				PlayerPrefs.SetFloat("Translation sensitivity" + gear, TransSens[gear]);
				PlayerPrefs.SetFloat("Translation sensitivity minimum" + gear, TransSensMin[gear]);
				PlayerPrefs.SetFloat("Translation sensitivity maximum" + gear, TransSensMax[gear]);
			}
			PlayerPrefs.SetFloat("Rotation sensitivity", RotSens);
			PlayerPrefs.SetFloat("Rotation sensitivity minimum", RotSensMin);
			PlayerPrefs.SetFloat("Rotation sensitivity maximum", RotSensMax);
			PlayerPrefs.SetInt("Show sensitivity as radio buttons", ShowSpeedGearsAsRadioButtons ? 1 : 0);
			// Focus
			PlayerPrefs.SetInt("OnlyNavWhenUnityHasFocus", OnlyNavWhenUnityHasFocus ? 1 : 0);
			PlayerPrefs.SetInt("ToggleLedWhenFocusChanged", ToggleLedWhenFocusChanged ? 1 : 0);
			// Runtime Editor Navigation
			PlayerPrefs.SetInt("RuntimeEditorNav", RuntimeEditorNav ? 1 : 0);
			// Axis Inversions
			WriteAxisInversions(FlyInvertTranslation, FlyInvertRotation, "Fly");
			WriteAxisInversions(OrbitInvertTranslation, OrbitInvertRotation, "Orbit");
			WriteAxisInversions(TelekinesisInvertTranslation, TelekinesisInvertRotation, "Telekinesis");
			WriteAxisInversions(GrabMoveInvertTranslation, GrabMoveInvertRotation, "Grab move");
			// Calibration
			PlayerPrefs.SetFloat("Translation sensitivity epsilon", TransSensEpsilon);
			PlayerPrefs.SetFloat("Rotation sensitivity epsilon", RotSensEpsilon);
		}

		/// <summary>
		/// Read settings from PlayerPrefs.
		/// </summary>
		public static void Read() {
			// Navigation Mode
			Mode = (OperationMode)PlayerPrefs.GetInt("Navigation mode", (int)OperationMode.Fly);
			// Coordinate System
			CoordSys = (CoordinateSystem)PlayerPrefs.GetInt("Coordinate System", (int)CoordinateSystem.Camera);
			// Presentation Mode
			PresentationMode = PlayerPrefs.GetInt("Presentation Mode", 0) == 1;
			PresentationDamping = PlayerPrefs.GetFloat("Presentation Damping", 0.015f);
			// Snap
			SnapTranslation = PlayerPrefs.GetInt("Snap Translation", 0) == 1;
			SnapDistance = PlayerPrefs.GetFloat("Snap Distance", 0.1f);
			SnapRotation = PlayerPrefs.GetInt("Snap Rotation", 0) == 1;
			SnapAngle = PlayerPrefs.GetInt("Snap Angle", 45);
			// Lock Horizon
			HorizonLock = PlayerPrefs.GetInt("LockHorizon", 1) == 1;
			// Lock Cursor on MacOS
			MacOSCursorLock = PlayerPrefs.GetInt("MacOSCursorLock", 1) == 1;
			// Lock Axis
			NavTranslationLock.Read();
			NavRotationLock.Read();
			ManipulateTranslationLock.Read();
			ManipulateRotationLock.Read();
			// Sensitivity
			for (int gear = 0; gear < Gears; gear++) {
				TransSens[gear] = PlayerPrefs.GetFloat("Translation sensitivity" + gear, TransSensDefault[gear]);
				TransSensMin[gear] = PlayerPrefs.GetFloat("Translation sensitivity minimum" + gear, TransSensMinDefault[gear]);
				TransSensMax[gear] = PlayerPrefs.GetFloat("Translation sensitivity maximum" + gear, TransSensMaxDefault[gear]);
			}
			RotSens = PlayerPrefs.GetFloat("Rotation sensitivity", RotSensDefault);
			RotSensMin = PlayerPrefs.GetFloat("Rotation sensitivity minimum", RotSensMinDefault);
			RotSensMax = PlayerPrefs.GetFloat("Rotation sensitivity maximum", RotSensMaxDefault);
			ShowSpeedGearsAsRadioButtons = PlayerPrefs.GetInt("Show sensitivity as radio buttons", 0) == 1;
			// Focus
			OnlyNavWhenUnityHasFocus = PlayerPrefs.GetInt("OnlyNavWhenUnityHasFocus", 1) == 1;
			ToggleLedWhenFocusChanged = PlayerPrefs.GetInt("ToggleLedWhenFocusChanged", 0) == 1;
			// Runtime Editor Navigation
			RuntimeEditorNav = PlayerPrefs.GetInt("RuntimeEditorNav", 1) == 1;
			// Axis Inversions
			ReadAxisInversions(ref FlyInvertTranslation, ref FlyInvertRotation, "Fly");
			ReadAxisInversions(ref OrbitInvertTranslation, ref OrbitInvertRotation, "Orbit");
			ReadAxisInversions(ref TelekinesisInvertTranslation, ref TelekinesisInvertRotation, "Telekinesis");
			ReadAxisInversions(ref GrabMoveInvertTranslation, ref GrabMoveInvertRotation, "Grab move");
			// Calibration
			TransSensEpsilon = PlayerPrefs.GetFloat("Translation sensitivity epsilon", TransSensEpsilonDefault);
			RotSensEpsilon = PlayerPrefs.GetFloat("Rotation sensitivity epsilon", RotSensEpsilonDefault);
		}

		/// <summary>
		/// Utility function to write axis inversions to PlayerPrefs.
		/// </summary>
		/// <param name="translation"></param>
		/// <param name="rotation"></param>
		/// <param name="baseName"></param>
		private static void WriteAxisInversions(Vector3 translation, Vector3 rotation, string baseName) {
			PlayerPrefs.SetInt(baseName + " invert translation x", translation.x < 0 ? -1 : 1);
			PlayerPrefs.SetInt(baseName + " invert translation y", translation.y < 0 ? -1 : 1);
			PlayerPrefs.SetInt(baseName + " invert translation z", translation.z < 0 ? -1 : 1);
			PlayerPrefs.SetInt(baseName + " invert rotation x", rotation.x < 0 ? -1 : 1);
			PlayerPrefs.SetInt(baseName + " invert rotation y", rotation.y < 0 ? -1 : 1);
			PlayerPrefs.SetInt(baseName + " invert rotation z", rotation.z < 0 ? -1 : 1);
		}

		/// <summary>
		/// Utility function to read axis inversions from PlayerPrefs.
		/// </summary>
		/// <param name="translation"></param>
		/// <param name="rotation"></param>
		/// <param name="baseName"></param>
		private static void ReadAxisInversions(ref Vector3 translation, ref Vector3 rotation, string baseName) {
			translation.x = PlayerPrefs.GetInt(baseName + " invert translation x", 1);
			translation.y = PlayerPrefs.GetInt(baseName + " invert translation y", 1);
			translation.z = PlayerPrefs.GetInt(baseName + " invert translation z", 1);
			rotation.x = PlayerPrefs.GetInt(baseName + " invert rotation x", 1);
			rotation.y = PlayerPrefs.GetInt(baseName + " invert rotation y", 1);
			rotation.z = PlayerPrefs.GetInt(baseName + " invert rotation z", 1);
		}

		/// <summary>
		/// Utility function to determine whether a specific axis is locked for the specified DoF.
		/// </summary>
		/// <param name="doF"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
		public static bool GetLock(DoF doF, Axis axis) {
			Locks locks = doF == DoF.Translation ? GetCurrentTranslationLocks() : GetCurrentRotationLocks();

			switch (axis) {
				case Axis.X:
					return (locks.X || locks.All) && !Application.isPlaying;
				case Axis.Y:
					return (locks.Y || locks.All) && !Application.isPlaying;
				case Axis.Z:
					return (locks.Z || locks.All) && !Application.isPlaying;
				default:
					throw new ArgumentOutOfRangeException("axis");
			}
		}

		/// <summary>
		/// Returns a vector which can be multiplied with an input vector to apply the current locks of the specified DoF. 
		/// </summary>
		/// <param name="doF"></param>
		/// <returns></returns>
		public static Vector3 GetLocks(DoF doF)
		{
			Locks locks = doF == DoF.Translation ? GetCurrentTranslationLocks() : GetCurrentRotationLocks();

			return new Vector3(
				(locks.X || locks.All) && !Application.isPlaying ? 0 : 1,
				(locks.Y || locks.All) && !Application.isPlaying ? 0 : 1,
				(locks.Z || locks.All) && !Application.isPlaying ? 0 : 1);
		}
		
		private static Locks GetCurrentRotationLocks() =>  Mode == OperationMode.Fly || Mode == OperationMode.Orbit ? NavRotationLock : ManipulateRotationLock;
		private static Locks GetCurrentTranslationLocks() => Mode == OperationMode.Fly || Mode == OperationMode.Orbit ? NavTranslationLock : ManipulateTranslationLock;
	}
}