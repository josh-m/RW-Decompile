using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_HasBionicBodyPart : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				AddedBodyPartProps addedPartProps = hediffs[i].def.addedPartProps;
				if (addedPartProps != null && addedPartProps.isBionic)
				{
					return true;
				}
			}
			return false;
		}
	}
}
