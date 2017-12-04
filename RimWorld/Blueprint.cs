using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Blueprint : ThingWithComps, IConstructible
	{
		private static List<CompSpawnerMechanoidsOnDamaged> tmpCrashedShipParts = new List<CompSpawnerMechanoidsOnDamaged>();

		public override string Label
		{
			get
			{
				return this.def.entityDefToBuild.label + "BlueprintLabelExtra".Translate();
			}
		}

		protected abstract float WorkTotal
		{
			get;
		}

		public override void Tick()
		{
			base.Tick();
			if (!GenConstruct.CanBuildOnTerrain(this.def.entityDefToBuild, base.Position, base.Map, base.Rotation, null))
			{
				this.Destroy(DestroyMode.Cancel);
			}
		}

		public override void Draw()
		{
			if (this.def.drawerType == DrawerType.RealtimeOnly)
			{
				base.Draw();
			}
			else
			{
				base.Comps_PostDraw();
			}
		}

		public virtual bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
		{
			jobEnded = false;
			if (GenConstruct.FirstBlockingThing(this, workerPawn) != null)
			{
				workerPawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				jobEnded = true;
				createdThing = null;
				return false;
			}
			createdThing = this.MakeSolidThing();
			Map map = base.Map;
			CellRect cellRect = this.OccupiedRect();
			GenSpawn.WipeExistingThings(base.Position, base.Rotation, createdThing.def, map, DestroyMode.Deconstruct);
			if (!base.Destroyed)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			createdThing.SetFactionDirect(workerPawn.Faction);
			GenSpawn.Spawn(createdThing, base.Position, map, base.Rotation, false);
			Blueprint.tmpCrashedShipParts.Clear();
			CellRect.CellRectIterator iterator = cellRect.ExpandedBy(3).GetIterator();
			while (!iterator.Done())
			{
				if (iterator.Current.InBounds(map))
				{
					List<Thing> thingList = iterator.Current.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						CompSpawnerMechanoidsOnDamaged compSpawnerMechanoidsOnDamaged = thingList[i].TryGetComp<CompSpawnerMechanoidsOnDamaged>();
						if (compSpawnerMechanoidsOnDamaged != null)
						{
							Blueprint.tmpCrashedShipParts.Add(compSpawnerMechanoidsOnDamaged);
						}
					}
				}
				iterator.MoveNext();
			}
			Blueprint.tmpCrashedShipParts.RemoveDuplicates<CompSpawnerMechanoidsOnDamaged>();
			for (int j = 0; j < Blueprint.tmpCrashedShipParts.Count; j++)
			{
				Blueprint.tmpCrashedShipParts[j].Notify_BlueprintReplacedWithSolidThingNearby(workerPawn);
			}
			Blueprint.tmpCrashedShipParts.Clear();
			return true;
		}

		protected abstract Thing MakeSolidThing();

		public abstract List<ThingCountClass> MaterialsNeeded();

		public abstract ThingDef UIStuff();

		public Thing BlockingHaulableOnTop()
		{
			if (this.def.entityDefToBuild.passability == Traversability.Standable)
			{
				return null;
			}
			CellRect.CellRectIterator iterator = this.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.EverHaulable)
					{
						return thing;
					}
				}
				iterator.MoveNext();
			}
			return null;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append("WorkLeft".Translate() + ": " + this.WorkTotal.ToStringWorkAmount());
			return stringBuilder.ToString();
		}
	}
}
