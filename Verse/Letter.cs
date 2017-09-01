using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Letter : IExposable
	{
		public const float DrawWidth = 38f;

		public const float DrawHeight = 30f;

		private const float FallTime = 1f;

		private const float FallDistance = 200f;

		public LetterDef def;

		public string label;

		public GlobalTargetInfo lookTarget = GlobalTargetInfo.Invalid;

		public float arrivalTime;

		public string debugInfo;

		public virtual bool StillValid
		{
			get
			{
				return (this.lookTarget.Thing == null || !this.lookTarget.Thing.Destroyed) && (this.lookTarget.WorldObject == null || this.lookTarget.WorldObject.Spawned);
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return Find.World;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look<LetterDef>(ref this.def, "def");
			Scribe_Values.Look<string>(ref this.label, "label", null, false);
			if (Scribe.mode == LoadSaveMode.Saving && this.lookTarget.HasThing && this.lookTarget.Thing.Destroyed)
			{
				this.lookTarget = GlobalTargetInfo.Invalid;
			}
			Scribe_TargetInfo.Look(ref this.lookTarget, "lookTarget");
		}

		public virtual void DrawButtonAt(float topY)
		{
			float num = (float)UI.screenWidth - 38f - 12f;
			Rect rect = new Rect(num, topY, 38f, 30f);
			Rect rect2 = new Rect(rect);
			float num2 = Time.time - this.arrivalTime;
			Color color = this.def.color;
			if (num2 < 1f)
			{
				rect2.y -= (1f - num2) * 200f;
				color.a = num2 / 1f;
			}
			if (!Mouse.IsOver(rect) && this.def.bounce && num2 > 15f && num2 % 5f < 1f)
			{
				float num3 = (float)UI.screenWidth * 0.06f;
				float num4 = 2f * (num2 % 1f) - 1f;
				float num5 = num3 * (1f - num4 * num4);
				rect2.x -= num5;
			}
			if (this.def.flashInterval > 0f)
			{
				float num6 = Time.time - (this.arrivalTime + 1f);
				if (num6 > 0f && num6 % this.def.flashInterval < 1f)
				{
					GenUI.DrawFlash(num, topY, (float)UI.screenWidth * 0.6f, Pulser.PulseBrightness(1f, 1f, num6) * 0.55f, this.def.flashColor);
				}
			}
			GUI.color = color;
			Widgets.DrawShadowAround(rect2);
			GUI.DrawTexture(rect2, this.def.Icon);
			GUI.color = Color.white;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperRight;
			string text = this.PostProcessedLabel();
			Vector2 vector = Text.CalcSize(text);
			float x = vector.x;
			float y = vector.y;
			Vector2 vector2 = new Vector2(rect2.x + rect2.width / 2f, rect2.center.y - y / 4f);
			float num7 = vector2.x + x / 2f - (float)(UI.screenWidth - 2);
			if (num7 > 0f)
			{
				vector2.x -= num7;
			}
			Rect position = new Rect(vector2.x - x / 2f - 4f - 1f, vector2.y, x + 8f, 12f);
			GUI.DrawTexture(position, TexUI.GrayTextBG);
			GUI.color = new Color(1f, 1f, 1f, 0.75f);
			Rect rect3 = new Rect(vector2.x - x / 2f, vector2.y - 3f, x, 999f);
			Widgets.Label(rect3, text);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
			{
				SoundDefOf.Click.PlayOneShotOnCamera(null);
				Find.LetterStack.RemoveLetter(this);
				Event.current.Use();
			}
			if (Widgets.ButtonInvisible(rect2, false))
			{
				this.OpenLetter();
				Event.current.Use();
			}
		}

		public virtual void CheckForMouseOverTextAt(float topY)
		{
			float num = (float)UI.screenWidth - 38f - 12f;
			Rect rect = new Rect(num, topY, 38f, 30f);
			if (Mouse.IsOver(rect))
			{
				Find.LetterStack.Notify_LetterMouseover(this);
				string mouseoverText = this.GetMouseoverText();
				if (!mouseoverText.NullOrEmpty())
				{
					Text.Font = GameFont.Small;
					Text.Anchor = TextAnchor.UpperLeft;
					float num2 = Text.CalcHeight(mouseoverText, 310f);
					num2 += 20f;
					float x = num - 330f - 10f;
					Rect infoRect = new Rect(x, topY - num2 / 2f, 330f, num2);
					Find.WindowStack.ImmediateWindow(2768333, infoRect, WindowLayer.Super, delegate
					{
						Text.Font = GameFont.Small;
						Rect position = infoRect.AtZero().ContractedBy(10f);
						GUI.BeginGroup(position);
						Widgets.Label(new Rect(0f, 0f, position.width, position.height), mouseoverText);
						GUI.EndGroup();
					}, true, false, 1f);
				}
			}
		}

		protected abstract string GetMouseoverText();

		public abstract void OpenLetter();

		public virtual void Received()
		{
		}

		public virtual void Removed()
		{
		}

		protected virtual string PostProcessedLabel()
		{
			return this.label;
		}
	}
}
