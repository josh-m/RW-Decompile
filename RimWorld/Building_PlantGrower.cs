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
				CellRect.CellRectIterator cri = this.OccupiedRect().GetIterator();
				while (!cri.Done())
				{
					List<Thing> thingList = Find.ThingGrid.ThingsListAt(cri.Current);
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

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			this.compPower = base.GetComp<CompPowerTrader>();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<ThingDef>(ref this.plantDefToGrow, "plantDefToGrow");
		}

		public override void TickRare()
		{
			if (this.compPower != null && !this.compPower.PowerOn)
			{
				foreach (Plant current in this.PlantsOnMe)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Rotting, 4, null, null, null);
					current.TakeDamage(dinfo);
				}
			}
		}

		public override void DeSpawn()
		{
			base.DeSpawn();
			if (this.def.building.plantsDestroyWithMe)
			{
				foreach (Plant current in this.PlantsOnMe.ToList<Plant>())
				{
					current.Destroy(DestroyMode.Vanish);
				}
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (GenPlant.GrowthSeasonNow(base.Position))
			{
				text = text + "\n" + "GrowSeasonHereNow".Translate();
			}
			else
			{
				text = text + "\n" + "CannotGrowTooCold".Translate();
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
	}
}
