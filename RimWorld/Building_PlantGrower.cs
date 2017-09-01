using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Building_PlantGrower : Building, IPlantToGrowSettable
	{
		private ThingDef plantDefToGrow;

		private CompPowerTrader compPower;

		public IEnumerable<Plant> PlantsOnMe
		{
			get
			{
				if (base.Spawned)
				{
					CellRect.CellRectIterator cri = this.OccupiedRect().GetIterator();
					while (!cri.Done())
					{
						List<Thing> thingList = base.Map.thingGrid.ThingsListAt(cri.Current);
						for (int i = 0; i < thingList.Count; i++)
						{
							Plant p = thingList[i] as Plant;
							if (p != null)
							{
								yield return p;
							}
						}
						cri.MoveNext();
					}
				}
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			yield return PlantToGrowSettableUtility.SetPlantToGrowCommand(this);
		}

		public override void PostMake()
		{
			base.PostMake();
			this.plantDefToGrow = this.def.building.defaultPlantToGrow;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.compPower = base.GetComp<CompPowerTrader>();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<ThingDef>(ref this.plantDefToGrow, "plantDefToGrow");
		}

		public override void TickRare()
		{
			if (this.compPower != null && !this.compPower.PowerOn)
			{
				foreach (Plant current in this.PlantsOnMe)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Rotting, 4, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
					current.TakeDamage(dinfo);
				}
			}
		}

		public override void DeSpawn()
		{
			foreach (Plant current in this.PlantsOnMe.ToList<Plant>())
			{
				current.Destroy(DestroyMode.Vanish);
			}
			base.DeSpawn();
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (base.Spawned)
			{
				if (GenPlant.GrowthSeasonNow(base.Position, base.Map))
				{
					text = text + "\n" + "GrowSeasonHereNow".Translate();
				}
				else
				{
					text = text + "\n" + "CannotGrowBadSeasonTemperature".Translate();
				}
			}
			return text;
		}

		public ThingDef GetPlantDefToGrow()
		{
			return this.plantDefToGrow;
		}

		public void SetPlantDefToGrow(ThingDef plantDef)
		{
			this.plantDefToGrow = plantDef;
		}

		public bool CanAcceptSowNow()
		{
			return this.compPower == null || this.compPower.PowerOn;
		}

		virtual Map get_Map()
		{
			return base.Map;
		}
	}
}
