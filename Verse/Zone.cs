using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Zone : IExposable, ISelectable
	{
		private const int StaticFireCheckInterval = 1000;

		public string label;

		public List<IntVec3> cells = new List<IntVec3>();

		private bool cellsShuffled;

		public Color color = Color.white;

		private Material materialInt;

		public bool hidden;

		private int lastStaticFireCheckTick = -9999;

		private bool lastStaticFireCheckResult;

		private static BoolGrid extantGrid = new BoolGrid();

		private static BoolGrid foundGrid = new BoolGrid();

		public Material Material
		{
			get
			{
				if (this.materialInt == null)
				{
					this.materialInt = SolidColorMaterials.SimpleSolidColorMaterial(this.color);
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
				ThingGrid grids = Find.ThingGrid;
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
						if (this.cells[i].ContainsStaticFire())
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

		public Zone(string baseName)
		{
			this.label = Find.ZoneManager.NewZoneName(baseName);
			this.color = this.NextZoneColor;
			Find.ZoneManager.RegisterZone(this);
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
			Scribe_Values.LookValue<string>(ref this.label, "label", null, false);
			Scribe_Values.LookValue<Color>(ref this.color, "color", default(Color), false);
			Scribe_Values.LookValue<bool>(ref this.hidden, "hidden", false, false);
			Scribe_Collections.LookList<IntVec3>(ref this.cells, "cells", LookMode.Undefined, new object[0]);
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
				}));
				return;
			}
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (!thing.def.CanOverlapZones)
				{
					Log.Error("Added zone over zone-incompatible thing " + thing);
					return;
				}
			}
			this.cells.Add(c);
			Find.ZoneManager.AddZoneGridCell(this, c);
			Find.MapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
			AutoHomeAreaMaker.Notify_ZoneCellAdded(c);
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
				}));
				return;
			}
			this.cells.Remove(c);
			Find.ZoneManager.ClearZoneGridCell(c);
			Find.MapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
			this.cellsShuffled = false;
			if (this.cells.Count == 0)
			{
				this.Deregister();
			}
		}

		public virtual void Delete()
		{
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
			SoundDefOf.DesignateZoneDelete.PlayOneShotOnCamera();
		}

		public virtual void Deregister()
		{
			Find.ZoneManager.DeregisterZone(this);
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
		public virtual IEnumerable<ITab> GetInspectionTabs()
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
					Find.WindowStack.Add(new Dialog_RenameZone(this.<>f__this));
				},
				hotKey = KeyBindingDefOf.Misc1
			};
			yield return new Command_Toggle
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone", true),
				defaultLabel = ((!this.hidden) ? "CommandHideZoneLabel".Translate() : "CommandUnhideZoneLabel".Translate()),
				defaultDesc = "CommandHideZoneDesc".Translate(),
				isActive = (() => this.<>f__this.hidden),
				toggleAction = delegate
				{
					this.<>f__this.hidden = !this.<>f__this.hidden;
					foreach (IntVec3 current in this.<>f__this.Cells)
					{
						Find.MapDrawer.MapMeshDirty(current, MapMeshFlag.Zone);
					}
				},
				hotKey = KeyBindingDefOf.Misc2
			};
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true),
				defaultLabel = "CommandDeleteZoneLabel".Translate(),
				defaultDesc = "CommandDeleteZoneDesc".Translate(),
				action = new Action(this.Delete),
				hotKey = KeyBindingDefOf.Misc3
			};
		}

		public void CheckContiguous()
		{
			if (this.cells.Count == 0)
			{
				return;
			}
			if (Zone.extantGrid.InnerArray.Length != Find.Map.Area)
			{
				Zone.extantGrid = new BoolGrid();
			}
			Zone.extantGrid.Clear();
			if (Zone.foundGrid.InnerArray.Length != Find.Map.Area)
			{
				Zone.foundGrid = new BoolGrid();
			}
			Zone.foundGrid.Clear();
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
			FloodFiller.FloodFill(this.cells[0], passCheck, processor);
			if (numFound < this.cells.Count)
			{
				foreach (IntVec3 current in Find.Map.AllCells)
				{
					if (Zone.extantGrid[current] && !Zone.foundGrid[current])
					{
						this.RemoveCell(current);
					}
				}
			}
		}

		public override string ToString()
		{
			return this.label;
		}
	}
}
