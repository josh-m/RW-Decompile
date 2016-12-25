using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RansomDemand : IncidentWorker
	{
		private static List<Pawn> candidates = new List<Pawn>();

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			return this.RandomKidnappedColonist() != null && base.CanFireNowSub(target);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Pawn colonist = this.RandomKidnappedColonist();
			if (colonist == null)
			{
				return false;
			}
			Faction faction = this.FactionWhichKidnapped(colonist);
			int fee = this.RandomFee(colonist);
			string text = "RansomDemand".Translate(new object[]
			{
				colonist.LabelShort,
				faction.Name,
				fee
			}).AdjustedFor(colonist);
			DiaNode diaNode = new DiaNode(text);
			DiaOption diaOption = new DiaOption("RansomDemand_Accept".Translate());
			diaOption.action = delegate
			{
				faction.kidnapped.RemoveKidnappedPawn(colonist);
				Find.WorldPawns.RemovePawn(colonist);
				IntVec3 intVec;
				if (faction.def.techLevel < TechLevel.Spacer)
				{
					if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && map.reachability.CanReachColony(c), map, out intVec) && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map), map, out intVec))
					{
						Log.Warning("Could not find any edge cell.");
						intVec = DropCellFinder.TradeDropSpot(map);
					}
					GenSpawn.Spawn(colonist, intVec, map);
				}
				else
				{
					intVec = DropCellFinder.TradeDropSpot(map);
					TradeUtility.SpawnDropPod(intVec, map, colonist);
				}
				Find.CameraDriver.JumpTo(intVec);
				this.LaunchSilver(map, fee);
			};
			diaOption.resolveTree = true;
			if (!this.ColonyHasEnoughSilver(map, fee))
			{
				diaOption.Disable("NotEnoughSilver".Translate());
			}
			diaNode.options.Add(diaOption);
			DiaOption diaOption2 = new DiaOption("RansomDemand_Reject".Translate());
			diaOption2.resolveTree = true;
			diaNode.options.Add(diaOption2);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true));
			return true;
		}

		private Pawn RandomKidnappedColonist()
		{
			IncidentWorker_RansomDemand.candidates.Clear();
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				List<Pawn> kidnappedPawnsListForReading = allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading;
				for (int j = 0; j < kidnappedPawnsListForReading.Count; j++)
				{
					if (kidnappedPawnsListForReading[j].Faction == Faction.OfPlayer)
					{
						IncidentWorker_RansomDemand.candidates.Add(kidnappedPawnsListForReading[j]);
					}
				}
			}
			Pawn result;
			if (!IncidentWorker_RansomDemand.candidates.TryRandomElement(out result))
			{
				return null;
			}
			IncidentWorker_RansomDemand.candidates.Clear();
			return result;
		}

		private Faction FactionWhichKidnapped(Pawn pawn)
		{
			return Find.FactionManager.AllFactionsListForReading.Find((Faction x) => x.kidnapped.KidnappedPawnsListForReading.Contains(pawn));
		}

		private bool ColonyHasEnoughSilver(Map map, int fee)
		{
			return (from t in TradeUtility.AllLaunchableThings(map)
			where t.def == ThingDefOf.Silver
			select t).Sum((Thing t) => t.stackCount) >= fee;
		}

		private void LaunchSilver(Map map, int fee)
		{
			TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, fee, map, null);
		}

		private int RandomFee(Pawn pawn)
		{
			return (int)(pawn.MarketValue * Rand.Range(0.9f, 2.2f));
		}
	}
}
