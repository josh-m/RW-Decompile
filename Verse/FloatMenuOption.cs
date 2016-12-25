using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class FloatMenuOption
	{
		public const float MaxWidth = 300f;

		private const float NormalVerticalMargin = 4f;

		private const float TinyVerticalMargin = 1f;

		private const float NormalHorizontalMargin = 6f;

		private const float TinyHorizontalMargin = 3f;

		private const float MouseOverLabelShift = 4f;

		public const float ExtraPartHeight = 30f;

		private string labelInt;

		public Action action;

		public MenuOptionPriority priority = MenuOptionPriority.Medium;

		public bool autoTakeable;

		public Action mouseoverGuiAction;

		public Thing revalidateClickTarget;

		public float extraPartWidth;

		public Func<Rect, bool> extraPartOnGUI;

		public string tutorTag;

		private FloatMenuSizeMode sizeMode;

		private float cachedRequiredHeight;

		private float cachedRequiredWidth;

		private static readonly Color ColorBGActive;

		private static readonly Color ColorBGActiveMouseover;

		private static readonly Color ColorBGDisabled;

		private static readonly Color ColorTextActive;

		private static readonly Color ColorTextDisabled;

		public string Label
		{
			get
			{
				return this.labelInt;
			}
			set
			{
				if (value.NullOrEmpty())
				{
					value = "(missing label)";
				}
				this.labelInt = value.TrimEnd(new char[0]);
				this.SetSizeMode(this.sizeMode);
			}
		}

		private float VerticalMargin
		{
			get
			{
				return (this.sizeMode != FloatMenuSizeMode.Normal) ? 1f : 4f;
			}
		}

		private float HorizontalMargin
		{
			get
			{
				return (this.sizeMode != FloatMenuSizeMode.Normal) ? 3f : 6f;
			}
		}

		private GameFont CurrentFont
		{
			get
			{
				return (this.sizeMode != FloatMenuSizeMode.Normal) ? GameFont.Tiny : GameFont.Small;
			}
		}

		public bool Disabled
		{
			get
			{
				return this.action == null;
			}
			set
			{
				if (value)
				{
					this.action = null;
				}
			}
		}

		public float RequiredHeight
		{
			get
			{
				return this.cachedRequiredHeight;
			}
		}

		public float RequiredWidth
		{
			get
			{
				return this.cachedRequiredWidth;
			}
		}

		public FloatMenuOption()
		{
		}

		public FloatMenuOption(string label, Action action, MenuOptionPriority priority = MenuOptionPriority.Medium, Action mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null)
		{
			this.Label = label;
			this.action = action;
			this.priority = priority;
			this.revalidateClickTarget = revalidateClickTarget;
			this.mouseoverGuiAction = mouseoverGuiAction;
			this.extraPartWidth = extraPartWidth;
			this.extraPartOnGUI = extraPartOnGUI;
		}

		static FloatMenuOption()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ColorInt colorInt = new ColorInt(21, 25, 29);
			FloatMenuOption.ColorBGActive = colorInt.ToColor;
			ColorInt colorInt2 = new ColorInt(29, 45, 50);
			FloatMenuOption.ColorBGActiveMouseover = colorInt2.ToColor;
			ColorInt colorInt3 = new ColorInt(40, 40, 40);
			FloatMenuOption.ColorBGDisabled = colorInt3.ToColor;
			FloatMenuOption.ColorTextActive = Color.white;
			FloatMenuOption.ColorTextDisabled = new Color(0.9f, 0.9f, 0.9f);
		}

		public void SetSizeMode(FloatMenuSizeMode newSizeMode)
		{
			this.sizeMode = newSizeMode;
			Text.Font = this.CurrentFont;
			this.cachedRequiredHeight = Text.CalcHeight(this.Label, 300f - 2f * this.HorizontalMargin - 4f - this.extraPartWidth) + 2f * this.VerticalMargin;
			this.cachedRequiredWidth = this.HorizontalMargin + 4f + Text.CalcSize(this.Label).x + this.extraPartWidth + this.HorizontalMargin;
		}

		public void Chosen(bool colonistOrdering)
		{
			if (!this.Disabled)
			{
				if (this.action != null)
				{
					if (colonistOrdering)
					{
						SoundDefOf.ColonistOrdered.PlayOneShotOnCamera();
					}
					this.action();
				}
			}
			else
			{
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
			}
		}

		public virtual bool DoGUI(Rect rect, bool colonistOrdering)
		{
			bool flag = !this.Disabled && Mouse.IsOver(rect);
			bool flag2 = false;
			Text.Font = this.CurrentFont;
			Rect rect2 = rect;
			rect2.xMin += this.HorizontalMargin;
			rect2.width -= this.HorizontalMargin;
			rect2.width -= this.extraPartWidth;
			if (flag)
			{
				rect2.x += 4f;
			}
			Rect rect3 = default(Rect);
			if (this.extraPartWidth != 0f)
			{
				float num = Mathf.Min(Text.CalcSize(this.Label).x, rect2.width - 4f);
				rect3 = new Rect(rect2.xMin + num, rect2.yMin, this.extraPartWidth, 30f);
				flag2 = Mouse.IsOver(rect3);
			}
			if (!this.Disabled)
			{
				MouseoverSounds.DoRegion(rect);
			}
			Color color = GUI.color;
			if (this.Disabled)
			{
				GUI.color = FloatMenuOption.ColorBGDisabled * color;
			}
			else if (flag && !flag2)
			{
				GUI.color = FloatMenuOption.ColorBGActiveMouseover * color;
			}
			else
			{
				GUI.color = FloatMenuOption.ColorBGActive * color;
			}
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = (this.Disabled ? FloatMenuOption.ColorTextDisabled : FloatMenuOption.ColorTextActive) * color;
			if (this.sizeMode == FloatMenuSizeMode.Tiny)
			{
				rect2.y += 3f;
			}
			Widgets.DrawAtlas(rect, TexUI.FloatMenuOptionBG);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect2, this.Label);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = color;
			if (this.extraPartOnGUI != null)
			{
				bool flag3 = this.extraPartOnGUI(rect3);
				GUI.color = color;
				if (flag3)
				{
					return true;
				}
			}
			if (flag && this.mouseoverGuiAction != null)
			{
				this.mouseoverGuiAction();
			}
			if (this.tutorTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, this.tutorTag);
			}
			if (!Widgets.ButtonInvisible(rect, false))
			{
				return false;
			}
			if (this.tutorTag != null && !TutorSystem.AllowAction(this.tutorTag))
			{
				return false;
			}
			this.Chosen(colonistOrdering);
			if (this.tutorTag != null)
			{
				TutorSystem.Notify_Event(this.tutorTag);
			}
			return true;
		}

		public override string ToString()
		{
			return "FloatMenuOption(" + this.Label + ")";
		}
	}
}
