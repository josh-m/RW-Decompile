using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public sealed class Room
	{
		private const int RegionCountHuge = 60;

		private const float UseOutdoorTemperatureUnroofedFraction = 0.25f;

		public sbyte mapIndex = -1;

		private List<Region> regions = new List<Region>();

		public int ID = -16161616;

		public int lastChangeTick = -1;

		private RoomTempTracker tempTracker;

		private int numRegionsTouchingMapEdge;

		public bool isPrisonCell;

		private int cachedCellCount = -1;

		private int cachedOpenRoofCount = -1;

		private bool statsAndRoleDirty = true;

		private DefMap<RoomStatDef, float> stats = new DefMap<RoomStatDef, float>();

		private RoomRoleDef role;

		private static int nextRoomID = 0;

		private static readonly Color PrisonFieldColor = new Color(1f, 0.7f, 0.2f);

		private static readonly Color NonPrisonFieldColor = new Color(0.3f, 0.3f, 1f);

		private HashSet<Thing> uniqueContainedThingsSet = new HashSet<Thing>();

		private List<Thing> uniqueContainedThings = new List<Thing>();

		private static List<IntVec3> fields = new List<IntVec3>();

		public Map Map
		{
			get
			{
				return ((int)this.mapIndex >= 0) ? Find.Maps[(int)this.mapIndex] : null;
			}
		}

		public RoomTempTracker TempTracker
		{
			get
			{
				return this.tempTracker;
			}
		}

		public List<Region> Regions
		{
			get
			{
				return this.regions;
			}
		}

		public int RegionCount
		{
			get
			{
				return this.regions.Count;
			}
		}

		public bool IsHuge
		{
			get
			{
				return this.regions.Count > 60;
			}
		}

		public bool Dereferenced
		{
			get
			{
				return this.regions.Count == 0;
			}
		}

		public bool TouchesMapEdge
		{
			get
			{
				return this.numRegionsTouchingMapEdge > 0;
			}
		}

		public bool PsychologicallyOutdoors
		{
			get
			{
				return this.TouchesMapEdge || this.OpenRoofCount > 300;
			}
		}

		public float Temperature
		{
			get
			{
				return this.tempTracker.Temperature;
			}
			set
			{
				this.tempTracker.Temperature = value;
			}
		}

		public bool UsesOutdoorTemperature
		{
			get
			{
				return this.TouchesMapEdge || this.OpenRoofCount > Mathf.CeilToInt((float)this.CellCount * 0.25f);
			}
		}

		public int CellCount
		{
			get
			{
				if (this.cachedCellCount == -1)
				{
					this.cachedCellCount = 0;
					for (int i = 0; i < this.regions.Count; i++)
					{
						this.cachedCellCount += this.regions[i].CellCount;
					}
				}
				return this.cachedCellCount;
			}
		}

		public int OpenRoofCount
		{
			get
			{
				if (this.cachedOpenRoofCount == -1)
				{
					this.cachedOpenRoofCount = 0;
					if (this.Map != null)
					{
						RoofGrid roofGrid = this.Map.roofGrid;
						foreach (IntVec3 current in this.Cells)
						{
							if (!roofGrid.Roofed(current))
							{
								this.cachedOpenRoofCount++;
							}
						}
					}
				}
				return this.cachedOpenRoofCount;
			}
		}

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				for (int i = 0; i < this.regions.Count; i++)
				{
					foreach (IntVec3 c in this.regions[i].Cells)
					{
						yield return c;
					}
				}
			}
		}

		public IEnumerable<Pawn> Owners
		{
			get
			{
				if (!this.TouchesMapEdge)
				{
					if (!this.IsHuge)
					{
						if (this.Role == RoomRoleDefOf.Bedroom || this.Role == RoomRoleDefOf.PrisonCell || this.Role == RoomRoleDefOf.Barracks || this.Role == RoomRoleDefOf.PrisonBarracks)
						{
							Pawn firstOwner = null;
							Pawn secondOwner = null;
							foreach (Building_Bed bed in this.ContainedBeds)
							{
								if (bed.def.building.bed_humanlike)
								{
									for (int i = 0; i < bed.owners.Count; i++)
									{
										if (firstOwner == null)
										{
											firstOwner = bed.owners[i];
										}
										else
										{
											if (secondOwner != null)
											{
												return;
											}
											secondOwner = bed.owners[i];
										}
									}
								}
							}
							if (firstOwner != null)
							{
								if (secondOwner == null)
								{
									yield return firstOwner;
								}
								else if (LovePartnerRelationUtility.LovePartnerRelationExists(firstOwner, secondOwner))
								{
									yield return firstOwner;
									yield return secondOwner;
								}
							}
						}
					}
				}
			}
		}

		public IEnumerable<Building_Bed> ContainedBeds
		{
			get
			{
				if (this.regions.Count > 60)
				{
					Log.Warning("Perf warning: Ran ContainedBeds in huge room " + this.ToString());
				}
				List<Thing> things = this.AllContainedThings;
				for (int i = 0; i < things.Count; i++)
				{
					Building_Bed bed = things[i] as Building_Bed;
					if (bed != null)
					{
						yield return bed;
					}
				}
			}
		}

		public bool Fogged
		{
			get
			{
				return this.regions.Count != 0 && this.regions[0].AnyCell.Fogged(this.Map);
			}
		}

		public List<Thing> AllContainedThings
		{
			get
			{
				this.uniqueContainedThingsSet.Clear();
				this.uniqueContainedThings.Clear();
				for (int i = 0; i < this.regions.Count; i++)
				{
					if (this.regions[i].ListerThings.AllThings != null)
					{
						for (int j = 0; j < this.regions[i].ListerThings.AllThings.Count; j++)
						{
							Thing item = this.regions[i].ListerThings.AllThings[j];
							if (!this.uniqueContainedThingsSet.Contains(item))
							{
								this.uniqueContainedThingsSet.Add(item);
								this.uniqueContainedThings.Add(item);
							}
						}
					}
				}
				return this.uniqueContainedThings;
			}
		}

		public RoomRoleDef Role
		{
			get
			{
				if (this.statsAndRoleDirty)
				{
					this.UpdateRoomStatsAndRole();
				}
				return this.role;
			}
		}

		public bool IsDoor
		{
			get
			{
				if (this.CellCount != 1)
				{
					return false;
				}
				Building edifice = this.Cells.FirstOrDefault<IntVec3>().GetEdifice(this.Map);
				return edifice != null && edifice is Building_Door;
			}
		}

		[DebuggerHidden]
		public IEnumerator<IntVec3> GetEnumerator()
		{
			foreach (IntVec3 c in this.Cells)
			{
				yield return c;
			}
		}

		public static Room MakeNew(Map map)
		{
			Room room = new Room();
			room.mapIndex = (sbyte)map.Index;
			room.ID = Room.nextRoomID;
			room.tempTracker = new RoomTempTracker(room);
			Room.nextRoomID++;
			return room;
		}

		public void AddRegion(Region r)
		{
			this.regions.Add(r);
			if (r.touchesMapEdge)
			{
				this.numRegionsTouchingMapEdge++;
			}
			if (this.regions.Count == 1)
			{
				this.Map.regionGrid.allRooms.Add(this);
			}
		}

		public void RemoveRegion(Region r)
		{
			this.regions.Remove(r);
			if (r.touchesMapEdge)
			{
				this.numRegionsTouchingMapEdge--;
			}
			if (this.regions.Count == 0)
			{
				this.Map.regionGrid.allRooms.Remove(this);
			}
		}

		public void RoofChanged()
		{
			this.cachedOpenRoofCount = -1;
			this.tempTracker.RoofChanged();
		}

		public void RoomChanged()
		{
			ProfilerThreadCheck.BeginSample("RoomChanged");
			this.cachedCellCount = -1;
			this.cachedOpenRoofCount = -1;
			if (Current.ProgramState == ProgramState.Playing && !this.Fogged)
			{
				ProfilerThreadCheck.BeginSample("RoofGenerationRequest");
				this.Map.autoBuildRoofAreaSetter.TryGenerateAreaFor(this);
				ProfilerThreadCheck.EndSample();
			}
			this.isPrisonCell = false;
			if (!this.TouchesMapEdge)
			{
				List<Thing> allContainedThings = this.AllContainedThings;
				for (int i = 0; i < allContainedThings.Count; i++)
				{
					Building_Bed building_Bed = allContainedThings[i] as Building_Bed;
					if (building_Bed != null && building_Bed.ForPrisoners)
					{
						this.isPrisonCell = true;
						break;
					}
				}
			}
			if (Current.ProgramState == ProgramState.Playing && this.isPrisonCell)
			{
				foreach (Building_Bed current in this.ContainedBeds)
				{
					current.ForPrisoners = true;
				}
			}
			this.lastChangeTick = Find.TickManager.TicksGame;
			this.tempTracker.RoomChanged();
			this.statsAndRoleDirty = true;
			FacilitiesUtility.NotifyFacilitiesAboutChangedLOSBlockers(this.regions);
			ProfilerThreadCheck.EndSample();
		}

		public void DecrementMapIndex()
		{
			if ((int)this.mapIndex <= 0)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to decrement map index for room ",
					this.ID,
					", but mapIndex=",
					this.mapIndex
				}));
				return;
			}
			this.mapIndex -= 1;
		}

		public void Notify_MyMapRemoved()
		{
			this.mapIndex = -1;
		}

		public void Notify_ContainedThingSpawnedOrDespawned(Thing th)
		{
			if (th.def.category != ThingCategory.Mote && th.def.category != ThingCategory.Projectile && th.def.category != ThingCategory.Skyfaller && th.def.category != ThingCategory.Pawn)
			{
				this.statsAndRoleDirty = true;
			}
		}

		public void Notify_TerrainChanged()
		{
			this.statsAndRoleDirty = true;
		}

		public void Notify_BedTypeChanged()
		{
			this.statsAndRoleDirty = true;
		}

		public float GetStat(RoomStatDef roomStat)
		{
			if (this.statsAndRoleDirty)
			{
				this.UpdateRoomStatsAndRole();
			}
			if (this.stats == null)
			{
				return roomStat.defaultScore;
			}
			return this.stats[roomStat];
		}

		public bool PushHeat(float energy)
		{
			if (this.UsesOutdoorTemperature)
			{
				return false;
			}
			this.Temperature += energy / (float)this.CellCount;
			return true;
		}

		public RoomStatScoreStage GetStatScoreStage(RoomStatDef stat)
		{
			return stat.GetScoreStage(this.GetStat(stat));
		}

		public void DrawFieldEdges()
		{
			if (this.RegionCount >= 20 || this.TouchesMapEdge)
			{
				return;
			}
			foreach (IntVec3 current in this.Cells)
			{
				Room.fields.Add(current);
			}
			Color color = (!this.isPrisonCell) ? Room.NonPrisonFieldColor : Room.PrisonFieldColor;
			color.a = Pulser.PulseBrightness(1f, 0.6f);
			GenDraw.DrawFieldEdges(Room.fields, color);
			Room.fields.Clear();
		}

		private void UpdateRoomStatsAndRole()
		{
			this.statsAndRoleDirty = false;
			if (!this.TouchesMapEdge && !this.IsDoor)
			{
				if (this.stats == null)
				{
					this.stats = new DefMap<RoomStatDef, float>();
				}
				foreach (RoomStatDef current in from x in DefDatabase<RoomStatDef>.AllDefs
				orderby x.updatePriority descending
				select x)
				{
					this.stats[current] = current.Worker.GetScore(this);
				}
				this.role = DefDatabase<RoomRoleDef>.AllDefs.MaxBy((RoomRoleDef x) => x.Worker.GetScore(this));
			}
			else
			{
				this.stats = null;
				this.role = RoomRoleDefOf.None;
			}
		}

		internal void DebugDraw()
		{
			int hashCode = this.GetHashCode();
			foreach (IntVec3 current in this.Cells)
			{
				CellRenderer.RenderCell(current, (float)hashCode * 0.01f);
			}
			this.tempTracker.DebugDraw();
		}

		internal string DebugString()
		{
			return string.Concat(new object[]
			{
				"Room ID=",
				this.ID,
				"\n  first=",
				this.Cells.FirstOrDefault<IntVec3>().ToString(),
				"\n  RegionsCount=",
				this.RegionCount.ToString(),
				"\n  CellCount=",
				this.CellCount,
				"\n  OpenRoofCount=",
				this.OpenRoofCount,
				"\n  numRegionsTouchingMapEdge=",
				this.numRegionsTouchingMapEdge,
				"\n  lastChangeTick=",
				this.lastChangeTick,
				"\n  isPrisonCell=",
				this.isPrisonCell,
				"\n  ",
				this.tempTracker.DebugString()
			});
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Room(roomID=",
				this.ID,
				", first=",
				this.Cells.FirstOrDefault<IntVec3>().ToString(),
				", RegionsCount=",
				this.RegionCount.ToString(),
				", lastChangeTick=",
				this.lastChangeTick,
				")"
			});
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(this.ID, 1538478890);
		}
	}
}
