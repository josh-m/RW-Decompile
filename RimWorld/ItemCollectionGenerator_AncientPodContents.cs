using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_AncientPodContents : ItemCollectionGenerator
	{
		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			PodContentsType? podContentsType = parms.podContentsType;
			PodContentsType podContentsType2 = (!podContentsType.HasValue) ? Gen.RandomEnumValue<PodContentsType>(true) : podContentsType.Value;
			switch (podContentsType2)
			{
			case PodContentsType.Empty:
				break;
			case PodContentsType.SpacerFriendly:
				outThings.Add(this.GenerateFriendlySpacer());
				break;
			case PodContentsType.SpacerIncapped:
				outThings.Add(this.GenerateIncappedSpacer());
				break;
			case PodContentsType.SpacerHalfEaten:
				outThings.Add(this.GenerateHalfEatenSpacer());
				outThings.AddRange(this.GenerateScarabs());
				break;
			case PodContentsType.SpacerHostile:
				outThings.Add(this.GenerateAngrySpacer());
				break;
			case PodContentsType.Slave:
				outThings.Add(this.GenerateSlave());
				break;
			default:
				Log.Error("Pod contents type not handled: " + podContentsType2);
				break;
			}
		}

		private Pawn GenerateFriendlySpacer()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceSoldier, Faction.OfSpacer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, true, false, false, null, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			this.GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private Pawn GenerateIncappedSpacer()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceSoldier, Faction.OfSpacer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, true, false, false, null, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.DamageUntilDowned(pawn);
			this.GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private Pawn GenerateSlave()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Slave, Faction.OfSpacer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, true, false, false, null, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.DamageUntilDowned(pawn);
			this.GiveRandomLootInventoryForTombPawn(pawn);
			if (Rand.Value < 0.5f)
			{
				HealthUtility.DamageUntilDead(pawn);
			}
			return pawn;
		}

		private Pawn GenerateAngrySpacer()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceSoldier, Faction.OfSpacerHostile, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, true, false, false, null, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			this.GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private Pawn GenerateHalfEatenSpacer()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceSoldier, Faction.OfSpacer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, true, false, false, null, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			int num = Rand.Range(6, 10);
			for (int i = 0; i < num; i++)
			{
				Thing arg_94_0 = pawn;
				DamageDef bite = DamageDefOf.Bite;
				int amount = Rand.Range(3, 8);
				Pawn instigator = pawn;
				arg_94_0.TakeDamage(new DamageInfo(bite, amount, -1f, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
			}
			this.GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private List<Thing> GenerateScarabs()
		{
			List<Thing> list = new List<Thing>();
			int num = Rand.Range(3, 6);
			for (int i = 0; i < num; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, null);
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, null);
				list.Add(pawn);
			}
			return list;
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

		private void MakeIntoContainer(ThingOwner container, ThingDef def, int count)
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
