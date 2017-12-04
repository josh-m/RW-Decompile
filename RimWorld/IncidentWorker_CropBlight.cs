using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_CropBlight : IncidentWorker
	{
		private const float Radius = 16f;

		private const float BaseBlightChance = 0.1f;

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Plant plant;
			return this.TryFindRandomBlightablePlant((Map)target, out plant);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Plant plant;
			if (!this.TryFindRandomBlightablePlant(map, out plant))
			{
				return false;
			}
			Room room = plant.GetRoom(RegionType.Set_Passable);
			plant.CropBlighted();
			int i = 0;
			int num = GenRadial.NumCellsInRadius(16f);
			while (i < num)
			{
				IntVec3 intVec = plant.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
				{
					Plant firstBlightableNowPlant = BlightUtility.GetFirstBlightableNowPlant(intVec, map);
					if (firstBlightableNowPlant != null && firstBlightableNowPlant != plant)
					{
						if (Rand.Chance(0.1f * this.BlightChanceFactor(firstBlightableNowPlant.Position, plant.Position)))
						{
							firstBlightableNowPlant.CropBlighted();
						}
					}
				}
				i++;
			}
			Find.LetterStack.ReceiveLetter("LetterLabelCropBlight".Translate(), "LetterCropBlight".Translate(), LetterDefOf.NegativeEvent, new TargetInfo(plant.Position, map, false), null);
			return true;
		}

		private bool TryFindRandomBlightablePlant(Map map, out Plant plant)
		{
			Thing thing;
			bool result = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
			where ((Plant)x).BlightableNow
			select x).TryRandomElement(out thing);
			plant = (Plant)thing;
			return result;
		}

		private float BlightChanceFactor(IntVec3 c, IntVec3 root)
		{
			return Mathf.InverseLerp(16f, 8f, c.DistanceTo(root));
		}
	}
}
