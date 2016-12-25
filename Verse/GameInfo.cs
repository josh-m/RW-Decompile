using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public sealed class GameInfo : IExposable
	{
		public bool permadeathMode;

		public string permadeathModeUniqueName;

		private float realPlayTimeInteracting;

		private float lastInputRealTime;

		public float RealPlayTimeInteracting
		{
			get
			{
				return this.realPlayTimeInteracting;
			}
		}

		public void GameInfoOnGUI()
		{
			if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseMove || Event.current.type == EventType.KeyDown)
			{
				this.lastInputRealTime = Time.realtimeSinceStartup;
			}
		}

		public void GameInfoUpdate()
		{
			if (Time.realtimeSinceStartup < this.lastInputRealTime + 90f && Find.MainTabsRoot.OpenTab != MainTabDefOf.Menu && Current.ProgramState == ProgramState.Playing && !Find.WindowStack.IsOpen<Dialog_Options>())
			{
				this.realPlayTimeInteracting += RealTime.realDeltaTime;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<float>(ref this.realPlayTimeInteracting, "realPlayTimeInteracting", 0f, false);
			Scribe_Values.LookValue<bool>(ref this.permadeathMode, "permadeathMode", false, false);
			Scribe_Values.LookValue<string>(ref this.permadeathModeUniqueName, "permadeathModeUniqueName", null, false);
		}
	}
}
