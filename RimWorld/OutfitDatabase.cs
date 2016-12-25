using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public sealed class OutfitDatabase : IExposable
	{
		private List<Outfit> outfits = new List<Outfit>();

		public List<Outfit> AllOutfits
		{
			get
			{
				return this.outfits;
			}
		}

		public OutfitDatabase()
		{
			this.GenerateStartingOutfits();
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Outfit>(ref this.outfits, "outfits", LookMode.Deep, new object[0]);
		}

		public Outfit DefaultOutfit()
		{
			if (this.outfits.Count == 0)
			{
				this.MakeNewOutfit();
			}
			return this.outfits[0];
		}

		public AcceptanceReport TryDelete(Outfit outfit)
		{
			foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods)
			{
				if (current.outfits != null && current.outfits.CurrentOutfit == outfit)
				{
					return new AcceptanceReport("OutfitInUse".Translate(new object[]
					{
						current
					}));
				}
			}
			this.outfits.Remove(outfit);
			return AcceptanceReport.WasAccepted;
		}

		public Outfit MakeNewOutfit()
		{
			int arg_40_0;
			if (this.outfits.Any<Outfit>())
			{
				arg_40_0 = this.outfits.Max((Outfit o) => o.uniqueId) + 1;
			}
			else
			{
				arg_40_0 = 1;
			}
			int uniqueId = arg_40_0;
			Outfit outfit = new Outfit(uniqueId, "Outfit".Translate() + " " + uniqueId.ToString());
			outfit.filter.SetAllow(ThingCategoryDefOf.Apparel, true);
			this.outfits.Add(outfit);
			return outfit;
		}

		private void GenerateStartingOutfits()
		{
			Outfit outfit = this.MakeNewOutfit();
			outfit.label = "Anything";
			Outfit outfit2 = this.MakeNewOutfit();
			outfit2.label = "Worker";
			outfit2.filter.SetDisallowAll(null, null);
			outfit2.filter.SetAllow(SpecialThingFilterDefOf.AllowNonDeadmansApparel, true);
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.apparel != null && current.apparel.defaultOutfitTags != null && current.apparel.defaultOutfitTags.Contains("Worker"))
				{
					outfit2.filter.SetAllow(current, true);
				}
			}
			Outfit outfit3 = this.MakeNewOutfit();
			outfit3.label = "Soldier";
			outfit3.filter.SetDisallowAll(null, null);
			outfit3.filter.SetAllow(SpecialThingFilterDefOf.AllowNonDeadmansApparel, true);
			foreach (ThingDef current2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (current2.apparel != null && current2.apparel.defaultOutfitTags != null && current2.apparel.defaultOutfitTags.Contains("Soldier"))
				{
					outfit3.filter.SetAllow(current2, true);
				}
			}
			Outfit outfit4 = this.MakeNewOutfit();
			outfit4.label = "Nudist";
			outfit4.filter.SetDisallowAll(null, null);
			outfit4.filter.SetAllow(SpecialThingFilterDefOf.AllowNonDeadmansApparel, true);
			foreach (ThingDef current3 in DefDatabase<ThingDef>.AllDefs)
			{
				if (current3.apparel != null && !current3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !current3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
				{
					outfit4.filter.SetAllow(current3, true);
				}
			}
		}
	}
}
