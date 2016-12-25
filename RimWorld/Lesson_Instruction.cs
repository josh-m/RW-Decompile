using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Lesson_Instruction : Lesson
	{
		private const float RectWidth = 310f;

		private const float BarHeight = 30f;

		public InstructionDef def;

		protected Map Map
		{
			get
			{
				return Find.AnyPlayerHomeMap;
			}
		}

		protected virtual float ProgressPercent
		{
			get
			{
				return -1f;
			}
		}

		protected virtual bool ShowProgressBar
		{
			get
			{
				return this.ProgressPercent >= 0f;
			}
		}

		public override string DefaultRejectInputMessage
		{
			get
			{
				return this.def.rejectInputMessage;
			}
		}

		public override void ExposeData()
		{
			Scribe_Defs.LookDef<InstructionDef>(ref this.def, "def");
			base.ExposeData();
		}

		public override void OnActivated()
		{
			base.OnActivated();
			if (this.def.giveOnActivateCount > 0)
			{
				Thing thing = ThingMaker.MakeThing(this.def.giveOnActivateDef, null);
				thing.stackCount = this.def.giveOnActivateCount;
				GenSpawn.Spawn(thing, TutorUtility.FindUsableRect(2, 2, this.Map, 0f, false).CenterCell, this.Map);
			}
			if (this.def.resetBuildDesignatorStuffs)
			{
				foreach (DesignationCategoryDef current in DefDatabase<DesignationCategoryDef>.AllDefs)
				{
					foreach (Designator current2 in current.ResolvedAllowedDesignators)
					{
						Designator_Build designator_Build = current2 as Designator_Build;
						if (designator_Build != null)
						{
							designator_Build.ResetStuffToDefault();
						}
					}
				}
			}
		}

		public override void LessonOnGUI()
		{
			Text.Font = GameFont.Small;
			string textAdj = this.def.text.AdjustedForKeys();
			float num = Text.CalcHeight(textAdj, 290f);
			float num2 = num + 20f;
			if (this.ShowProgressBar)
			{
				num2 += 47f;
			}
			Vector2 b = new Vector2((float)UI.screenWidth - 17f - 155f, 17f + num2 / 2f);
			if (!Find.TutorialState.introDone)
			{
				float screenOverlayAlpha = 0f;
				if (this.def.startCentered)
				{
					Vector2 vector = new Vector2((float)(UI.screenWidth / 2), (float)(UI.screenHeight / 2));
					if (base.AgeSeconds < 4f)
					{
						b = vector;
						screenOverlayAlpha = 0.9f;
					}
					else if (base.AgeSeconds < 5f)
					{
						float t = (base.AgeSeconds - 4f) / 1f;
						b = Vector2.Lerp(vector, b, t);
						screenOverlayAlpha = Mathf.Lerp(0.9f, 0f, t);
					}
				}
				if (screenOverlayAlpha > 0f)
				{
					Rect fullScreenRect = new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight);
					Find.WindowStack.ImmediateWindow(972651, fullScreenRect, WindowLayer.SubSuper, delegate
					{
						GUI.color = new Color(1f, 1f, 1f, screenOverlayAlpha);
						GUI.DrawTexture(fullScreenRect, BaseContent.BlackTex);
						GUI.color = Color.white;
					}, false, true, 0f);
				}
				else
				{
					Find.TutorialState.introDone = true;
				}
			}
			Rect mainRect = new Rect(b.x - 155f, b.y - num2 / 2f - 10f, 310f, num2);
			Find.WindowStack.ImmediateWindow(177706, mainRect, WindowLayer.Super, delegate
			{
				Rect rect = mainRect.AtZero();
				Widgets.DrawWindowBackgroundTutor(rect);
				Rect rect2 = rect.ContractedBy(10f);
				Text.Font = GameFont.Small;
				Rect rect3 = rect2;
				if (this.ShowProgressBar)
				{
					rect3.height -= 47f;
				}
				Widgets.Label(rect3, textAdj);
				if (this.ShowProgressBar)
				{
					Rect rect4 = new Rect(rect2.x, rect2.yMax - 30f, rect2.width, 30f);
					Widgets.FillableBar(rect4, this.ProgressPercent, LearningReadout.ProgressBarFillTex);
				}
				if (this.AgeSeconds < 0.5f)
				{
					GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, this.AgeSeconds / 0.5f));
					GUI.DrawTexture(rect, BaseContent.WhiteTex);
					GUI.color = Color.white;
				}
			}, false, false, 1f);
			if (this.def.highlightTags != null)
			{
				for (int i = 0; i < this.def.highlightTags.Count; i++)
				{
					UIHighlighter.HighlightTag(this.def.highlightTags[i]);
				}
			}
		}

		public override void Notify_Event(EventPack ep)
		{
			if (this.def.eventTagsEnd != null && this.def.eventTagsEnd.Contains(ep.Tag))
			{
				Find.ActiveLesson.Deactivate();
			}
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			return this.def.actionTagsAllowed != null && this.def.actionTagsAllowed.Contains(ep.Tag);
		}

		public override void PostDeactivated()
		{
			SoundDefOf.CommsWindow_Close.PlayOneShotOnCamera();
			TutorSystem.Notify_Event("InstructionDeactivated-" + this.def.defName);
			if (this.def.endTutorial)
			{
				Find.ActiveLesson.Deactivate();
				Find.TutorialState.Notify_TutorialEnding();
				LessonAutoActivator.Notify_TutorialEnding();
			}
		}
	}
}
