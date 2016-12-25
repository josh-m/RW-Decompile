using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RansomDemand : IncidentWorker
	{
		private static List<Pawn> candidates = new List<Pawn>();

		protected override bool CanFireNowSub()
		{
			return this.RandomKidnappedColonist() != null && base.CanFireNowSub();
		}

		public override bool TryExecute(IncidentParms parms)
		{
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
					if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable() && c.CanReachColony(), out intVec))
					{
						if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(), out intVec))
						{
							Log.Warning("Could not find any edge cell.");
							intVec = DropCellFinder.TradeDropSpot();
						}
					}
					GenSpawn.Spawn(colonist, intVec);
				}
				else
				{
					intVec = DropCellFinder.TradeDropSpot();
					TradeUtility.SpawnDropPod(intVec, colonist);
				}
				Find.CameraDriver.JumpTo(intVec);
				this.LaunchSilver(fee);
			};
			diaOption.resolveTree = true;
			if (!this.ColonyHasEnoughSilver(fee))
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

		private bool ColonyHasEnoughSilver(int fee)
		{
			return (from t in TradeUtility.AllLaunchableThings
			where t.def == ThingDefOf.Silver
			select t).Sum((Thing t) => t.stackCount) >= fee;
		}

		private void LaunchSilver(int fee)
		{
			TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, fee, null);
		}

		private int RandomFee(Pawn pawn)
		{
			return (int)(pawn.MarketValue * Rand.Range(1f, 3f));
		}
	}
}
