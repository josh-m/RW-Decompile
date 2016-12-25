using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldObject : ILoadReferenceable, IExposable, ISelectable
	{
		private const float BaseDrawSize = 0.7f;

		public WorldObjectDef def;

		public int ID = -1;

		private int tileInt = -1;

		private Faction factionInt;

		public int creationGameTicks = -1;

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		public int Tile
		{
			get
			{
				return this.tileInt;
			}
			set
			{
				if (this.tileInt != value)
				{
					int tile = this.tileInt;
					this.tileInt = value;
					if (this.Spawned)
					{
						if (!this.def.useDynamicDrawer)
						{
							Find.World.renderer.Notify_StaticWorldObjectPosChanged();
						}
						if (this.def.AffectsPathing)
						{
							WorldPathGrid worldPathGrid = Find.WorldPathGrid;
							worldPathGrid.RecalculatePerceivedPathCostAt(tile);
							worldPathGrid.RecalculatePerceivedPathCostAt(this.tileInt);
						}
					}
					this.PositionChanged();
				}
			}
		}

		public bool Spawned
		{
			get
			{
				return Find.WorldObjects.Contains(this);
			}
		}

		public virtual Vector3 DrawPos
		{
			get
			{
				return Find.WorldGrid.GetTileCenter(this.Tile);
			}
		}

		public Faction Faction
		{
			get
			{
				return this.factionInt;
			}
		}

		public virtual string Label
		{
			get
			{
				return this.def.label;
			}
		}

		public string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		public virtual string LabelShort
		{
			get
			{
				return this.Label;
			}
		}

		public virtual string LabelShortCap
		{
			get
			{
				return this.LabelShort.CapitalizeFirst();
			}
		}

		public virtual Material Material
		{
			get
			{
				return this.def.Material;
			}
		}

		public virtual bool SelectableNow
		{
			get
			{
				return this.def.selectable;
			}
		}

		public virtual bool NeverMultiSelect
		{
			get
			{
				return this.def.neverMultiSelect;
			}
		}

		public virtual Texture2D ExpandingIcon
		{
			get
			{
				return this.def.ExpandingIconTexture ?? ((Texture2D)this.Material.mainTexture);
			}
		}

		public virtual Color ExpandingIconColor
		{
			get
			{
				return this.Material.color;
			}
		}

		public virtual float ExpandingIconPriority
		{
			get
			{
				return this.def.expandingIconPriority;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.LookDef<WorldObjectDef>(ref this.def, "def");
			Scribe_Values.LookValue<int>(ref this.ID, "ID", -1, false);
			Scribe_Values.LookValue<int>(ref this.tileInt, "tile", -1, false);
			Scribe_References.LookReference<Faction>(ref this.factionInt, "faction", false);
			Scribe_Values.LookValue<int>(ref this.creationGameTicks, "creationGameTicks", 0, false);
		}

		public virtual void SetFaction(Faction newFaction)
		{
			if (!this.def.canHaveFaction && newFaction != null)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set faction to ",
					newFaction,
					" but this world object (",
					this,
					") cannot have faction."
				}));
				return;
			}
			this.factionInt = newFaction;
		}

		public virtual string GetInspectString()
		{
			if (this.Faction != null)
			{
				return "Faction".Translate() + ": " + this.Faction.Name;
			}
			return string.Empty;
		}

		public virtual void Tick()
		{
		}

		public virtual void ExtraSelectionOverlaysOnGUI()
		{
		}

		public virtual void DrawExtraSelectionOverlays()
		{
		}

		public virtual void PostMake()
		{
		}

		public virtual void PostAdd()
		{
		}

		protected virtual void PositionChanged()
		{
		}

		public virtual void SpawnSetup()
		{
			if (!this.def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (this.def.AffectsPathing)
			{
				Find.WorldPathGrid.RecalculatePerceivedPathCostAt(this.Tile);
			}
			if (this.def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.RegisterDrawable(this);
			}
		}

		public virtual void PostRemove()
		{
			if (!this.def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (this.def.AffectsPathing)
			{
				Find.WorldPathGrid.RecalculatePerceivedPathCostAt(this.Tile);
			}
			if (this.def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.DeRegisterDrawable(this);
			}
			Find.WorldSelector.Deselect(this);
		}

		public virtual void Print(LayerSubMesh subMesh)
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			WorldRendererUtility.PrintQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 0.008f, subMesh, false, true, true);
		}

		public virtual void Draw()
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
			if (this.def.expandingIcon && transitionPct > 0f)
			{
				Color color = this.Material.color;
				float num = 1f - transitionPct;
				WorldObject.propertyBlock.SetColor(WorldRendererUtility.ColorPropertyID, new Color(color.r, color.g, color.b, color.a * num));
				MaterialPropertyBlock materialPropertyBlock = WorldObject.propertyBlock;
				WorldRendererUtility.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 0.008f, this.Material, false, false, materialPropertyBlock);
			}
			else
			{
				WorldRendererUtility.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 0.008f, this.Material, false, false, null);
			}
		}

		[DebuggerHidden]
		public virtual IEnumerable<Gizmo> GetGizmos()
		{
		}

		[DebuggerHidden]
		public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
		}

		public virtual IEnumerable<InspectTabBase> GetInspectTabs()
		{
			if (this.def.inspectorTabsResolved != null)
			{
				return this.def.inspectorTabsResolved;
			}
			return Enumerable.Empty<InspectTabBase>();
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				base.GetType(),
				" ",
				this.LabelCap,
				" (tile=",
				this.Tile,
				")"
			});
		}

		public override int GetHashCode()
		{
			return this.ID;
		}

		public string GetUniqueLoadID()
		{
			return "WorldObject_" + this.ID;
		}
	}
}
