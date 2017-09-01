using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public sealed class Room
	{
		private const int RegionCountHuge = 60;

		private const int MaxRegionsToAssignRoomRole = 36;

		public sbyte mapIndex = -1;

		private RoomGroup groupInt;

		private List<Region> regions = new List<Region>();

		public int ID = -16161616;

		public int lastChangeTick = -1;

		private int numRegionsTouchingMapEdge;

		private int cachedOpenRoofCount = -1;

		public bool isPrisonCell;

		private int cachedCellCount = -1;

		private bool statsAndRoleDirty = true;

		private DefMap<RoomStatDef, float> stats = new DefMap<RoomStatDef, float>();

		private RoomRoleDef role;

		public int newOrReusedRoomGroupIndex = -1;

		private static int nextRoomID;

		private static readonly Color PrisonFieldColor = new Color(1f, 0.7f, 0.2f);

		private static readonly Color NonPrisonFieldColor = new Color(0.3f, 0.3f, 1f);

		private HashSet<Room> uniqueNeighborsSet = new HashSet<Room>();

		private List<Room> uniqueNeighbors = new List<Room>();

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

		public RegionType RegionType
		{
			get
			{
				return (!this.regions.Any<Region>()) ? RegionType.None : this.regions[0].type;
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

		public float Temperature
		{
			get
			{
				return this.Group.Temperature;
			}
		}

		public bool UsesOutdoorTemperature
		{
			get
			{
				return this.Group.UsesOutdoorTemperature;
			}
		}

		public RoomGroup Group
		{
			get
			{
				return this.groupInt;
			}
			set
			{
				if (value == this.groupInt)
				{
					return;
				}
				if (this.groupInt != null)
				{
					this.groupInt.RemoveRoom(this);
				}
				this.groupInt = value;
				if (this.groupInt != null)
				{
					this.groupInt.AddRoom(this);
				}
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

		public bool PsychologicallyOutdoors
		{
			get
			{
				return this.TouchesMapEdge || this.OpenRoofCount > 300 || (this.Group.AnyRoomTouchesMapEdge && (float)this.OpenRoofCount / (float)this.CellCount >= 0.5f);
			}
		}

		public List<Room> Neighbors
		{
			get
			{
				this.uniqueNeighborsSet.Clear();
				this.uniqueNeighbors.Clear();
				for (int i = 0; i < this.regions.Count; i++)
				{
					foreach (Region current in this.regions[i].Neighbors)
					{
						if (this.uniqueNeighborsSet.Add(current.Room) && current.Room != this)
						{
							this.uniqueNeighbors.Add(current.Room);
						}
					}
				}
				this.uniqueNeighborsSet.Clear();
				return this.uniqueNeighbors;
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

		public IEnumerable<IntVec3> BorderCells
		{
			get
			{
				foreach (IntVec3 c in this.Cells)
				{
					for (int i = 0; i < 8; i++)
					{
						IntVec3 prospective = c + GenAdj.AdjacentCells[i];
						Region region = (c + GenAdj.AdjacentCells[i]).GetRegion(this.Map, RegionType.Set_Passable);
						if (region == null || region.Room != this)
						{
							yield return prospective;
						}
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
				List<Thing> things = this.ContainedAndAdjacentThings;
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

		public List<Thing> ContainedAndAdjacentThings
		{
			get
			{
				this.uniqueContainedThingsSet.Clear();
				this.uniqueContainedThings.Clear();
				for (int i = 0; i < this.regions.Count; i++)
				{
					List<Thing> allThings = this.regions[i].ListerThings.AllThings;
					if (allThings != null)
					{
						for (int j = 0; j < allThings.Count; j++)
						{
							Thing item = allThings[j];
							if (this.uniqueContainedThingsSet.Add(item))
							{
								this.uniqueContainedThings.Add(item);
							}
						}
					}
				}
				this.uniqueContainedThingsSet.Clear();
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

		public static Room MakeNew(Map map)
		{
			Room room = new Room();
			room.mapIndex = (sbyte)map.Index;
			room.ID = Room.nextRoomID;
			Room.nextRoomID++;
			return room;
		}

		public void AddRegion(Region r)
		{
			if (this.regions.Contains(r))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to add the same region twice to Room. region=",
					r,
					", room=",
					this
				}));
				return;
			}
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
			if (!this.regions.Contains(r))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to remove region from Room but this region is not here. region=",
					r,
					", room=",
					this
				}));
				return;
			}
			this.regions.Remove(r);
			if (r.touchesMapEdge)
			{
				this.numRegionsTouchingMapEdge--;
			}
			if (this.regions.Count == 0)
			{
				this.Group = null;
				this.cachedOpenRoofCount = -1;
				this.cachedOpenRoofCount = -1;
				this.statsAndRoleDirty = true;
				this.Map.regionGrid.allRooms.Remove(this);
			}
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

		public void Notify_RoofChanged()
		{
			this.cachedOpenRoofCount = -1;
			this.Group.Notify_RoofChanged();
		}

		public void Notify_RoomShapeOrContainedBedsChanged()
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
			if (Building_Bed.RoomCanBePrisonCell(this))
			{
				List<Thing> containedAndAdjacentThings = this.ContainedAndAdjacentThings;
				for (int i = 0; i < containedAndAdjacentThings.Count; i++)
				{
					Building_Bed building_Bed = containedAndAdjacentThings[i] as Building_Bed;
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

		public RoomStatScoreStage GetStatScoreStage(RoomStatDef stat)
		{
			return stat.GetScoreStage(this.GetStat(stat));
		}

		public void DrawFieldEdges()
		{
			Room.fields.Clear();
			Room.fields.AddRange(this.Cells);
			Color color = (!this.isPrisonCell) ? Room.NonPrisonFieldColor : Room.PrisonFieldColor;
			color.a = Pulser.PulseBrightness(1f, 0.6f);
			GenDraw.DrawFieldEdges(Room.fields, color);
			Room.fields.Clear();
		}

		private void UpdateRoomStatsAndRole()
		{
			this.statsAndRoleDirty = false;
			if (!this.TouchesMapEdge && this.RegionType == RegionType.Normal && this.regions.Count <= 36)
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
		}

		internal string DebugString()
		{
			return string.Concat(new object[]
			{
				"Room ID=",
				this.ID,
				"\n  first cell=",
				this.Cells.FirstOrDefault<IntVec3>(),
				"\n  RegionCount=",
				this.RegionCount,
				"\n  RegionType=",
				this.RegionType,
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
				"\n  RoomGroup=",
				(this.Group == null) ? "null" : this.Group.ID.ToString()
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
