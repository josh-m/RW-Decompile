using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ApparelUtility
	{
		public static bool CanWearTogether(ThingDef A, ThingDef B)
		{
			bool flag = false;
			for (int i = 0; i < A.apparel.layers.Count; i++)
			{
				for (int j = 0; j < B.apparel.layers.Count; j++)
				{
					if (A.apparel.layers[i] == B.apparel.layers[j])
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
			for (int k = 0; k < A.apparel.bodyPartGroups.Count; k++)
			{
				for (int l = 0; l < B.apparel.bodyPartGroups.Count; l++)
				{
					BodyPartGroupDef item = A.apparel.bodyPartGroups[k];
					BodyPartGroupDef item2 = B.apparel.bodyPartGroups[l];
					for (int m = 0; m < BodyDefOf.Human.AllParts.Count; m++)
					{
						BodyPartRecord bodyPartRecord = BodyDefOf.Human.AllParts[m];
						if (bodyPartRecord.groups.Contains(item) && bodyPartRecord.groups.Contains(item2))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public static bool HasPartsToWear(Pawn p, ThingDef apparel)
		{
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			bool flag = false;
			for (int j = 0; j < hediffs.Count; j++)
			{
				if (hediffs[j] is Hediff_MissingPart)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
			IEnumerable<BodyPartRecord> notMissingParts = p.health.hediffSet.GetNotMissingParts(null, null);
			List<BodyPartGroupDef> groups = apparel.apparel.bodyPartGroups;
			int i;
			for (i = 0; i < groups.Count; i++)
			{
				if (notMissingParts.Any((BodyPartRecord x) => x.IsInGroup(groups[i])))
				{
					return true;
				}
			}
			return false;
		}
	}
}
