using RimWorld.Planet;
using System;
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
			PodContentsType contentsType = (!podContentsType.HasValue) ? Gen.RandomEnumValue<PodContentsType>(true) : podContentsType.Value;
			Rot4? thingRot = rp.thingRot;
			Rot4 rot = (!thingRot.HasValue) ? Rot4.North : thingRot.Value;
			Building_AncientCryptosleepCasket building_AncientCryptosleepCasket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket, null);
			building_AncientCryptosleepCasket.groupID = groupID;
			this.GeneratePodContents(building_AncientCryptosleepCasket, contentsType);
			GenSpawn.Spawn(building_AncientCryptosleepCasket, rp.rect.RandomCell, BaseGen.globalSettings.map, rot);
		}

		private void GeneratePodContents(Building_CryptosleepCasket casket, PodContentsType contentsType)
		{
			switch (contentsType)
			{
			case PodContentsType.SpacerFriendly:
				this.GenerateFriendlySpacer(casket);
				break;
			case PodContentsType.SpacerIncapped:
				this.GenerateIncappedSpacer(casket);
				break;
			case PodContentsType.SpacerHalfEaten:
				this.GenerateHalfEatenSpacer(casket);
				break;
			case PodContentsType.SpacerHostile:
				this.GenerateAngrySpacer(casket);
				break;
			case PodContentsType.Slave:
				this.GenerateSlave(casket);
				break;
			}
		}

		private void GenerateFriendlySpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			this.GiveRandomLootInventoryForTombPawn(pawn);
		}

		private void GenerateIncappedSpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			HealthUtility.GiveInjuriesToForceDowned(pawn);
			this.GiveRandomLootInventoryForTombPawn(pawn);
		}

		private void GenerateSlave(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Slave, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			HealthUtility.GiveInjuriesToForceDowned(pawn);
			this.GiveRandomLootInventoryForTombPawn(pawn);
			if (Rand.Value < 0.5f)
			{
				HealthUtility.GiveInjuriesToKill(pawn);
			}
		}

		private void GenerateAngrySpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.SpacerHostile);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			this.GiveRandomLootInventoryForTombPawn(pawn);
		}

		private void GenerateHalfEatenSpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			int num = Rand.Range(6, 10);
			for (int i = 0; i < num; i++)
			{
				Thing arg_64_0 = pawn;
				Pawn instigator = pawn;
				arg_64_0.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(3, 8), -1f, instigator, null, null));
			}
			this.GiveRandomLootInventoryForTombPawn(pawn);
			int num2 = Rand.Range(3, 6);
			for (int j = 0; j < num2; j++)
			{
				Pawn pawn2 = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, null);
				if (!pod.TryAcceptThing(pawn2, false))
				{
					Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
					return;
				}
				pawn2.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, null);
			}
		}

		private void GiveRandomLootInventoryForTombPawn(Pawn p)
		{
			if ((double)Rand.Value < 0.65)
			{
				this.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Gold, Rand.Range(10, 50));
			}
			else
			{
				this.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Plasteel, Rand.Range(10, 50));
			}
			this.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Component, Rand.Range(-2, 4));
		}

		private void MakeIntoContainer(ThingContainer container, ThingDef def, int count)
		{
			if (count <= 0)
			{
				return;
			}
			Thing thing = ThingMaker.MakeThing(def, null);
			thing.stackCount = count;
			container.TryAdd(thing, true);
		}
	}
}
