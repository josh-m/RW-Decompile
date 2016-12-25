using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_SetNeedLevel : ScenPart_PawnModifier
	{
		private NeedDef need;

		private FloatRange levelRange;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f + 31f);
			if (Widgets.ButtonText(scenPartRect.TopPartPixels(ScenPart.RowHeight), this.need.LabelCap, true, false, true))
			{
				FloatMenuUtility.MakeMenu<NeedDef>(this.PossibleNeeds(), (NeedDef hd) => hd.LabelCap, (NeedDef n) => delegate
				{
					this.need = n;
				});
			}
			Widgets.FloatRange(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f), listing.CurHeight.GetHashCode(), ref this.levelRange, 0f, 1f, "ConfigurableLevel", ToStringStyle.FloatTwo);
			base.DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(ScenPart.RowHeight * 2f));
		}

		private IEnumerable<NeedDef> PossibleNeeds()
		{
			return from x in DefDatabase<NeedDef>.AllDefsListForReading
			where x.major
			select x;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<NeedDef>(ref this.need, "need");
			Scribe_Values.LookValue<FloatRange>(ref this.levelRange, "levelRange", default(FloatRange), false);
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_SetNeed".Translate(new object[]
			{
				this.context.ToStringHuman(),
				this.chance.ToStringPercent(),
				this.need.label,
				this.levelRange.min.ToStringPercent(),
				this.levelRange.max.ToStringPercent()
			}).CapitalizeFirst();
		}

		public override void Randomize()
		{
			base.Randomize();
			this.need = this.PossibleNeeds().RandomElement<NeedDef>();
			this.levelRange.max = Rand.Range(0f, 1f);
			this.levelRange.min = this.levelRange.max * Rand.Range(0f, 0.95f);
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_SetNeedLevel scenPart_SetNeedLevel = other as ScenPart_SetNeedLevel;
			if (scenPart_SetNeedLevel != null && this.need == scenPart_SetNeedLevel.need)
			{
				this.chance = GenMath.ChanceEitherHappens(this.chance, scenPart_SetNeedLevel.chance);
				return true;
			}
			return false;
		}

		protected override void ModifyPawn(Pawn p)
		{
			if (Rand.Value < this.chance && p.needs != null)
			{
				Need need = p.needs.TryGetNeed(this.need);
				if (need != null)
				{
					need.ForceSetLevel(this.levelRange.RandomInRange);
				}
			}
		}
	}
}
