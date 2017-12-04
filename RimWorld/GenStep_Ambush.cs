using System;
using Verse;

namespace RimWorld
{
	public abstract class GenStep_Ambush : GenStep
	{
		public FloatRange pointsRange = new FloatRange(180f, 340f);

		public override void Generate(Map map)
		{
			CellRect rectToDefend;
			IntVec3 root;
			if (!SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out rectToDefend, out root, map))
			{
				return;
			}
			this.SpawnTrigger(rectToDefend, root, map);
		}

		private void SpawnTrigger(CellRect rectToDefend, IntVec3 root, Map map)
		{
			int nextSignalTagID = Find.UniqueIDsManager.GetNextSignalTagID();
			string signalTag = "ambushActivated-" + nextSignalTagID;
			CellRect rect;
			if (root.IsValid)
			{
				rect = CellRect.CenteredOn(root, 17);
			}
			else
			{
				rect = rectToDefend.ExpandedBy(12);
			}
			SignalAction_Ambush signalAction_Ambush = this.MakeAmbushSignalAction(rectToDefend, root);
			signalAction_Ambush.signalTag = signalTag;
			GenSpawn.Spawn(signalAction_Ambush, rect.CenterCell, map);
			RectTrigger rectTrigger = this.MakeRectTrigger();
			rectTrigger.signalTag = signalTag;
			rectTrigger.Rect = rect;
			GenSpawn.Spawn(rectTrigger, rect.CenterCell, map);
		}

		protected virtual RectTrigger MakeRectTrigger()
		{
			return (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger, null);
		}

		protected virtual SignalAction_Ambush MakeAmbushSignalAction(CellRect rectToDefend, IntVec3 root)
		{
			SignalAction_Ambush signalAction_Ambush = (SignalAction_Ambush)ThingMaker.MakeThing(ThingDefOf.SignalAction_Ambush, null);
			signalAction_Ambush.points = this.pointsRange.RandomInRange;
			int num = Rand.RangeInclusive(0, 2);
			if (num == 0)
			{
				signalAction_Ambush.manhunters = true;
			}
			else if (num == 1 && PawnGroupMakerUtility.CanGenerateAnyNormalGroup(Faction.OfMechanoids, signalAction_Ambush.points))
			{
				signalAction_Ambush.mechanoids = true;
			}
			return signalAction_Ambush;
		}
	}
}
