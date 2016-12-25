using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class TimeControls
	{
		public static readonly Vector2 TimeButSize = new Vector2(32f, 24f);

		private static readonly string[] SpeedSounds = new string[]
		{
			"ClockStop",
			"ClockNormal",
			"ClockFast",
			"ClockSuperfast",
			"ClockSuperfast"
		};

		private static readonly TimeSpeed[] CachedTimeSpeedValues = (TimeSpeed[])Enum.GetValues(typeof(TimeSpeed));

		private static void PlaySoundOf(TimeSpeed speed)
		{
			SoundDef.Named(TimeControls.SpeedSounds[(int)speed]).PlayOneShotOnCamera();
		}

		public static void DoTimeControlsGUI(Rect timerRect)
		{
			TickManager tickManager = Find.TickManager;
			GUI.BeginGroup(timerRect);
			Rect rect = new Rect(0f, 0f, TimeControls.TimeButSize.x, TimeControls.TimeButSize.y);
			for (int i = 0; i < TimeControls.CachedTimeSpeedValues.Length; i++)
			{
				TimeSpeed timeSpeed = TimeControls.CachedTimeSpeedValues[i];
				if (timeSpeed != TimeSpeed.Ultrafast)
				{
					if (Widgets.ButtonImage(rect, TexButton.SpeedButtonTextures[(int)timeSpeed]))
					{
						if (timeSpeed == TimeSpeed.Paused)
						{
							tickManager.TogglePaused();
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
						}
						else
						{
							tickManager.CurTimeSpeed = timeSpeed;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
						}
						TimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
					}
					if (tickManager.CurTimeSpeed == timeSpeed)
					{
						GUI.DrawTexture(rect, TexUI.HighlightTex);
					}
					rect.x += rect.width;
				}
			}
			if (Find.TickManager.slower.ForcedNormalSpeed)
			{
				Widgets.DrawLineHorizontal(rect.width * 2f, rect.height / 2f, rect.width * 2f);
			}
			GUI.EndGroup();
			GenUI.AbsorbClicksInRect(timerRect);
			UIHighlighter.HighlightOpportunity(timerRect, "TimeControls");
			if (Event.current.type == EventType.KeyDown)
			{
				if (KeyBindingDefOf.TogglePause.KeyDownEvent)
				{
					Find.TickManager.TogglePaused();
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeedNormal.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeedFast.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeedSuperfast.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (Prefs.DevMode)
				{
					if (KeyBindingDefOf.TimeSpeedUltrafast.KeyDownEvent)
					{
						Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
						TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
						Event.current.Use();
					}
					if (KeyBindingDefOf.TickOnce.KeyDownEvent && tickManager.CurTimeSpeed == TimeSpeed.Paused)
					{
						tickManager.DoSingleTick();
						SoundDef.Named(TimeControls.SpeedSounds[0]).PlayOneShotOnCamera();
					}
				}
			}
		}
	}
}
