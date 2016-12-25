using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Facility : CompProperties
	{
		[Unsaved]
		public List<ThingDef> linkableBuildings;

		public List<StatModifier> statOffsets;

		public int maxSimultaneous = 1;

		public bool mustBePlacedAdjacent;

		public bool canLinkToMedBedsOnly;

		public CompProperties_Facility()
		{
			this.compClass = typeof(CompFacility);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			this.linkableBuildings = new List<ThingDef>();
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				CompProperties_AffectedByFacilities compProperties = allDefsListForReading[i].GetCompProperties<CompProperties_AffectedByFacilities>();
				if (compProperties != null && compProperties.linkableFacilities != null)
				{
					for (int j = 0; j < compProperties.linkableFacilities.Count; j++)
					{
						if (compProperties.linkableFacilities[j] == parentDef)
						{
							this.linkableBuildings.Add(allDefsListForReading[i]);
							break;
						}
					}
				}
			}
		}
	}
}
