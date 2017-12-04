using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_CaravanDemand : IncidentWorker
	{
		private static readonly FloatRange DemandAsPercentageOfCaravan = new FloatRange(0.02f, 0.35f);

		private const float DemandSilverWeight = 5f;

		private const float DemandAnimalWeight = 1f;

		private const float DemandPrisonerWeight = 1f;

		private const float DemandColonistWeight = 0.2f;

		private const float DemandFallbackWeight = 1f;

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			return CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(target.Tile);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Caravan caravan = (Caravan)parms.target;
			if (!PawnGroupMakerUtility.TryGetRandomFactionForNormalPawnGroup(parms.points, out parms.faction, null, false, false, false, true))
			{
				return false;
			}
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms, false);
			defaultPawnGroupMakerParms.generateFightersOnly = true;
			List<Pawn> attackers = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
			List<Thing> demands = this.GenerateDemands(caravan);
			if (demands.Count == 0)
			{
				return false;
			}
			CameraJumper.TryJumpAndSelect(caravan);
			DiaNode diaNode = new DiaNode(this.GenerateMessageText(parms.faction, attackers.Count, demands));
			DiaOption diaOption = new DiaOption("CaravanDemand_Give".Translate());
			diaOption.action = delegate
			{
				this.ActionGive(caravan, demands, attackers);
			};
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			DiaOption diaOption2 = new DiaOption("CaravanDemand_Fight".Translate());
			diaOption2.action = delegate
			{
				this.ActionFight(caravan, attackers);
			};
			diaOption2.resolveTree = true;
			diaNode.options.Add(diaOption2);
			WindowStack arg_15D_0 = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			bool delayInteractivity = true;
			string title = "CaravanDemandTitle".Translate(new object[]
			{
				parms.faction.Name
			});
			arg_15D_0.Add(new Dialog_NodeTree(nodeRoot, delayInteractivity, false, title));
			return true;
		}

		private List<Thing> GenerateDemands(Caravan caravan)
		{
			List<Thing> list = new List<Thing>();
			List<Thing> list2 = new List<Thing>();
			list2.AddRange(caravan.PawnsListForReading.Cast<Thing>());
			list2.AddRange(caravan.PawnsListForReading.SelectMany((Pawn pawn) => ThingOwnerUtility.GetAllThingsRecursively(pawn, false)));
			float num = list2.Sum((Thing thing) => thing.MarketValue);
			float num2 = IncidentWorker_CaravanDemand.DemandAsPercentageOfCaravan.RandomInRange * num;
			while (num2 > 0f)
			{
				if (list2.Count == 0)
				{
					break;
				}
				if (list2.Count((Thing thing) => thing is Pawn && ((Pawn)thing).IsColonist) == 1)
				{
					list2.RemoveAll((Thing thing) => thing is Pawn && ((Pawn)thing).IsColonist);
				}
				Thing thing2 = list2.RandomElementByWeight(delegate(Thing thing)
				{
					if (thing.def == ThingDefOf.Silver)
					{
						return 5f;
					}
					if (thing is Pawn)
					{
						Pawn pawn = (Pawn)thing;
						if (pawn.RaceProps.Animal)
						{
							return 1f;
						}
						if (pawn.IsPrisoner)
						{
							return 1f;
						}
						if (pawn.IsColonist)
						{
							return 0.2f;
						}
					}
					return 1f;
				});
				num2 -= thing2.MarketValue;
				list.Add(thing2);
				list2.Remove(thing2);
			}
			return list;
		}

		private string GenerateMessageText(Faction enemyFaction, int attackerCount, List<Thing> demands)
		{
			return "CaravanDemand".Translate(new object[]
			{
				enemyFaction.Name,
				attackerCount,
				GenLabel.ThingsLabel(demands),
				enemyFaction.def.pawnsPlural
			});
		}

		private void TakeFromCaravan(Caravan caravan, List<Thing> demands, Faction enemyFaction)
		{
			List<Thing> list = new List<Thing>();
			for (int i = 0; i < demands.Count; i++)
			{
				Thing thing = demands[i];
				if (thing is Pawn)
				{
					Pawn pawn = (Pawn)thing;
					caravan.RemovePawn(pawn);
					foreach (Thing current in ThingOwnerUtility.GetAllThingsRecursively(pawn, false))
					{
						list.Add(current);
						current.holdingOwner.Take(current);
					}
					enemyFaction.kidnapped.KidnapPawn(pawn, null);
				}
				else
				{
					if (thing.holdingOwner != null)
					{
						thing.holdingOwner.Take(thing);
					}
					thing.Destroy(DestroyMode.Vanish);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!list[j].Destroyed)
				{
					CaravanInventoryUtility.GiveThing(caravan, list[j]);
				}
			}
		}

		private void ActionGive(Caravan caravan, List<Thing> demands, List<Pawn> attackers)
		{
			this.TakeFromCaravan(caravan, demands, attackers[0].Faction);
			for (int i = 0; i < attackers.Count; i++)
			{
				Find.WorldPawns.PassToWorld(attackers[i], PawnDiscardDecideMode.Discard);
			}
		}

		private void ActionFight(Caravan caravan, List<Pawn> attackers)
		{
			Faction enemyFaction = attackers[0].Faction;
			TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushedByHumanlike, new object[]
			{
				caravan.RandomOwner()
			});
			LongEventHandler.QueueLongEvent(delegate
			{
				Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, attackers);
				LordJob_AssaultColony lordJob_AssaultColony = new LordJob_AssaultColony(enemyFaction, true, false, false, false, true);
				if (lordJob_AssaultColony != null)
				{
					LordMaker.MakeNewLord(enemyFaction, lordJob_AssaultColony, map, attackers);
				}
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				CameraJumper.TryJump(attackers[0]);
			}, "GeneratingMapForNewEncounter", false, null);
		}
	}
}
