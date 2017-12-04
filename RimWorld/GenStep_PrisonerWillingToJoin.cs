using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class GenStep_PrisonerWillingToJoin : GenStep_Scatterer
	{
		private const int Size = 8;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			if (!c.SupportsStructureType(map, TerrainAffordance.Heavy))
			{
				return false;
			}
			if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
			{
				return false;
			}
			CellRect.CellRectIterator iterator = CellRect.CenteredOn(c, 8, 8).GetIterator();
			while (!iterator.Done())
			{
				if (!iterator.Current.InBounds(map) || iterator.Current.GetEdifice(map) != null)
				{
					return false;
				}
				iterator.MoveNext();
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, int count = 1)
		{
			Faction faction;
			if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
			{
				faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
			}
			else
			{
				faction = map.ParentFaction;
			}
			CellRect cellRect = CellRect.CenteredOn(loc, 8, 8).ClipInsideMap(map);
			PrisonerWillingToJoinComp component = map.info.parent.GetComponent<PrisonerWillingToJoinComp>();
			Pawn singlePawnToSpawn;
			if (component != null && component.pawn.Any)
			{
				singlePawnToSpawn = component.pawn.Take(component.pawn[0]);
			}
			else
			{
				singlePawnToSpawn = PrisonerWillingToJoinQuestUtility.GeneratePrisoner(map.Tile, faction);
			}
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = cellRect;
			resolveParams.faction = faction;
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("prisonCell", resolveParams);
			BaseGen.Generate();
			ResolveParams resolveParams2 = default(ResolveParams);
			resolveParams2.rect = cellRect;
			resolveParams2.faction = faction;
			resolveParams2.singlePawnToSpawn = singlePawnToSpawn;
			resolveParams2.postThingSpawn = delegate(Thing x)
			{
				MapGenerator.rootsToUnfog.Add(x.Position);
				((Pawn)x).mindState.willJoinColonyIfRescued = true;
			};
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("pawn", resolveParams2);
			BaseGen.Generate();
			MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);
		}
	}
}
