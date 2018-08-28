using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestItemStashAICore : IncidentWorker_QuestItemStash
	{
		protected override List<Thing> GenerateItems(Faction siteFaction, float siteThreatPoints)
		{
			return new List<Thing>
			{
				ThingMaker.MakeThing(ThingDefOf.AIPersonaCore, null)
			};
		}
	}
}
