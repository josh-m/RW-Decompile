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

		private static Root_Entry rootEntryInt;

		private static Root_Play rootPlayInt;

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

		public static Root_Entry Root_Entry
		{
			get
			{
				return Current.rootEntryInt;
			}
		}

		public static Root_Play Root_Play
		{
			get
			{
				return Current.rootPlayInt;
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
				Current.rootEntryInt = GameObject.Find("GameRoot").GetComponent<Root_Entry>();
				Current.rootPlayInt = null;
				Current.rootInt = Current.rootEntryInt;
				Current.cameraDriverInt = null;
				Current.colorCorrectionCurvesInt = null;
			}
			else if (GenScene.InPlayScene)
			{
				Current.ProgramState = ProgramState.MapInitializing;
				Current.rootEntryInt = null;
				Current.rootPlayInt = GameObject.Find("GameRoot").GetComponent<Root_Play>();
				Current.rootInt = Current.rootPlayInt;
				Current.cameraDriverInt = Current.cameraInt.GetComponent<CameraDriver>();
				Current.colorCorrectionCurvesInt = Current.cameraInt.GetComponent<ColorCorrectionCurves>();
			}
		}
	}
}
