using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_DeepDrillInfestation : StorytellerComp
	{
		private static List<Thing> tmpDrills = new List<Thing>();

		protected StorytellerCompProperties_DeepDrillInfestation Props
		{
			get
			{
				return (StorytellerCompProperties_DeepDrillInfestation)this.props;
			}
		}

		private float DeepDrillInfestationMTBDaysPerDrill
		{
			get
			{
				DifficultyDef difficulty = Find.Storyteller.difficulty;
				if (difficulty.deepDrillInfestationChanceFactor <= 0f)
				{
					return -1f;
				}
				return this.Props.baseMtbDaysPerDrill / difficulty.deepDrillInfestationChanceFactor;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			Map map = (Map)target;
			StorytellerComp_DeepDrillInfestation.tmpDrills.Clear();
			DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, StorytellerComp_DeepDrillInfestation.tmpDrills);
			if (StorytellerComp_DeepDrillInfestation.tmpDrills.Any<Thing>())
			{
				float mtb = this.DeepDrillInfestationMTBDaysPerDrill;
				for (int i = 0; i < StorytellerComp_DeepDrillInfestation.tmpDrills.Count; i++)
				{
					IncidentDef def;
					if (Rand.MTBEventOccurs(mtb, 60000f, 1000f) && base.UsableIncidentsInCategory(IncidentCategoryDefOf.DeepDrillInfestation, target).TryRandomElement(out def))
					{
						IncidentParms parms = this.GenerateParms(def.category, target);
						yield return new FiringIncident(def, this, parms);
					}
				}
			}
		}
	}
}
