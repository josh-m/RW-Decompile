using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Needs : ITab
	{
		private const float ThoughtHeight = 20f;

		private const float ThoughtSpacing = 4f;

		private const float ThoughtIntervalY = 24f;

		private const float MoodX = 235f;

		private const float MoodNumberWidth = 32f;

		private const float NeedsColumnWidth = 225f;

		private Vector2 thoughtScrollPosition = default(Vector2);

		private List<Need> displayNeeds = new List<Need>();

		private static readonly Color MoodColor = new Color(0.1f, 1f, 0.1f);

		private static readonly Color MoodColorNegative = new Color(0.8f, 0.4f, 0.4f);

		private static readonly Color NoEffectColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);

		private static readonly Vector2 FullSize = new Vector2(580f, 520f);

		private static List<Thought> thoughtGroupsPresent = new List<Thought>();

		public override bool IsVisible
		{
			get
			{
				return base.SelPawn.needs != null && base.SelPawn.needs.AllNeeds.Count > 0;
			}
		}

		public ITab_Pawn_Needs()
		{
			this.labelKey = "TabNeeds";
			this.tutorTag = "Needs";
		}

		public override void OnOpen()
		{
			this.thoughtScrollPosition = default(Vector2);
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, 225f, this.size.y);
			this.DoNeeds(rect);
			if (base.SelPawn.needs.mood != null)
			{
				Rect rect2 = new Rect(rect.xMax, 0f, this.size.x - rect.width, this.size.y);
				this.DoMoodAndThoughts(rect2);
			}
		}

		protected override void UpdateSize()
		{
			this.UpdateDisplayNeeds();
			if (base.SelPawn.needs.mood != null)
			{
				this.size = ITab_Pawn_Needs.FullSize;
			}
			else
			{
				this.size = new Vector2(225f, (float)this.displayNeeds.Count * Mathf.Min(70f, ITab_Pawn_Needs.FullSize.y / (float)this.displayNeeds.Count));
			}
		}

		private void UpdateDisplayNeeds()
		{
			this.displayNeeds.Clear();
			List<Need> allNeeds = base.SelPawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				if (allNeeds[i].def.showOnNeedList)
				{
					this.displayNeeds.Add(allNeeds[i]);
				}
			}
			this.displayNeeds.Sort((Need a, Need b) => b.def.listPriority.CompareTo(a.def.listPriority));
		}

		private void DoNeeds(Rect rect)
		{
			this.UpdateDisplayNeeds();
			float num = 0f;
			for (int i = 0; i < this.displayNeeds.Count; i++)
			{
				Need need = this.displayNeeds[i];
				Rect rect2 = new Rect(rect.x, rect.y + num, rect.width, Mathf.Min(70f, rect.height / (float)this.displayNeeds.Count));
				if (!need.def.major)
				{
					if (i > 0 && this.displayNeeds[i - 1].def.major)
					{
						rect2.y += 10f;
					}
					rect2.width *= 0.73f;
					rect2.height = Mathf.Max(rect2.height * 0.666f, 30f);
				}
				need.DrawOnGUI(rect2);
				num = rect2.yMax;
			}
		}

		protected void DoMoodAndThoughts(Rect rect)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, rect.width * 0.8f, 70f);
			base.SelPawn.needs.mood.DrawOnGUI(rect2);
			Rect rect3 = new Rect(0f, 80f, rect.width, rect.height - 70f - 10f);
			rect3 = rect3.ContractedBy(10f);
			this.DrawThoughtListing(rect3);
			GUI.EndGroup();
		}

		private void DrawThoughtListing(Rect listingRect)
		{
			Text.Font = GameFont.Small;
			ITab_Pawn_Needs.thoughtGroupsPresent.Clear();
			ITab_Pawn_Needs.thoughtGroupsPresent.AddRange(base.SelPawn.needs.mood.thoughts.DistinctThoughtGroups());
			ITab_Pawn_Needs.thoughtGroupsPresent.SortByDescending((Thought th) => base.SelPawn.needs.mood.thoughts.MoodOffsetOfThoughtGroup(th));
			float height = (float)ITab_Pawn_Needs.thoughtGroupsPresent.Count * 24f;
			Widgets.BeginScrollView(listingRect, ref this.thoughtScrollPosition, new Rect(0f, 0f, listingRect.width - 16f, height));
			Text.Anchor = TextAnchor.MiddleLeft;
			float num = 0f;
			for (int i = 0; i < ITab_Pawn_Needs.thoughtGroupsPresent.Count; i++)
			{
				Rect rect = new Rect(0f, num, listingRect.width - 16f, 20f);
				if (this.DrawThoughtGroup(rect, ITab_Pawn_Needs.thoughtGroupsPresent[i]))
				{
					num += 24f;
				}
			}
			Widgets.EndScrollView();
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private bool DrawThoughtGroup(Rect rect, Thought group)
		{
			try
			{
				List<Thought> list = base.SelPawn.needs.mood.thoughts.ThoughtsInGroup(group).ToList<Thought>();
				int index = 0;
				int num = -1;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].CurStageIndex > num)
					{
						num = list[i].CurStageIndex;
						index = i;
					}
				}
				if (!list[index].VisibleInNeedsTab)
				{
					return false;
				}
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				if (group.def.DurationTicks > 5)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(list[index].Description);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					Thought_Memory thought_Memory = list[index] as Thought_Memory;
					if (thought_Memory != null)
					{
						if (list.Count == 1)
						{
							stringBuilder.Append("ThoughtExpiresIn".Translate(new object[]
							{
								(group.def.DurationTicks - thought_Memory.age).ToStringTicksToPeriod(true)
							}));
						}
						else
						{
							Thought_Memory thought_Memory2 = (Thought_Memory)list[list.Count - 1];
							stringBuilder.Append("ThoughtStartsExpiringIn".Translate(new object[]
							{
								(group.def.DurationTicks - thought_Memory.age).ToStringTicksToPeriod(true)
							}));
							stringBuilder.AppendLine();
							stringBuilder.Append("ThoughtFinishesExpiringIn".Translate(new object[]
							{
								(group.def.DurationTicks - thought_Memory2.age).ToStringTicksToPeriod(true)
							}));
						}
					}
					TooltipHandler.TipRegion(rect, new TipSignal(stringBuilder.ToString(), 7291));
				}
				else
				{
					TooltipHandler.TipRegion(rect, new TipSignal(list[index].Description, 7141));
				}
				Text.WordWrap = false;
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect2 = new Rect(rect.x + 10f, rect.y, 225f, rect.height);
				rect2.yMin -= 3f;
				rect2.yMax += 3f;
				string text = list[index].LabelCap;
				if (list.Count > 1)
				{
					text = text + " x" + list.Count;
				}
				Widgets.Label(rect2, text);
				Text.Anchor = TextAnchor.MiddleCenter;
				float num2 = base.SelPawn.needs.mood.thoughts.MoodOffsetOfThoughtGroup(group);
				if (num2 == 0f)
				{
					GUI.color = ITab_Pawn_Needs.NoEffectColor;
				}
				else if (num2 > 0f)
				{
					GUI.color = ITab_Pawn_Needs.MoodColor;
				}
				else
				{
					GUI.color = ITab_Pawn_Needs.MoodColorNegative;
				}
				Rect rect3 = new Rect(rect.x + 235f, rect.y, 32f, rect.height);
				Widgets.Label(rect3, num2.ToString("##0"));
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				Text.WordWrap = true;
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(string.Concat(new object[]
				{
					"Exception in DrawThoughtGroup for ",
					group.def,
					" on ",
					base.SelPawn,
					": ",
					ex.ToString()
				}), 3452698);
			}
			return true;
		}
	}
}
