using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;
using Verse.Sound;

namespace Verse
{
	public class Building : ThingWithComps
	{
		private Sustainer sustainerAmbient;

		public CompPower PowerComp
		{
			get
			{
				return base.GetComp<CompPower>();
			}
		}

		public virtual bool TransmitsPowerNow
		{
			get
			{
				CompPower powerComp = this.PowerComp;
				return powerComp != null && powerComp.Props.transmitsPower;
			}
		}

		public override int HitPoints
		{
			set
			{
				int hitPoints = this.HitPoints;
				base.HitPoints = value;
				BuildingsDamageSectionLayerUtility.Notify_BuildingHitPointsChanged(this, hitPoints);
			}
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			Find.ListerBuildings.Add(this);
			if (this.def.coversFloor)
			{
				Find.MapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Terrain, true, false);
			}
			CellRect cellRect = this.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					Find.MapDrawer.MapMeshDirty(intVec, MapMeshFlag.Buildings);
					Find.GlowGrid.MarkGlowGridDirty(intVec);
					if (!SnowGrid.CanCoexistWithSnow(this.def))
					{
						Find.SnowGrid.SetDepth(intVec, 0f);
					}
				}
			}
			if (this.def.IsEdifice())
			{
				Find.EdificeGrid.Register(this);
			}
			if (base.Faction == Faction.OfPlayer && this.def.building != null && this.def.building.spawnedConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(this.def.building.spawnedConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
			AutoHomeAreaMaker.Notify_BuildingSpawned(this);
			if (this.def.building != null && !this.def.building.soundAmbient.NullOrUndefined())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					SoundInfo info = SoundInfo.InWorld(this, MaintenanceType.None);
					this.sustainerAmbient = this.def.building.soundAmbient.TrySpawnSustainer(info);
				});
			}
			ListerBuildingsRepairable.Notify_BuildingSpawned(this);
		}

		public override void DeSpawn()
		{
			if (this.def.IsEdifice())
			{
				Find.EdificeGrid.DeRegister(this);
			}
			base.DeSpawn();
			if (this.def.building.ai_combatDangerous)
			{
				AvoidGridMaker.Notify_CombatDangerousBuildingDespawned(this);
			}
			if (this.def.MakeFog)
			{
				Find.FogGrid.Notify_FogBlockerRemoved(base.Position);
			}
			if (this.def.holdsRoof)
			{
				RoofCollapseCellsFinder.Notify_RoofHolderDespawned(this);
			}
			if (this.sustainerAmbient != null)
			{
				this.sustainerAmbient.End();
			}
			CellRect cellRect = this.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 loc = new IntVec3(j, 0, i);
					MapMeshFlag mapMeshFlag = MapMeshFlag.Buildings;
					if (this.def.coversFloor)
					{
						mapMeshFlag |= MapMeshFlag.Terrain;
					}
					if (this.def.Fillage == FillCategory.Full)
					{
						mapMeshFlag |= MapMeshFlag.Roofs;
						mapMeshFlag |= MapMeshFlag.Snow;
					}
					Find.Map.mapDrawer.MapMeshDirty(loc, mapMeshFlag);
					Find.GlowGrid.MarkGlowGridDirty(loc);
				}
			}
			Find.ListerBuildings.Remove(this);
			ListerBuildingsRepairable.Notify_BuildingDeSpawned(this);
			if (this.def.leaveTerrain != null && Current.ProgramState == ProgramState.MapPlaying)
			{
				CellRect.CellRectIterator iterator = this.OccupiedRect().GetIterator();
				while (!iterator.Done())
				{
					Find.TerrainGrid.SetTerrain(iterator.Current, this.def.leaveTerrain);
					iterator.MoveNext();
				}
			}
			Find.DesignationManager.Notify_BuildingDespawned(this);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			InstallBlueprintUtility.CancelBlueprintsFor(this);
			if (mode == DestroyMode.Deconstruct)
			{
				SoundDef.Named("BuildingDeconstructed").PlayOneShot(base.Position);
			}
		}

		public override void Draw()
		{
			if (this.def.drawerType == DrawerType.RealtimeOnly)
			{
				base.Draw();
			}
			base.Comps_PostDraw();
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			base.SetFaction(newFaction, recruiter);
			ListerBuildingsRepairable.Notify_BuildingFactionChanged(this);
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostApplyDamage(dinfo, totalDamageDealt);
			ListerBuildingsRepairable.Notify_BuildingTookDamage(this);
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(this);
			if (blueprint_Install != null)
			{
				GenDraw.DrawLineBetween(this.TrueCenter(), blueprint_Install.TrueCenter());
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo c in base.GetGizmos())
			{
				yield return c;
			}
			if (this.def.Minifiable)
			{
				yield return InstallationDesignatorDatabase.DesignatorFor(this.def);
			}
			Command buildCopy = BuildCopyCommandUtility.BuildCopyCommand(this.def, base.Stuff);
			if (buildCopy != null)
			{
				yield return buildCopy;
			}
		}

		public virtual bool ClaimableBy(Faction faction)
		{
			return !this.def.building.isNaturalRock && this.def.building.claimable;
		}

		public virtual ushort PathFindCostFor(Pawn p)
		{
			return 0;
		}

		public virtual ushort PathWalkCostFor(Pawn p)
		{
			return 0;
		}
	}
}
