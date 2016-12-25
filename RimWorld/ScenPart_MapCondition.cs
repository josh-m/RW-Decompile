using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_MapCondition : ScenPart
	{
		private float durationDays;

		private string durationDaysBuf;

		public override string Label
		{
			get
			{
				return this.def.mapCondition.LabelCap;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.durationDays, "durationDayS", 0f, false);
		}

		public override string Summary(Scenario scen)
		{
			return string.Concat(new string[]
			{
				this.def.mapCondition.LabelCap,
				": ",
				this.def.mapCondition.description,
				" (",
				((int)(this.durationDays * 60000f)).ToStringTicksToDays(),
				")"
			});
		}

		public override void Randomize()
		{
			this.durationDays = Mathf.Round(this.def.durationRandomRange.RandomInRange);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			Widgets.TextFieldNumericLabeled<float>(scenPartRect, "durationDays".Translate(), ref this.durationDays, ref this.durationDaysBuf, 0f, 1E+09f);
		}

		public override void GenerateIntoMap(Map map)
		{
			MapCondition cond = MapConditionMaker.MakeCondition(this.def.mapCondition, (int)(this.durationDays * 60000f), 0);
			map.mapConditionManager.RegisterCondition(cond);
		}

		public override bool CanCoexistWith(ScenPart other)
		{
			ScenPart_MapCondition scenPart_MapCondition = other as ScenPart_MapCondition;
			return scenPart_MapCondition == null || scenPart_MapCondition.def.mapCondition.CanCoexistWith(this.def.mapCondition);
		}
	}
}
