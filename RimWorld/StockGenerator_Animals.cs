using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Animals : StockGenerator
	{
		private IntRange kindCountRange = new IntRange(1, 1);

		private float minWildness;

		private float maxWildness = 1f;

		private List<string> tradeTags;

		private bool checkTemperature;

		private static readonly SimpleCurve SelectionChanceFromWildnessCurve = new SimpleCurve
		{
			new CurvePoint(0f, 100f),
			new CurvePoint(0.25f, 60f),
			new CurvePoint(0.5f, 30f),
			new CurvePoint(0.75f, 12f),
			new CurvePoint(1f, 2f)
		};

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings()
		{
			int numKinds = this.kindCountRange.RandomInRange;
			int count = this.countRange.RandomInRange;
			List<PawnKindDef> kinds = new List<PawnKindDef>();
			for (int i = 0; i < numKinds; i++)
			{
				PawnKindDef kind;
				if (!(from k in DefDatabase<PawnKindDef>.AllDefs
				where !this.<kinds>__2.Contains(k) && this.<>f__this.PawnKindAllowed(k)
				select k).TryRandomElementByWeight((PawnKindDef k) => this.<>f__this.SelectionChance(k), out kind))
				{
					break;
				}
				kinds.Add(kind);
			}
			for (int j = 0; j < count; j++)
			{
				PawnKindDef kind2;
				if (!kinds.TryRandomElement(out kind2))
				{
					break;
				}
				yield return PawnGenerator.GeneratePawn(kind2, null);
			}
		}

		private float SelectionChance(PawnKindDef k)
		{
			return StockGenerator_Animals.SelectionChanceFromWildnessCurve.Evaluate(k.RaceProps.wildness);
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Animal;
		}

		private bool PawnKindAllowed(PawnKindDef kind)
		{
			return kind.RaceProps.Animal && kind.RaceProps.wildness >= this.minWildness && kind.RaceProps.wildness <= this.maxWildness && kind.RaceProps.wildness <= 1f && (!this.checkTemperature || GenTemperature.SeasonAndOutdoorTemperatureAcceptableFor(kind.race)) && kind.race.tradeTags != null && this.tradeTags.Find((string x) => kind.race.tradeTags.Contains(x)) != null;
		}

		public void LogAnimalChances()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PawnKindDef current in DefDatabase<PawnKindDef>.AllDefs)
			{
				stringBuilder.AppendLine(current.defName + ": " + this.SelectionChance(current).ToString("F2"));
			}
			Log.Message(stringBuilder.ToString());
		}

		internal static void LogStockGeneration()
		{
			new StockGenerator_Animals
			{
				tradeTags = new List<string>(),
				tradeTags = 
				{
					"StandardAnimal",
					"BadassAnimal"
				}
			}.LogAnimalChances();
		}
	}
}
