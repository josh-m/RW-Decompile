using System;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class PawnColumnWorker
	{
		public PawnColumnDef def;

		protected const int DefaultCellHeight = 30;

		private static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting", true);

		private static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending", true);

		protected virtual Color DefaultHeaderColor
		{
			get
			{
				return Color.white;
			}
		}

		protected virtual GameFont DefaultHeaderFont
		{
			get
			{
				return GameFont.Small;
			}
		}

		public virtual void DoHeader(Rect rect, PawnTable table)
		{
			if (!this.def.label.NullOrEmpty())
			{
				Text.Font = this.DefaultHeaderFont;
				GUI.color = this.DefaultHeaderColor;
				Text.Anchor = TextAnchor.LowerCenter;
				Rect rect2 = rect;
				rect2.y += 3f;
				Widgets.Label(rect2, this.def.LabelCap.Truncate(rect.width, null));
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				Text.Font = GameFont.Small;
			}
			else if (this.def.HeaderIcon != null)
			{
				Vector2 headerIconSize = this.def.HeaderIconSize;
				int num = (int)((rect.width - headerIconSize.x) / 2f);
				Rect position = new Rect(rect.x + (float)num, rect.yMax - headerIconSize.y, headerIconSize.x, headerIconSize.y);
				GUI.DrawTexture(position, this.def.HeaderIcon);
			}
			if (table.SortingBy == this.def)
			{
				Texture2D texture2D = (!table.SortingDescending) ? PawnColumnWorker.SortingIcon : PawnColumnWorker.SortingDescendingIcon;
				Rect position2 = new Rect(rect.xMax - (float)texture2D.width - 1f, rect.yMax - (float)texture2D.height - 1f, (float)texture2D.width, (float)texture2D.height);
				GUI.DrawTexture(position2, texture2D);
			}
			if (this.def.HeaderInteractable)
			{
				Rect interactableHeaderRect = this.GetInteractableHeaderRect(rect, table);
				Widgets.DrawHighlightIfMouseover(interactableHeaderRect);
				if (interactableHeaderRect.Contains(Event.current.mousePosition))
				{
					string headerTip = this.GetHeaderTip(table);
					if (!headerTip.NullOrEmpty())
					{
						TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
					}
				}
				if (Widgets.ButtonInvisible(interactableHeaderRect, false))
				{
					this.HeaderClicked(rect, table);
				}
			}
		}

		public abstract void DoCell(Rect rect, Pawn pawn, PawnTable table);

		public virtual int GetMinWidth(PawnTable table)
		{
			if (!this.def.label.NullOrEmpty())
			{
				Text.Font = this.DefaultHeaderFont;
				int result = Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap).x);
				Text.Font = GameFont.Small;
				return result;
			}
			if (this.def.HeaderIcon != null)
			{
				return Mathf.CeilToInt(this.def.HeaderIconSize.x);
			}
			return 1;
		}

		public virtual int GetMaxWidth(PawnTable table)
		{
			return 1000000;
		}

		public virtual int GetOptimalWidth(PawnTable table)
		{
			return this.GetMinWidth(table);
		}

		public virtual int GetMinCellHeight(Pawn pawn)
		{
			return 30;
		}

		public virtual int GetMinHeaderHeight(PawnTable table)
		{
			if (!this.def.label.NullOrEmpty())
			{
				Text.Font = this.DefaultHeaderFont;
				int result = Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap).y);
				Text.Font = GameFont.Small;
				return result;
			}
			if (this.def.HeaderIcon != null)
			{
				return Mathf.CeilToInt(this.def.HeaderIconSize.y);
			}
			return 0;
		}

		public virtual int Compare(Pawn a, Pawn b)
		{
			return 0;
		}

		protected virtual Rect GetInteractableHeaderRect(Rect headerRect, PawnTable table)
		{
			float num = Mathf.Min(25f, headerRect.height);
			return new Rect(headerRect.x, headerRect.yMax - num, headerRect.width, num);
		}

		protected virtual void HeaderClicked(Rect headerRect, PawnTable table)
		{
			if (this.def.sortable && !Event.current.shift)
			{
				if (Event.current.button == 0)
				{
					if (table.SortingBy != this.def)
					{
						table.SortBy(this.def, true);
						SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
					}
					else if (table.SortingDescending)
					{
						table.SortBy(this.def, false);
						SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
					}
					else
					{
						table.SortBy(null, false);
						SoundDefOf.TickLow.PlayOneShotOnCamera(null);
					}
				}
				else if (Event.current.button == 1)
				{
					if (table.SortingBy != this.def)
					{
						table.SortBy(this.def, false);
						SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
					}
					else if (table.SortingDescending)
					{
						table.SortBy(null, false);
						SoundDefOf.TickLow.PlayOneShotOnCamera(null);
					}
					else
					{
						table.SortBy(this.def, true);
						SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
					}
				}
			}
		}

		protected virtual string GetHeaderTip(PawnTable table)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!this.def.headerTip.NullOrEmpty())
			{
				stringBuilder.Append(this.def.headerTip);
			}
			if (this.def.sortable)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("ClickToSortByThisColumn".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
