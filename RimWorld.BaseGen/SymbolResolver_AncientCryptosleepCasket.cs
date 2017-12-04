using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientCryptosleepCasket : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			int? ancientCryptosleepCasketGroupID = rp.ancientCryptosleepCasketGroupID;
			int groupID = (!ancientCryptosleepCasketGroupID.HasValue) ? Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID() : ancientCryptosleepCasketGroupID.Value;
			PodContentsType? podContentsType = rp.podContentsType;
			PodContentsType value = (!podContentsType.HasValue) ? Gen.RandomEnumValue<PodContentsType>(true) : podContentsType.Value;
			Rot4? thingRot = rp.thingRot;
			Rot4 rot = (!thingRot.HasValue) ? Rot4.North : thingRot.Value;
			Building_AncientCryptosleepCasket building_AncientCryptosleepCasket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket, null);
			building_AncientCryptosleepCasket.groupID = groupID;
			ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
			parms.podContentsType = new PodContentsType?(value);
			List<Thing> list = ItemCollectionGeneratorDefOf.AncientPodContents.Worker.Generate(parms);
			for (int i = 0; i < list.Count; i++)
			{
				if (!building_AncientCryptosleepCasket.TryAcceptThing(list[i], false))
				{
					Pawn pawn = list[i] as Pawn;
					if (pawn != null)
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
					else
					{
						list[i].Destroy(DestroyMode.Vanish);
					}
				}
			}
			GenSpawn.Spawn(building_AncientCryptosleepCasket, rp.rect.RandomCell, BaseGen.globalSettings.map, rot, false);
		}
	}
}
