using System;
using Verse;

namespace RimWorld
{
	public abstract class GenStep_Ambush : GenStep
	{
		public FloatRange defaultPointsRange = new FloatRange(180f, 340f);

		public override void Generate(Map map, GenStepParams parms)
		{
			CellRect rectToDefend;
			IntVec3 root;
			if (!SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out rectToDefend, out root, map))
			{
				return;
			}
			this.SpawnTrigger(rectToDefend, root, map, parms);
		}

		private void SpawnTrigger(CellRect rectToDefend, IntVec3 root, Map map, GenStepParams parms)
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
			SignalAction_Ambush signalAction_Ambush = this.MakeAmbushSignalAction(rectToDefend, root, parms);
			signalAction_Ambush.signalTag = signalTag;
			GenSpawn.Spawn(signalAction_Ambush, rect.CenterCell, map, WipeMode.Vanish);
			RectTrigger rectTrigger = this.MakeRectTrigger();
			rectTrigger.signalTag = signalTag;
			rectTrigger.Rect = rect;
			GenSpawn.Spawn(rectTrigger, rect.CenterCell, map, WipeMode.Vanish);
			TriggerUnfogged triggerUnfogged = (TriggerUnfogged)ThingMaker.MakeThing(ThingDefOf.TriggerUnfogged, null);
			triggerUnfogged.signalTag = signalTag;
			GenSpawn.Spawn(triggerUnfogged, rect.CenterCell, map, WipeMode.Vanish);
		}

		protected virtual RectTrigger MakeRectTrigger()
		{
			return (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger, null);
		}

		protected virtual SignalAction_Ambush MakeAmbushSignalAction(CellRect rectToDefend, IntVec3 root, GenStepParams parms)
		{
			SignalAction_Ambush signalAction_Ambush = (SignalAction_Ambush)ThingMaker.MakeThing(ThingDefOf.SignalAction_Ambush, null);
			if (parms.siteCoreOrPart != null)
			{
				signalAction_Ambush.points = parms.siteCoreOrPart.parms.threatPoints;
			}
			else
			{
				signalAction_Ambush.points = this.defaultPointsRange.RandomInRange;
			}
			int num = Rand.RangeInclusive(0, 2);
			if (num == 0)
			{
				signalAction_Ambush.ambushType = SignalActionAmbushType.Manhunters;
			}
			else if (num == 1 && PawnGroupMakerUtility.CanGenerateAnyNormalGroup(Faction.OfMechanoids, signalAction_Ambush.points))
			{
				signalAction_Ambush.ambushType = SignalActionAmbushType.Mechanoids;
			}
			else
			{
				signalAction_Ambush.ambushType = SignalActionAmbushType.Normal;
			}
			return signalAction_Ambush;
		}
	}
}
