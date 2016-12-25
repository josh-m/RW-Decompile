using RimWorld.Planet;
using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Verse
{
	public static class Current
	{
		private static ProgramState programStateInt;

		private static Root rootInt;

		private static RootEntry rootEntryInt;

		private static RootMap rootMapInt;

		private static Camera cameraInt;

		private static CameraDriver cameraDriverInt;

		private static ColorCorrectionCurves colorCorrectionCurvesInt;

		private static Game gameInt;

		private static World creatingWorldInt;

		public static Root Root
		{
			get
			{
				return Current.rootInt;
			}
		}

		public static RootEntry RootEntry
		{
			get
			{
				return Current.rootEntryInt;
			}
		}

		public static RootMap RootMap
		{
			get
			{
				return Current.rootMapInt;
			}
		}

		public static Camera Camera
		{
			get
			{
				return Current.cameraInt;
			}
		}

		public static CameraDriver CameraDriver
		{
			get
			{
				return Current.cameraDriverInt;
			}
		}

		public static ColorCorrectionCurves ColorCorrectionCurves
		{
			get
			{
				return Current.colorCorrectionCurvesInt;
			}
		}

		public static Game Game
		{
			get
			{
				return Current.gameInt;
			}
			set
			{
				Current.gameInt = value;
			}
		}

		public static World CreatingWorld
		{
			get
			{
				return Current.creatingWorldInt;
			}
			set
			{
				Current.creatingWorldInt = value;
			}
		}

		public static ProgramState ProgramState
		{
			get
			{
				return Current.programStateInt;
			}
			set
			{
				Current.programStateInt = value;
			}
		}

		public static void Notify_LoadedSceneChanged()
		{
			Current.cameraInt = GameObject.Find("Camera").GetComponent<Camera>();
			if (GenScene.InEntryScene)
			{
				Current.ProgramState = ProgramState.Entry;
				Current.rootEntryInt = GameObject.Find("GameRoot").GetComponent<RootEntry>();
				Current.rootMapInt = null;
				Current.rootInt = Current.rootEntryInt;
				Current.cameraDriverInt = null;
				Current.colorCorrectionCurvesInt = null;
			}
			else if (GenScene.InMapScene)
			{
				Current.ProgramState = ProgramState.MapInitializing;
				Current.rootEntryInt = null;
				Current.rootMapInt = GameObject.Find("GameRoot").GetComponent<RootMap>();
				Current.rootInt = Current.rootMapInt;
				Current.cameraDriverInt = Current.cameraInt.GetComponent<CameraDriver>();
				Current.colorCorrectionCurvesInt = Current.cameraInt.GetComponent<ColorCorrectionCurves>();
			}
		}
	}
}
