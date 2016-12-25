using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class Need : IExposable
	{
		public const float MaxDrawHeight = 70f;

		private const float BarInstantMarkerSize = 12f;

		public NeedDef def;

		protected Pawn pawn;

		protected float curLevelInt;

		protected List<float> threshPercents;

		private static readonly Texture2D BarInstantMarkerTex = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarker", true);

		public string LabelCap
		{
			get
			{
				return this.def.LabelCap;
			}
		}

		public float CurInstantLevelPercentage
		{
			get
			{
				return this.CurInstantLevel / this.MaxLevel;
			}
		}

		public virtual int GUIChangeArrow
		{
			get
			{
				return 0;
			}
		}

		public virtual float CurInstantLevel
		{
			get
			{
				return -1f;
			}
		}

		public virtual float MaxLevel
		{
			get
			{
				return 1f;
			}
		}

		public virtual float CurLevel
		{
			get
			{
				return this.curLevelInt;
			}
			set
			{
				this.curLevelInt = Mathf.Clamp(value, 0f, this.MaxLevel);
			}
		}

		public float CurLevelPercentage
		{
			get
			{
				return this.CurLevel / this.MaxLevel;
			}
			set
			{
				this.CurLevel = value * this.MaxLevel;
			}
		}

		public Need()
		{
		}

		public Need(Pawn newPawn)
		{
			this.pawn = newPawn;
			this.SetInitialLevel();
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.LookDef<NeedDef>(ref this.def, "def");
			Scribe_Values.LookValue<float>(ref this.curLevelInt, "curLevel", 0f, false);
		}

		public abstract void NeedInterval();

		public virtual string GetTipString()
		{
			return string.Concat(new string[]
			{
				this.LabelCap,
				": ",
				this.CurLevelPercentage.ToStringPercent(),
				"\n",
				this.def.description
			});
		}

		public virtual void SetInitialLevel()
		{
			this.CurLevelPercentage = 0.5f;
		}

		public void ForceSetLevel(float levelPercent)
		{
			this.CurLevelPercentage = levelPercent;
		}

		public virtual void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (rect.height > 70f)
			{
				float num = (rect.height - 70f) / 2f;
				rect.height = 70f;
				rect.y += num;
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (doTooltip)
			{
				TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
			}
			float num2 = 14f;
			float num3 = (customMargin < 0f) ? (num2 + 15f) : customMargin;
			if (rect.height < 50f)
			{
				num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
			}
			Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
			Text.Anchor = TextAnchor.LowerLeft;
			Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
			Widgets.Label(rect2, this.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
			rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
			Widgets.FillableBar(rect3, this.CurLevelPercentage);
			if (drawArrows)
			{
				Widgets.FillableBarChangeArrows(rect3, this.GUIChangeArrow);
			}
			if (this.threshPercents != null)
			{
				for (int i = 0; i < Mathf.Min(this.threshPercents.Count, maxThresholdMarkers); i++)
				{
					this.DrawBarThreshold(rect3, this.threshPercents[i]);
				}
			}
			float curInstantLevelPercentage = this.CurInstantLevelPercentage;
			if (curInstantLevelPercentage >= 0f)
			{
				this.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
			}
			if (!this.def.tutorHighlightTag.NullOrEmpty())
			{
				UIHighlighter.HighlightOpportunity(rect, this.def.tutorHighlightTag);
			}
			Text.Font = GameFont.Small;
		}

		protected void DrawBarInstantMarkerAt(Rect barRect, float pct)
		{
			if (pct > 1f)
			{
				Log.ErrorOnce(this.def + " drawing bar percent > 1 : " + pct, 6932178);
			}
			float num = 12f;
			if (barRect.width < 150f)
			{
				num /= 2f;
			}
			Vector2 vector = new Vector2(barRect.x + barRect.width * pct, barRect.y + barRect.height);
			Rect position = new Rect(vector.x - num / 2f, vector.y, num, num);
			GUI.DrawTexture(position, Need.BarInstantMarkerTex);
		}

		private void DrawBarThreshold(Rect barRect, float threshPct)
		{
			float num = (float)((barRect.width <= 60f) ? 1 : 2);
			Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
			Texture2D image;
			if (threshPct < this.CurLevelPercentage)
			{
				image = BaseContent.BlackTex;
				GUI.color = new Color(1f, 1f, 1f, 0.9f);
			}
			else
			{
				image = BaseContent.GreyTex;
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
			}
			GUI.DrawTexture(position, image);
			GUI.color = Color.white;
		}
	}
}
