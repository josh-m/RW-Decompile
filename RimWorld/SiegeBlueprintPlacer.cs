using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SiegeBlueprintPlacer
	{
		private const int MaxArtyCount = 2;

		public const float ArtyCost = 60f;

		private const int MinSandbagDistSquared = 36;

		private static IntVec3 center;

		private static Faction faction;

		private static List<IntVec3> placedSandbagLocs = new List<IntVec3>();

		private static readonly IntRange NumSandbagRange = new IntRange(2, 4);

		private static readonly IntRange SandbagLengthRange = new IntRange(2, 7);

		[DebuggerHidden]
		public static IEnumerable<Blueprint_Build> PlaceBlueprints(IntVec3 placeCenter, Faction placeFaction, float points)
		{
			SiegeBlueprintPlacer.center = placeCenter;
			SiegeBlueprintPlacer.faction = placeFaction;
			foreach (Blueprint_Build blue in SiegeBlueprintPlacer.PlaceSandbagBlueprints())
			{
				yield return blue;
			}
			foreach (Blueprint_Build blue2 in SiegeBlueprintPlacer.PlaceArtilleryBlueprints(points))
			{
				yield return blue2;
			}
		}

		private static bool CanPlaceBlueprintAt(IntVec3 root, Rot4 rot, ThingDef buildingDef)
		{
			return GenConstruct.CanPlaceBlueprintAt(buildingDef, root, rot, false, null).Accepted;
		}

		[DebuggerHidden]
		private static IEnumerable<Blueprint_Build> PlaceSandbagBlueprints()
		{
			SiegeBlueprintPlacer.placedSandbagLocs.Clear();
			int numSandbags = SiegeBlueprintPlacer.NumSandbagRange.RandomInRange;
			for (int i = 0; i < numSandbags; i++)
			{
				IntVec3 bagRoot = SiegeBlueprintPlacer.FindSandbagRoot();
				if (!bagRoot.IsValid)
				{
					break;
				}
				Rot4 growDirA;
				if (bagRoot.x > SiegeBlueprintPlacer.center.x)
				{
					growDirA = Rot4.West;
				}
				else
				{
					growDirA = Rot4.East;
				}
				Rot4 growDirB;
				if (bagRoot.z > SiegeBlueprintPlacer.center.z)
				{
					growDirB = Rot4.South;
				}
				else
				{
					growDirB = Rot4.North;
				}
				foreach (Blueprint_Build bag in SiegeBlueprintPlacer.MakeSandbagLine(bagRoot, growDirA, SiegeBlueprintPlacer.SandbagLengthRange.RandomInRange))
				{
					yield return bag;
				}
				bagRoot += growDirB.FacingCell;
				foreach (Blueprint_Build bag2 in SiegeBlueprintPlacer.MakeSandbagLine(bagRoot, growDirB, SiegeBlueprintPlacer.SandbagLengthRange.RandomInRange))
				{
					yield return bag2;
				}
			}
		}

		[DebuggerHidden]
		private static IEnumerable<Blueprint_Build> MakeSandbagLine(IntVec3 root, Rot4 growDir, int maxLength)
		{
			IntVec3 cur = root;
			for (int i = 0; i < maxLength; i++)
			{
				if (!SiegeBlueprintPlacer.CanPlaceBlueprintAt(cur, Rot4.North, ThingDefOf.Sandbags))
				{
					break;
				}
				yield return GenConstruct.PlaceBlueprintForBuild(ThingDefOf.Sandbags, cur, Rot4.North, SiegeBlueprintPlacer.faction, null);
				SiegeBlueprintPlacer.placedSandbagLocs.Add(cur);
				cur += growDir.FacingCell;
			}
		}

		[DebuggerHidden]
		private static IEnumerable<Blueprint_Build> PlaceArtilleryBlueprints(float points)
		{
			IEnumerable<ThingDef> artyDefs = from def in DefDatabase<ThingDef>.AllDefs
			where def.building != null && def.building.buildingTags.Contains("Artillery_BaseDestroyer")
			select def;
			int numArtillery = Mathf.RoundToInt(points / 60f);
			numArtillery = Mathf.Clamp(numArtillery, 1, 2);
			for (int i = 0; i < numArtillery; i++)
			{
				Rot4 rot = Rot4.Random;
				ThingDef artyDef = artyDefs.RandomElement<ThingDef>();
				IntVec3 artySpot = SiegeBlueprintPlacer.FindArtySpot(artyDef, rot);
				if (!artySpot.IsValid)
				{
					break;
				}
				yield return GenConstruct.PlaceBlueprintForBuild(artyDef, artySpot, rot, SiegeBlueprintPlacer.faction, ThingDefOf.Steel);
				points -= 60f;
			}
		}

		private static IntVec3 FindSandbagRoot()
		{
			CellRect cellRect = CellRect.CenteredOn(SiegeBlueprintPlacer.center, 13);
			cellRect.ClipInsideMap();
			CellRect cellRect2 = CellRect.CenteredOn(SiegeBlueprintPlacer.center, 8);
			cellRect2.ClipInsideMap();
			int num = 0;
			while (true)
			{
				num++;
				if (num > 200)
				{
					break;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (!cellRect2.Contains(randomCell))
				{
					if (randomCell.CanReach(SiegeBlueprintPlacer.center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly))
					{
						if (SiegeBlueprintPlacer.CanPlaceBlueprintAt(randomCell, Rot4.North, ThingDefOf.Sandbags))
						{
							bool flag = false;
							for (int i = 0; i < SiegeBlueprintPlacer.placedSandbagLocs.Count; i++)
							{
								float lengthHorizontalSquared = (SiegeBlueprintPlacer.placedSandbagLocs[i] - randomCell).LengthHorizontalSquared;
								if (lengthHorizontalSquared < 36f)
								{
									flag = true;
								}
							}
							if (!flag)
							{
								return randomCell;
							}
						}
					}
				}
			}
			return IntVec3.Invalid;
		}

		private static IntVec3 FindArtySpot(ThingDef artyDef, Rot4 rot)
		{
			CellRect cellRect = CellRect.CenteredOn(SiegeBlueprintPlacer.center, 8);
			cellRect.ClipInsideMap();
			int num = 0;
			while (true)
			{
				num++;
				if (num > 200)
				{
					break;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (randomCell.CanReach(SiegeBlueprintPlacer.center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly))
				{
					if (!randomCell.Roofed())
					{
						if (SiegeBlueprintPlacer.CanPlaceBlueprintAt(randomCell, rot, artyDef))
						{
							return randomCell;
						}
					}
				}
			}
			return IntVec3.Invalid;
		}
	}
}
