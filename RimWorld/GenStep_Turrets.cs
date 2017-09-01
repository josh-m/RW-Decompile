using RimWorld.BaseGen;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_Turrets : GenStep
	{
		private const int Padding = 7;

		public IntRange widthRange = new IntRange(3, 4);

		public IntRange turretsCountRange = new IntRange(4, 5);

		public IntRange mortarsCountRange = new IntRange(0, 1);

		public IntRange guardsCountRange = IntRange.one;

		public override void Generate(Map map)
		{
			CellRect cellRect;
			if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out cellRect))
			{
				cellRect = this.FindRandomRectToDefend(map);
			}
			Faction faction;
			if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
			{
				faction = (from x in Find.FactionManager.AllFactions
				where !x.defeated && x.HostileTo(Faction.OfPlayer) && !x.def.hidden && x.def.techLevel >= TechLevel.Industrial
				select x).RandomElementWithFallback(Find.FactionManager.RandomEnemyFaction(false, false, true));
			}
			else
			{
				faction = map.ParentFaction;
			}
			int randomInRange = this.widthRange.RandomInRange;
			CellRect rect = cellRect.ExpandedBy(7 + randomInRange).ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.faction = faction;
			resolveParams.edgeDefenseWidth = new int?(randomInRange);
			resolveParams.edgeDefenseTurretsCount = new int?(this.turretsCountRange.RandomInRange);
			resolveParams.edgeDefenseMortarsCount = new int?(this.mortarsCountRange.RandomInRange);
			resolveParams.edgeDefenseGuardsCount = new int?(this.guardsCountRange.RandomInRange);
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("edgeDefense", resolveParams);
			BaseGen.Generate();
		}

		private CellRect FindRandomRectToDefend(Map map)
		{
			int rectRadius = Mathf.Max(Mathf.RoundToInt((float)Mathf.Min(map.Size.x, map.Size.z) * 0.07f), 1);
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			IntVec3 center;
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(delegate(IntVec3 x)
			{
				if (!map.reachability.CanReachMapEdge(x, traverseParams))
				{
					return false;
				}
				CellRect cellRect = CellRect.CenteredOn(x, rectRadius);
				int num = 0;
				CellRect.CellRectIterator iterator = cellRect.GetIterator();
				while (!iterator.Done())
				{
					if (!iterator.Current.InBounds(map))
					{
						return false;
					}
					if (iterator.Current.Standable(map) || iterator.Current.GetPlant(map) != null)
					{
						num++;
					}
					iterator.MoveNext();
				}
				return (float)num / (float)cellRect.Area >= 0.6f;
			}, map, out center))
			{
				return CellRect.CenteredOn(center, rectRadius);
			}
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(map), map, out center))
			{
				return CellRect.CenteredOn(center, rectRadius);
			}
			return CellRect.CenteredOn(CellFinder.RandomCell(map), rectRadius).ClipInsideMap(map);
		}
	}
}
