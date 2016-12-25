using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_ForcedTrait : ScenPart_PawnModifier
	{
		private TraitDef trait;

		private int degree;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<TraitDef>(ref this.trait, "trait");
			Scribe_Values.LookValue<int>(ref this.degree, "degree", 0, false);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
			if (Widgets.ButtonText(scenPartRect.TopPart(0.333f), this.trait.DataAtDegree(this.degree).label.CapitalizeFirst(), true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (TraitDef current in from td in DefDatabase<TraitDef>.AllDefs
				orderby td.label
				select td)
				{
					foreach (TraitDegreeData current2 in current.degreeDatas)
					{
						TraitDef localDef = current;
						TraitDegreeData localDeg = current2;
						list.Add(new FloatMenuOption(localDeg.label.CapitalizeFirst(), delegate
						{
							this.trait = localDef;
							this.degree = localDeg.degree;
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			base.DoPawnModifierEditInterface(scenPartRect.BottomPart(0.666f));
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_PawnsHaveTrait".Translate(new object[]
			{
				this.context.ToStringHuman(),
				this.chance.ToStringPercent(),
				this.trait.DataAtDegree(this.degree).label.CapitalizeFirst()
			}).CapitalizeFirst();
		}

		public override void Randomize()
		{
			base.Randomize();
			this.trait = DefDatabase<TraitDef>.GetRandom();
			this.degree = this.trait.degreeDatas.RandomElement<TraitDegreeData>().degree;
		}

		public override bool CanCoexistWith(ScenPart other)
		{
			ScenPart_ForcedTrait scenPart_ForcedTrait = other as ScenPart_ForcedTrait;
			return scenPart_ForcedTrait == null || this.trait != scenPart_ForcedTrait.trait;
		}

		protected override void ModifyPawn(Pawn pawn)
		{
			if (Rand.Value > this.chance)
			{
				return;
			}
			if (pawn.story == null || pawn.story.traits == null)
			{
				return;
			}
			if (pawn.story.traits.HasTrait(this.trait) && pawn.story.traits.DegreeOfTrait(this.trait) == this.degree)
			{
				return;
			}
			if (pawn.story.traits.HasTrait(this.trait))
			{
				pawn.story.traits.allTraits.RemoveAll((Trait tr) => tr.def == this.trait);
			}
			else
			{
				IEnumerable<Trait> source = from tr in pawn.story.traits.allTraits
				where !tr.ScenForced && !ScenPart_ForcedTrait.PawnHasTraitForcedByBackstory(pawn, tr.def)
				select tr;
				if (source.Any<Trait>())
				{
					Trait trait = (from tr in source
					where tr.def.conflictingTraits.Contains(this.trait)
					select tr).FirstOrDefault<Trait>();
					if (trait != null)
					{
						pawn.story.traits.allTraits.Remove(trait);
					}
					else
					{
						pawn.story.traits.allTraits.Remove(source.RandomElement<Trait>());
					}
				}
			}
			pawn.story.traits.GainTrait(new Trait(this.trait, this.degree, true));
		}

		private static bool PawnHasTraitForcedByBackstory(Pawn pawn, TraitDef trait)
		{
			return (pawn.story.childhood != null && pawn.story.childhood.forcedTraits != null && pawn.story.childhood.forcedTraits.Any((TraitEntry te) => te.def == trait)) || (pawn.story.adulthood != null && pawn.story.adulthood.forcedTraits != null && pawn.story.adulthood.forcedTraits.Any((TraitEntry te) => te.def == trait));
		}
	}
}
