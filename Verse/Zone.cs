using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Zone : IExposable, ISelectable, ILoadReferenceable
	{
		public ZoneManager zoneManager;

		public int ID = -1;

		public string label;

		public List<IntVec3> cells = new List<IntVec3>();

		private bool cellsShuffled;

		public Color color = Color.white;

		private Material materialInt;

		public bool hidden;

		private int lastStaticFireCheckTick = -9999;

		private bool lastStaticFireCheckResult;

		private const int StaticFireCheckInterval = 1000;

		private static BoolGrid extantGrid;

		private static BoolGrid foundGrid;

		public Map Map
		{
			get
			{
				return this.zoneManager.map;
			}
		}

		public IntVec3 Position
		{
			get
			{
				return (this.cells.Count == 0) ? IntVec3.Invalid : this.cells[0];
			}
		}

		public Material Material
		{
			get
			{
				if (this.materialInt == null)
				{
					this.materialInt = SolidColorMaterials.SimpleSolidColorMaterial(this.color, false);
					this.materialInt.renderQueue = 3600;
				}
				return this.materialInt;
			}
		}

		public List<IntVec3> Cells
		{
			get
			{
				if (!this.cellsShuffled)
				{
					this.cells.Shuffle<IntVec3>();
					this.cellsShuffled = true;
				}
				return this.cells;
			}
		}

		public IEnumerable<Thing> AllContainedThings
		{
			get
			{
				ThingGrid grids = this.Map.thingGrid;
				for (int i = 0; i < this.cells.Count; i++)
				{
					List<Thing> thingList = grids.ThingsListAt(this.cells[i]);
					for (int j = 0; j < thingList.Count; j++)
					{
						yield return thingList[j];
					}
				}
			}
		}

		public bool ContainsStaticFire
		{
			get
			{
				if (Find.TickManager.TicksGame > this.lastStaticFireCheckTick + 1000)
				{
					this.lastStaticFireCheckResult = false;
					for (int i = 0; i < this.cells.Count; i++)
					{
						if (this.cells[i].ContainsStaticFire(this.Map))
						{
							this.lastStaticFireCheckResult = true;
							break;
						}
					}
				}
				return this.lastStaticFireCheckResult;
			}
		}

		public virtual bool IsMultiselectable
		{
			get
			{
				return false;
			}
		}

		protected abstract Color NextZoneColor
		{
			get;
		}

		public Zone()
		{
		}

		public Zone(string baseName, ZoneManager zoneManager)
		{
			this.label = zoneManager.NewZoneName(baseName);
			this.zoneManager = zoneManager;
			this.ID = Find.UniqueIDsManager.GetNextZoneID();
			this.color = this.NextZoneColor;
		}

		[DebuggerHidden]
		public IEnumerator<IntVec3> GetEnumerator()
		{
			for (int i = 0; i < this.cells.Count; i++)
			{
				yield return this.cells[i];
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ID, "ID", -1, false);
			Scribe_Values.Look<string>(ref this.label, "label", null, false);
			Scribe_Values.Look<Color>(ref this.color, "color", default(Color), false);
			Scribe_Values.Look<bool>(ref this.hidden, "hidden", false, false);
			Scribe_Collections.Look<IntVec3>(ref this.cells, "cells", LookMode.Undefined, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.ZonePostLoadInit(this);
				this.CheckAddHaulDestination();
			}
		}

		public virtual void AddCell(IntVec3 c)
		{
			if (this.cells.Contains(c))
			{
				Log.Error(string.Concat(new object[]
				{
					"Adding cell to zone which already has it. c=",
					c,
					", zone=",
					this
				}), false);
				return;
			}
			List<Thing> list = this.Map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (!thing.def.CanOverlapZones)
				{
					Log.Error("Added zone over zone-incompatible thing " + thing, false);
					return;
				}
			}
			this.cells.Add(c);
			this.zoneManager.AddZoneGridCell(this, c);
			this.Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
			AutoHomeAreaMaker.Notify_ZoneCellAdded(c, this);
			this.cellsShuffled = false;
		}

		public virtual void RemoveCell(IntVec3 c)
		{
			if (!this.cells.Contains(c))
			{
				Log.Error(string.Concat(new object[]
				{
					"Cannot remove cell from zone which doesn't have it. c=",
					c,
					", zone=",
					this
				}), false);
				return;
			}
			this.cells.Remove(c);
			this.zoneManager.ClearZoneGridCell(c);
			this.Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
			this.cellsShuffled = false;
			if (this.cells.Count == 0)
			{
				this.Deregister();
			}
		}

		public virtual void Delete()
		{
			SoundDefOf.Designate_ZoneDelete.PlayOneShotOnCamera(this.Map);
			if (this.cells.Count == 0)
			{
				this.Deregister();
			}
			else
			{
				while (this.cells.Count > 0)
				{
					this.RemoveCell(this.cells[this.cells.Count - 1]);
				}
			}
			Find.Selector.Deselect(this);
		}

		public void Deregister()
		{
			this.zoneManager.DeregisterZone(this);
		}

		public virtual void PostRegister()
		{
			this.CheckAddHaulDestination();
		}

		public virtual void PostDeregister()
		{
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				this.Map.haulDestinationManager.RemoveHaulDestination(haulDestination);
			}
		}

		public bool ContainsCell(IntVec3 c)
		{
			for (int i = 0; i < this.cells.Count; i++)
			{
				if (this.cells[i] == c)
				{
					return true;
				}
			}
			return false;
		}

		public virtual string GetInspectString()
		{
			return string.Empty;
		}

		[DebuggerHidden]
		public virtual IEnumerable<InspectTabBase> GetInspectTabs()
		{
		}

		[DebuggerHidden]
		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone", true),
				defaultLabel = "CommandRenameZoneLabel".Translate(),
				defaultDesc = "CommandRenameZoneDesc".Translate(),
				action = delegate
				{
					Find.WindowStack.Add(new Dialog_RenameZone(this.$this));
				},
				hotKey = KeyBindingDefOf.Misc1
			};
			yield return new Command_Toggle
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone", true),
				defaultLabel = ((!this.hidden) ? "CommandHideZoneLabel".Translate() : "CommandUnhideZoneLabel".Translate()),
				defaultDesc = "CommandHideZoneDesc".Translate(),
				isActive = (() => this.$this.hidden),
				toggleAction = delegate
				{
					this.$this.hidden = !this.$this.hidden;
					foreach (IntVec3 current in this.$this.Cells)
					{
						this.$this.Map.mapDrawer.MapMeshDirty(current, MapMeshFlag.Zone);
					}
				},
				hotKey = KeyBindingDefOf.Misc2
			};
			foreach (Gizmo gizmo in this.GetZoneAddGizmos())
			{
				yield return gizmo;
			}
			Designator delete = DesignatorUtility.FindAllowedDesignator<Designator_ZoneDelete_Shrink>();
			if (delete != null)
			{
				yield return delete;
			}
			yield return new Command_Action
			{
				icon = TexButton.DeleteX,
				defaultLabel = "CommandDeleteZoneLabel".Translate(),
				defaultDesc = "CommandDeleteZoneDesc".Translate(),
				action = new Action(this.Delete),
				hotKey = KeyBindingDefOf.Misc3
			};
		}

		[DebuggerHidden]
		public virtual IEnumerable<Gizmo> GetZoneAddGizmos()
		{
		}

		public void CheckContiguous()
		{
			if (this.cells.Count == 0)
			{
				return;
			}
			if (Zone.extantGrid == null)
			{
				Zone.extantGrid = new BoolGrid(this.Map);
			}
			else
			{
				Zone.extantGrid.ClearAndResizeTo(this.Map);
			}
			if (Zone.foundGrid == null)
			{
				Zone.foundGrid = new BoolGrid(this.Map);
			}
			else
			{
				Zone.foundGrid.ClearAndResizeTo(this.Map);
			}
			for (int i = 0; i < this.cells.Count; i++)
			{
				Zone.extantGrid.Set(this.cells[i], true);
			}
			Predicate<IntVec3> passCheck = (IntVec3 c) => Zone.extantGrid[c] && !Zone.foundGrid[c];
			int numFound = 0;
			Action<IntVec3> processor = delegate(IntVec3 c)
			{
				Zone.foundGrid.Set(c, true);
				numFound++;
			};
			this.Map.floodFiller.FloodFill(this.cells[0], passCheck, processor, 2147483647, false, null);
			if (numFound < this.cells.Count)
			{
				foreach (IntVec3 current in this.Map.AllCells)
				{
					if (Zone.extantGrid[current] && !Zone.foundGrid[current])
					{
						this.RemoveCell(current);
					}
				}
			}
		}

		private void CheckAddHaulDestination()
		{
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				this.Map.haulDestinationManager.AddHaulDestination(haulDestination);
			}
		}

		public override string ToString()
		{
			return this.label;
		}

		public string GetUniqueLoadID()
		{
			return "Zone_" + this.ID;
		}
	}
}
