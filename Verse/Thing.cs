using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class Thing : Entity, ILoadReferenceable, IExposable, ISelectable
	{
		public ThingDef def;

		public int thingIDNumber = -1;

		private ThingState thingStateInt = ThingState.Unspawned;

		private IntVec3 positionInt = IntVec3.Invalid;

		private Rot4 rotationInt = Rot4.North;

		public int stackCount = 1;

		public ThingContainer holder;

		protected Faction factionInt;

		private ThingDef stuffInt;

		private Graphic graphicInt;

		private int hitPointsInt = -1;

		public static bool allowDestroyNonDestroyable;

		public virtual int HitPoints
		{
			get
			{
				return this.hitPointsInt;
			}
			set
			{
				this.hitPointsInt = value;
			}
		}

		public int MaxHitPoints
		{
			get
			{
				return Mathf.RoundToInt(this.GetStatValue(StatDefOf.MaxHitPoints, true));
			}
		}

		public float MarketValue
		{
			get
			{
				return this.GetStatValue(StatDefOf.MarketValue, true);
			}
		}

		public ThingState ThingState
		{
			get
			{
				return this.thingStateInt;
			}
		}

		public bool FlammableNow
		{
			get
			{
				if (this.GetStatValue(StatDefOf.Flammability, true) < 0.01f)
				{
					return false;
				}
				if (this.Spawned)
				{
					List<Thing> thingList = this.Position.GetThingList();
					if (thingList != null)
					{
						for (int i = 0; i < thingList.Count; i++)
						{
							if (thingList[i].def.Fillage == FillCategory.Full && thingList[i] != this)
							{
								return false;
							}
						}
					}
				}
				return true;
			}
		}

		public bool Destroyed
		{
			get
			{
				return this.thingStateInt == ThingState.Memory || this.thingStateInt == ThingState.Discarded;
			}
		}

		public bool Spawned
		{
			get
			{
				return this.thingStateInt == ThingState.Spawned;
			}
		}

		public IntVec3 Position
		{
			get
			{
				return this.positionInt;
			}
			set
			{
				if (value == this.positionInt)
				{
					return;
				}
				if (ListerThings.EverListable(this.def, ListerThingsUse.Region) && this.def.passability != Traversability.Impassable)
				{
					RegionGrid regionGrid = Find.RegionGrid;
					if (regionGrid != null)
					{
						Region region = null;
						if (this.positionInt.InBounds())
						{
							region = regionGrid.GetValidRegionAt(this.positionInt);
						}
						Region region2 = null;
						if (value.InBounds())
						{
							region2 = regionGrid.GetValidRegionAt(value);
						}
						if (region2 != region)
						{
							if (region != null)
							{
								region.ListerThings.Remove(this);
							}
							if (region2 != null)
							{
								region2.ListerThings.Add(this);
							}
						}
					}
				}
				ThingGrid thingGrid = Find.ThingGrid;
				if (thingGrid != null)
				{
					thingGrid.Deregister(this);
					this.positionInt = value;
					thingGrid.Register(this);
				}
			}
		}

		public IntVec3 PositionHeld
		{
			get
			{
				if (this.holder == null)
				{
					return this.Position;
				}
				if (this.holder.owner == null)
				{
					Log.Error(string.Concat(new object[]
					{
						"Holder of ",
						this,
						" is ",
						this.holder,
						" and it has a null owner."
					}));
					return this.Position;
				}
				return this.holder.owner.GetPosition();
			}
		}

		public Rot4 Rotation
		{
			get
			{
				return this.rotationInt;
			}
			set
			{
				if (value == this.rotationInt)
				{
					return;
				}
				this.rotationInt = value;
			}
		}

		public bool Smeltable
		{
			get
			{
				return !this.def.smeltProducts.NullOrEmpty<ThingCount>() || (this.def.MadeFromStuff && this.Stuff.stuffProps.smeltable);
			}
		}

		public Faction Faction
		{
			get
			{
				return this.factionInt;
			}
		}

		public string ThingID
		{
			get
			{
				if (this.def.HasThingIDNumber)
				{
					return this.def.defName + this.thingIDNumber.ToString();
				}
				return this.def.defName;
			}
			set
			{
				this.thingIDNumber = Thing.IDNumberFromThingID(value);
			}
		}

		public IntVec2 RotatedSize
		{
			get
			{
				if (!this.rotationInt.IsHorizontal)
				{
					return this.def.size;
				}
				return new IntVec2(this.def.size.z, this.def.size.x);
			}
		}

		public override string Label
		{
			get
			{
				if (this.stackCount != 1)
				{
					return this.LabelNoCount + " x" + this.stackCount.ToStringCached();
				}
				return this.LabelNoCount;
			}
		}

		public virtual string LabelNoCount
		{
			get
			{
				return GenLabel.ThingLabel(this.def, this.Stuff, 1);
			}
		}

		public override string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		public virtual string LabelCapNoCount
		{
			get
			{
				return this.LabelNoCount.CapitalizeFirst();
			}
		}

		public override string LabelShort
		{
			get
			{
				return this.LabelNoCount;
			}
		}

		public virtual bool IngestibleNow
		{
			get
			{
				return this.def.IsIngestible;
			}
		}

		public ThingDef Stuff
		{
			get
			{
				return this.stuffInt;
			}
		}

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats
		{
			get
			{
			}
		}

		public virtual Graphic Graphic
		{
			get
			{
				if (this.graphicInt == null)
				{
					if (this.def.graphicData == null)
					{
						Log.ErrorOnce(this.def + " has no graphicData but we are trying to access it.", 764532);
						return BaseContent.BadGraphic;
					}
					this.graphicInt = this.def.graphicData.GraphicColoredFor(this);
				}
				return this.graphicInt;
			}
		}

		public virtual IntVec3 InteractionCell
		{
			get
			{
				return Thing.InteractionCellWhenAt(this.def, this.Position, this.Rotation);
			}
		}

		public virtual Vector3 DrawPos
		{
			get
			{
				return this.TrueCenter();
			}
		}

		public virtual Color DrawColor
		{
			get
			{
				if (this.Stuff != null)
				{
					return this.Stuff.stuffProps.color;
				}
				if (this.def.graphicData != null)
				{
					return this.def.graphicData.color;
				}
				return Color.white;
			}
			set
			{
				Log.Error(string.Concat(new object[]
				{
					"Cannot set instance color on non-ThingWithComps ",
					this.LabelCap,
					" at ",
					this.Position,
					"."
				}));
			}
		}

		public virtual Color DrawColorTwo
		{
			get
			{
				if (this.def.graphicData != null)
				{
					return this.def.graphicData.colorTwo;
				}
				return Color.white;
			}
		}

		public static int IDNumberFromThingID(string thingID)
		{
			string value = Regex.Match(thingID, "\\d+$").Value;
			int result = 0;
			try
			{
				result = Convert.ToInt32(value);
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new string[]
				{
					"Could not convert id number from thingID=",
					thingID,
					", numString=",
					value,
					" Exception=",
					ex.ToString()
				}));
			}
			return result;
		}

		public virtual void PostMake()
		{
			ThingIDMaker.GiveIDTo(this);
			if (this.def.useHitPoints)
			{
				this.HitPoints = this.MaxHitPoints;
			}
		}

		public string GetUniqueLoadID()
		{
			return "Thing_" + this.ThingID;
		}

		public override void SpawnSetup()
		{
			if (this.Destroyed)
			{
				Log.Error(string.Concat(new object[]
				{
					"Spawning destroyed thing ",
					this,
					" at ",
					this.Position,
					". Correcting."
				}));
				this.thingStateInt = ThingState.Unspawned;
				if (this.HitPoints <= 0 && this.def.useHitPoints)
				{
					this.HitPoints = 1;
				}
			}
			if (this.Spawned)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to spawn already-spawned thing ",
					this,
					" at ",
					this.Position
				}));
				return;
			}
			if (this.stackCount > this.def.stackLimit)
			{
				Log.Error(string.Concat(new object[]
				{
					"Spawned ",
					this,
					" with stackCount ",
					this.stackCount,
					" but stackLimit is ",
					this.def.stackLimit,
					". Truncating."
				}));
				this.stackCount = this.def.stackLimit;
			}
			this.holder = null;
			this.thingStateInt = ThingState.Spawned;
			Find.Map.listerThings.Add(this);
			if (Find.TickManager != null)
			{
				Find.TickManager.RegisterAllTickabilityFor(this);
			}
			if (this.def.drawerType != DrawerType.RealtimeOnly)
			{
				CellRect.CellRectIterator iterator = this.OccupiedRect().GetIterator();
				while (!iterator.Done())
				{
					Find.Map.mapDrawer.MapMeshDirty(iterator.Current, MapMeshFlag.Things);
					iterator.MoveNext();
				}
			}
			if (this.def.drawerType != DrawerType.MapMeshOnly)
			{
				Find.DynamicDrawManager.RegisterDrawable(this);
			}
			if (this.def.hasTooltip)
			{
				Find.TooltipGiverList.RegisterTooltipGiver(this);
			}
			if (this.def.graphicData != null && this.def.graphicData.Linked)
			{
				LinkGrid.Notify_LinkerCreatedOrDestroyed(this);
				Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things, true, false);
			}
			if (!this.def.CanOverlapZones)
			{
				Find.ZoneManager.Notify_NoZoneOverlapThingSpawned(this);
			}
			if (this.def.regionBarrier)
			{
				RegionDirtyer.Notify_BarrierSpawned(this);
			}
			if (this.def.pathCost != 0 || this.def.passability == Traversability.Impassable)
			{
				Find.PathGrid.RecalculatePerceivedPathCostUnderThing(this);
			}
			if (this.def.passability == Traversability.Impassable)
			{
				Reachability.ClearCache();
			}
			Find.CoverGrid.Register(this);
			if (this.def.category == ThingCategory.Item)
			{
				ListerHaulables.Notify_Spawned(this);
			}
			Find.AttackTargetsCache.Notify_ThingSpawned(this);
			Region validRegionAt_NoRebuild = Find.RegionGrid.GetValidRegionAt_NoRebuild(this.Position);
			Room room = (validRegionAt_NoRebuild != null) ? validRegionAt_NoRebuild.Room : null;
			if (room != null)
			{
				room.Notify_ContainedThingSpawnedOrDespawned(this);
			}
			if (this.def.category == ThingCategory.Item)
			{
				Building_Door building_Door = this.Position.GetEdifice() as Building_Door;
				if (building_Door != null)
				{
					building_Door.Notify_ItemSpawnedOrDespawnedOnTop(this);
				}
			}
			StealAIDebugDrawer.Notify_ThingChanged(this);
		}

		public override void DeSpawn()
		{
			if (this.Destroyed)
			{
				Log.Error("Tried to despawn " + this + " which is already destroyed.");
				return;
			}
			if (!this.Spawned)
			{
				Log.Error("Tried to despawn " + this + " which is not spawned.");
				return;
			}
			Find.Map.listerThings.Remove(this);
			if (this.def.passability != Traversability.Impassable)
			{
				Region validRegionAt = Find.RegionGrid.GetValidRegionAt(this.Position);
				if (validRegionAt != null)
				{
					validRegionAt.ListerThings.Remove(this);
				}
				if (this.def.size == IntVec2.One && !this.Position.Walkable())
				{
					this.DeregisterInAdjacentRegions();
				}
			}
			Find.ThingGrid.Deregister(this);
			Find.CoverGrid.DeRegister(this);
			if (this.def.hasTooltip)
			{
				Find.TooltipGiverList.DeregisterTooltipGiver(this);
			}
			if (this.def.graphicData != null && this.def.graphicData.Linked)
			{
				LinkGrid.Notify_LinkerCreatedOrDestroyed(this);
				Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things, true, false);
			}
			Find.Selector.Deselect(this);
			if (this.def.drawerType != DrawerType.RealtimeOnly)
			{
				CellRect cellRect = this.OccupiedRect();
				for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
				{
					for (int j = cellRect.minX; j <= cellRect.maxX; j++)
					{
						Find.Map.mapDrawer.MapMeshDirty(new IntVec3(j, 0, i), MapMeshFlag.Things);
					}
				}
			}
			if (this.def.drawerType != DrawerType.MapMeshOnly)
			{
				Find.DynamicDrawManager.DeRegisterDrawable(this);
			}
			Region validRegionAt_NoRebuild = Find.RegionGrid.GetValidRegionAt_NoRebuild(this.Position);
			Room room = (validRegionAt_NoRebuild != null) ? validRegionAt_NoRebuild.Room : null;
			if (room != null)
			{
				room.Notify_ContainedThingSpawnedOrDespawned(this);
			}
			if (this.def.regionBarrier)
			{
				RegionDirtyer.Notify_BarrierDespawned(this);
			}
			if (this.def.pathCost != 0 || this.def.passability == Traversability.Impassable)
			{
				Find.PathGrid.RecalculatePerceivedPathCostUnderThing(this);
			}
			if (this.def.passability == Traversability.Impassable)
			{
				Reachability.ClearCache();
			}
			Find.TickManager.DeRegisterAllTickabilityFor(this);
			this.thingStateInt = ThingState.Unspawned;
			if (this.def.category == ThingCategory.Item)
			{
				ListerHaulables.Notify_DeSpawned(this);
			}
			Find.AttackTargetsCache.Notify_ThingDespawned(this);
			if (this.def.category == ThingCategory.Item)
			{
				Building_Door building_Door = this.Position.GetEdifice() as Building_Door;
				if (building_Door != null)
				{
					building_Door.Notify_ItemSpawnedOrDespawnedOnTop(this);
				}
			}
			StealAIDebugDrawer.Notify_ThingChanged(this);
		}

		public virtual void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (!Thing.allowDestroyNonDestroyable && !this.def.destroyable)
			{
				Log.Error("Tried to destroy non-destroyable thing " + this);
				return;
			}
			if (this.Destroyed)
			{
				Log.Error("Tried to destroy already-destroyed thing " + this);
				return;
			}
			bool spawned = this.Spawned;
			if (this.Spawned)
			{
				this.DeSpawn();
			}
			this.thingStateInt = ThingState.Memory;
			if (this.def.DiscardOnDestroyed)
			{
				this.Discard();
			}
			if (spawned)
			{
				GenLeaving.DoLeavingsFor(this, mode);
			}
			if (this.holder != null)
			{
				this.holder.Notify_ContainedItemDestroyed(this);
			}
			if (Find.Map != null)
			{
				Find.Reservations.ReleaseAllForTarget(this);
				Find.PhysicalInteractionReservations.ReleaseAllForTarget(this);
				IAttackTarget attackTarget = this as IAttackTarget;
				if (attackTarget != null)
				{
					Find.AttackTargetReservations.ReleaseAllForTarget(attackTarget);
				}
				if (this.def.category != ThingCategory.Mote)
				{
					Find.DesignationManager.RemoveAllDesignationsOn(this, false);
				}
			}
		}

		public virtual void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
		}

		private void DeregisterInAdjacentRegions()
		{
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = this.positionInt + GenAdj.AdjacentCells[i];
				if (c.InBounds())
				{
					Region validRegionAt = Find.RegionGrid.GetValidRegionAt(c);
					if (validRegionAt != null && validRegionAt.ListerThings.Contains(this))
					{
						validRegionAt.ListerThings.Remove(this);
					}
				}
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.LookDef<ThingDef>(ref this.def, "def");
			if (this.def.HasThingIDNumber)
			{
				string thingID = this.ThingID;
				Scribe_Values.LookValue<string>(ref thingID, "id", null, false);
				this.ThingID = thingID;
			}
			Scribe_Values.LookValue<IntVec3>(ref this.positionInt, "pos", IntVec3.Invalid, false);
			Scribe_Values.LookValue<Rot4>(ref this.rotationInt, "rot", Rot4.North, false);
			if (this.def.useHitPoints)
			{
				Scribe_Values.LookValue<int>(ref this.hitPointsInt, "health", -1, false);
			}
			bool flag = this.def.tradeability != Tradeability.Never && this.def.category == ThingCategory.Item;
			if (this.def.stackLimit > 1 || flag)
			{
				Scribe_Values.LookValue<int>(ref this.stackCount, "stackCount", 0, true);
			}
			Scribe_Defs.LookDef<ThingDef>(ref this.stuffInt, "stuff");
			string facID = (this.factionInt == null) ? "null" : this.factionInt.GetUniqueLoadID();
			Scribe_Values.LookValue<string>(ref facID, "faction", "null", false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.factionInt = Find.FactionManager.AllFactions.FirstOrDefault((Faction fa) => fa.GetUniqueLoadID() == facID);
			}
		}

		public virtual void PostMapInit()
		{
		}

		public virtual void Draw()
		{
			this.DrawAt(this.DrawPos);
		}

		public virtual void DrawAt(Vector3 drawLoc)
		{
			this.Graphic.Draw(drawLoc, this.Rotation, this);
		}

		public virtual void Print(SectionLayer layer)
		{
			this.Graphic.Print(layer, this);
		}

		public virtual void DrawGUIOverlay()
		{
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
			{
				QualityCategory cat;
				if (this.def.stackLimit > 1)
				{
					GenWorldUI.DrawThingLabel(this, this.stackCount.ToStringCached());
				}
				else if (this.TryGetQuality(out cat))
				{
					GenWorldUI.DrawThingLabel(this, cat.GetLabelShort());
				}
			}
		}

		public virtual void DrawExtraSelectionOverlays()
		{
			if (this.def.specialDisplayRadius > 0.1f)
			{
				GenDraw.DrawRadiusRing(this.Position, this.def.specialDisplayRadius);
			}
			if (this.def.drawPlaceWorkersWhileSelected && this.def.PlaceWorkers != null)
			{
				for (int i = 0; i < this.def.PlaceWorkers.Count; i++)
				{
					this.def.PlaceWorkers[i].DrawGhost(this.def, this.Position, this.Rotation);
				}
			}
			if (this.def.hasInteractionCell)
			{
				GenDraw.DrawInteractionCell(this.def, this.Position, this.rotationInt);
			}
		}

		public virtual string GetInspectString()
		{
			return string.Empty;
		}

		[DebuggerHidden]
		public virtual IEnumerable<Gizmo> GetGizmos()
		{
		}

		[DebuggerHidden]
		public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
		{
		}

		public void TakeDamage(DamageInfo dinfo)
		{
			if (this.Destroyed)
			{
				return;
			}
			if (dinfo.Amount == 0)
			{
				return;
			}
			if (this.def.damageMultipliers != null)
			{
				for (int i = 0; i < this.def.damageMultipliers.Count; i++)
				{
					if (this.def.damageMultipliers[i].damageDef == dinfo.Def)
					{
						int amount = Mathf.RoundToInt((float)dinfo.Amount * this.def.damageMultipliers[i].multiplier);
						dinfo.SetAmount(amount);
					}
				}
			}
			bool flag;
			this.PreApplyDamage(dinfo, out flag);
			if (flag)
			{
				return;
			}
			float num = dinfo.Def.Worker.Apply(dinfo, this);
			if (dinfo.Def.harmsHealth)
			{
				Find.StoryWatcher.watcherDamage.Notify_DamageTaken(this, num);
			}
			if (dinfo.Def.externalViolence)
			{
				GenLeaving.DropFilthDueToDamage(this, num);
				if (dinfo.Instigator != null)
				{
					Pawn pawn = dinfo.Instigator as Pawn;
					if (pawn != null)
					{
						pawn.records.AddTo(RecordDefOf.DamageDealt, num);
					}
				}
			}
			this.PostApplyDamage(dinfo, num);
		}

		public virtual void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
		}

		public virtual void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
		}

		public virtual bool CanStackWith(Thing other)
		{
			return this.def == other.def && this.Stuff == other.Stuff;
		}

		public virtual bool TryAbsorbStack(Thing other, bool respectStackLimit)
		{
			if (!this.CanStackWith(other))
			{
				return false;
			}
			int num = ThingUtility.TryAsborbStackNumToTake(this, other, respectStackLimit);
			if (this.def.useHitPoints)
			{
				this.HitPoints = Mathf.CeilToInt((float)(this.HitPoints * this.stackCount + other.HitPoints * num) / (float)(this.stackCount + num));
			}
			this.stackCount += num;
			other.stackCount -= num;
			StealAIDebugDrawer.Notify_ThingChanged(this);
			if (other.stackCount <= 0)
			{
				other.Destroy(DestroyMode.Vanish);
				return true;
			}
			return false;
		}

		public virtual Thing SplitOff(int count)
		{
			if (count <= 0)
			{
				throw new ArgumentException("SplitOff with count <= 0", "count");
			}
			if (count >= this.stackCount)
			{
				if (count > this.stackCount)
				{
					Log.Error(string.Concat(new object[]
					{
						"Tried to split off ",
						count,
						" of ",
						this,
						" but there are only ",
						this.stackCount
					}));
				}
				if (this.Spawned)
				{
					this.DeSpawn();
				}
				return this;
			}
			Thing thing = ThingMaker.MakeThing(this.def, this.Stuff);
			thing.stackCount = count;
			this.stackCount -= count;
			if (this.def.useHitPoints)
			{
				thing.HitPoints = this.HitPoints;
			}
			return thing;
		}

		public virtual void Notify_ColorChanged()
		{
			this.graphicInt = null;
		}

		public virtual TipSignal GetTooltip()
		{
			string text = this.LabelCap;
			if (this.def.useHitPoints)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					"\n",
					this.HitPoints,
					" / ",
					this.MaxHitPoints
				});
			}
			return new TipSignal(text, this.thingIDNumber * 251235);
		}

		public virtual bool BlocksPawn(Pawn p)
		{
			return this.def.passability == Traversability.Impassable;
		}

		public void SetFactionDirect(Faction newFaction)
		{
			if (!this.def.CanHaveFaction)
			{
				Log.Error("Tried to SetFactionDirect on " + this + " which cannot have a faction.");
				return;
			}
			this.factionInt = newFaction;
		}

		public virtual void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			if (!this.def.CanHaveFaction)
			{
				Log.Error("Tried to SetFaction on " + this + " which cannot have a faction.");
				return;
			}
			this.factionInt = newFaction;
			IAttackTarget attackTarget = this as IAttackTarget;
			if (attackTarget != null)
			{
				Find.AttackTargetsCache.UpdateTarget(attackTarget);
			}
		}

		public void SetPositionDirect(IntVec3 newPos)
		{
			this.positionInt = newPos;
		}

		public void SetStuffDirect(ThingDef newStuff)
		{
			this.stuffInt = newStuff;
		}

		public virtual string GetDescription()
		{
			return this.def.description;
		}

		public override string ToString()
		{
			if (this.def != null)
			{
				return this.ThingID;
			}
			return base.GetType().ToString();
		}

		public override int GetHashCode()
		{
			return this.thingIDNumber;
		}

		public virtual void Discard()
		{
			if (this.thingStateInt != ThingState.Memory)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to discard ",
					this,
					" whose state is ",
					this.thingStateInt,
					"."
				}));
				return;
			}
			this.thingStateInt = ThingState.Discarded;
		}

		[DebuggerHidden]
		public virtual IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
		{
			if (this.def.butcherProducts != null)
			{
				for (int i = 0; i < this.def.butcherProducts.Count; i++)
				{
					ThingCount ta = this.def.butcherProducts[i];
					Thing t = ThingMaker.MakeThing(ta.thingDef, null);
					t.stackCount = ta.count;
					yield return t;
				}
			}
		}

		[DebuggerHidden]
		public virtual IEnumerable<Thing> SmeltProducts(float efficiency)
		{
			List<ThingCount> costListAdj = this.def.CostListAdjusted(this.Stuff, true);
			for (int i = 0; i < costListAdj.Count; i++)
			{
				if (!costListAdj[i].thingDef.intricate)
				{
					float countF = (float)costListAdj[i].count * 0.25f;
					int count = GenMath.RoundRandom(countF);
					if (count > 0)
					{
						Thing t = ThingMaker.MakeThing(costListAdj[i].thingDef, null);
						t.stackCount = count;
						yield return t;
					}
				}
			}
			if (this.def.smeltProducts != null)
			{
				for (int j = 0; j < this.def.smeltProducts.Count; j++)
				{
					ThingCount ta = this.def.smeltProducts[j];
					Thing t2 = ThingMaker.MakeThing(ta.thingDef, null);
					t2.stackCount = ta.count;
					yield return t2;
				}
			}
		}

		public float Ingested(Pawn ingester, float nutritionWanted)
		{
			if (this.Destroyed)
			{
				Log.Error(ingester + " ingested destroyed thing " + this);
				return 0f;
			}
			if (!this.IngestibleNow)
			{
				Log.Error(ingester + " ingested IngestibleNow=false thing " + this);
				return 0f;
			}
			ingester.mindState.lastIngestTick = Find.TickManager.TicksGame;
			if (this.def.ingestible.outcomeDoers != null)
			{
				for (int i = 0; i < this.def.ingestible.outcomeDoers.Count; i++)
				{
					this.def.ingestible.outcomeDoers[i].DoIngestionOutcome(ingester, this);
				}
			}
			if (ingester.needs.mood != null)
			{
				List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(ingester, this);
				for (int j = 0; j < list.Count; j++)
				{
					ingester.needs.mood.thoughts.memories.TryGainMemoryThought(list[j], null);
				}
			}
			if (ingester.IsColonist && FoodUtility.IsHumanlikeMeatOrHumanlikeCorpse(this))
			{
				TaleRecorder.RecordTale(TaleDefOf.AteRawHumanlikeMeat, new object[]
				{
					ingester
				});
			}
			int num;
			float result;
			this.IngestedCalculateAmounts(ingester, nutritionWanted, out num, out result);
			if (!ingester.Dead && ingester.needs.joy != null && Mathf.Abs(this.def.ingestible.joy) > 0.0001f && num > 0)
			{
				JoyKindDef joyKind = (this.def.ingestible.joyKind == null) ? JoyKindDefOf.Gluttonous : this.def.ingestible.joyKind;
				ingester.needs.joy.GainJoy((float)num * this.def.ingestible.joy, joyKind);
			}
			if (num > 0)
			{
				if (num == this.stackCount)
				{
					this.Destroy(DestroyMode.Vanish);
				}
				else
				{
					this.SplitOff(num);
				}
			}
			this.PostIngested(ingester);
			return result;
		}

		protected virtual void PostIngested(Pawn ingester)
		{
		}

		protected virtual void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
		{
			numTaken = Mathf.CeilToInt(nutritionWanted / this.def.ingestible.nutrition);
			numTaken = Mathf.Min(new int[]
			{
				numTaken,
				this.def.ingestible.maxNumToIngestAtOnce,
				this.stackCount
			});
			numTaken = Mathf.Max(numTaken, 1);
			nutritionIngested = (float)numTaken * this.def.ingestible.nutrition;
		}

		public static IntVec3 InteractionCellWhenAt(ThingDef def, IntVec3 center, Rot4 rot)
		{
			if (def.hasInteractionCell)
			{
				IntVec3 b = def.interactionCellOffset.RotatedBy(rot);
				return center + b;
			}
			if (def.Size.x == 1 && def.Size.z == 1)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = center + GenAdj.AdjacentCells[i];
					if (intVec.Standable())
					{
						return intVec;
					}
				}
				for (int j = 0; j < 8; j++)
				{
					IntVec3 intVec2 = center + GenAdj.AdjacentCells[j];
					if (intVec2.Walkable())
					{
						return intVec2;
					}
				}
				return center;
			}
			List<IntVec3> list = GenAdjFast.AdjacentCells8Way(center);
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].Standable())
				{
					return list[k];
				}
			}
			for (int l = 0; l < list.Count; l++)
			{
				if (list[l].Walkable())
				{
					return list[l];
				}
			}
			return center;
		}

		public virtual bool PreventPlayerSellingThingsNearby(out string reason)
		{
			reason = null;
			return false;
		}
	}
}
