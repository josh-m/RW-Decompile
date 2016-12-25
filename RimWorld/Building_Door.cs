using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class Building_Door : Building
	{
		private const float BaseDoorOpenTicks = 45f;

		private const int AutomaticCloseDelayTicks = 60;

		private const int ApproachCloseDelayTicks = 300;

		private const float VisualDoorOffsetStart = 0f;

		private const float VisualDoorOffsetEnd = 0.45f;

		public CompPowerTrader powerComp;

		private bool openInt;

		private bool holdOpenInt;

		private bool freePassageUntilClosed;

		private int lastFriendlyTouchTick = -9999;

		protected int ticksUntilClose;

		protected int visualTicksOpen;

		public bool Open
		{
			get
			{
				return this.openInt;
			}
		}

		public bool HoldOpen
		{
			get
			{
				return this.holdOpenInt;
			}
		}

		public bool FreePassage
		{
			get
			{
				return this.openInt && (this.holdOpenInt || this.freePassageUntilClosed || this.BlockedOpenLongTerm);
			}
		}

		public bool BlockedOpenMomentary
		{
			get
			{
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Pawn)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool BlockedOpenLongTerm
		{
			get
			{
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.category == ThingCategory.Item)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool DoorPowerOn
		{
			get
			{
				return this.powerComp != null && this.powerComp.PowerOn;
			}
		}

		public bool SlowsPawns
		{
			get
			{
				return !this.DoorPowerOn || this.TicksToOpenNow > 20;
			}
		}

		public int TicksToOpenNow
		{
			get
			{
				float num = 45f / this.GetStatValue(StatDefOf.DoorOpenSpeed, true);
				if (this.DoorPowerOn)
				{
					num *= 0.25f;
				}
				return Mathf.RoundToInt(num);
			}
		}

		private int VisualTicksToOpen
		{
			get
			{
				return this.TicksToOpenNow;
			}
		}

		public override void PostMake()
		{
			base.PostMake();
			this.powerComp = base.GetComp<CompPowerTrader>();
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			this.powerComp = base.GetComp<CompPowerTrader>();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<bool>(ref this.openInt, "open", false, false);
			Scribe_Values.LookValue<bool>(ref this.holdOpenInt, "holdOpen", false, false);
			Scribe_Values.LookValue<bool>(ref this.freePassageUntilClosed, "freePassageUntilClosed", false, false);
			Scribe_Values.LookValue<int>(ref this.lastFriendlyTouchTick, "lastFriendlyTouchTick", 0, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars && this.openInt)
			{
				this.visualTicksOpen = this.VisualTicksToOpen;
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (!this.openInt)
			{
				if (this.visualTicksOpen > 0)
				{
					this.visualTicksOpen--;
				}
				if ((Find.TickManager.TicksGame + this.thingIDNumber.HashOffset()) % 375 == 0)
				{
					GenTemperature.EqualizeTemperaturesThroughBuilding(this, 1f, false);
				}
			}
			else if (this.openInt)
			{
				if (this.visualTicksOpen < this.VisualTicksToOpen)
				{
					this.visualTicksOpen++;
				}
				if (!this.holdOpenInt)
				{
					if (base.Map.thingGrid.CellContains(base.Position, ThingCategory.Pawn))
					{
						this.ticksUntilClose = 60;
					}
					else
					{
						this.ticksUntilClose--;
						if (this.DoorPowerOn && this.ticksUntilClose <= 0 && Find.TickManager.TicksGame < this.lastFriendlyTouchTick + 200)
						{
							this.DoorTryClose();
						}
					}
				}
				if ((Find.TickManager.TicksGame + this.thingIDNumber.HashOffset()) % 22 == 0)
				{
					GenTemperature.EqualizeTemperaturesThroughBuilding(this, 1f, false);
				}
			}
		}

		public void FriendlyTouched()
		{
			this.lastFriendlyTouchTick = Find.TickManager.TicksGame;
		}

		public void Notify_PawnApproaching(Pawn p)
		{
			if (this.PawnCanOpen(p) && !this.SlowsPawns)
			{
				this.DoorOpen(300);
			}
		}

		public bool CanPhysicallyPass(Pawn p)
		{
			return this.FreePassage || this.PawnCanOpen(p);
		}

		public virtual bool PawnCanOpen(Pawn p)
		{
			Lord lord = p.GetLord();
			return (lord != null && lord.LordJob != null && lord.LordJob.CanOpenAnyDoor(p)) || base.Faction == null || GenAI.MachinesLike(base.Faction, p);
		}

		public override bool BlocksPawn(Pawn p)
		{
			return !this.openInt && !this.PawnCanOpen(p);
		}

		protected void DoorOpen(int ticksToClose = 60)
		{
			this.ticksUntilClose = ticksToClose;
			if (!this.openInt)
			{
				this.openInt = true;
				if (this.holdOpenInt)
				{
					this.freePassageUntilClosed = true;
					if (base.Spawned)
					{
						base.Map.reachability.ClearCache();
					}
				}
				if (this.DoorPowerOn)
				{
					this.def.building.soundDoorOpenPowered.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
				}
				else
				{
					this.def.building.soundDoorOpenManual.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
				}
			}
		}

		protected void DoorTryClose()
		{
			if (this.holdOpenInt || this.BlockedOpenMomentary)
			{
				return;
			}
			this.openInt = false;
			if (this.freePassageUntilClosed)
			{
				this.freePassageUntilClosed = false;
				if (base.Spawned)
				{
					base.Map.reachability.ClearCache();
				}
			}
			if (this.DoorPowerOn)
			{
				this.def.building.soundDoorClosePowered.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			}
			else
			{
				this.def.building.soundDoorCloseManual.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			}
		}

		public void StartManualOpenBy(Pawn opener)
		{
			this.DoorOpen(60);
		}

		public void StartManualCloseBy(Pawn closer)
		{
			this.DoorTryClose();
		}

		public override void Draw()
		{
			base.Rotation = Building_Door.DoorRotationAt(base.Position, base.Map);
			float num = Mathf.Clamp01((float)this.visualTicksOpen / (float)this.VisualTicksToOpen);
			float d = 0f + 0.45f * num;
			for (int i = 0; i < 2; i++)
			{
				Vector3 vector = default(Vector3);
				Mesh mesh;
				if (i == 0)
				{
					vector = new Vector3(0f, 0f, -1f);
					mesh = MeshPool.plane10;
				}
				else
				{
					vector = new Vector3(0f, 0f, 1f);
					mesh = MeshPool.plane10Flip;
				}
				Rot4 rotation = base.Rotation;
				rotation.Rotate(RotationDirection.Clockwise);
				vector = rotation.AsQuat * vector;
				Vector3 vector2 = this.DrawPos;
				vector2.y = Altitudes.AltitudeFor(AltitudeLayer.DoorMoveable);
				vector2 += vector * d;
				Graphics.DrawMesh(mesh, vector2, base.Rotation.AsQuat, this.Graphic.MatAt(base.Rotation, null), 0);
			}
			base.Comps_PostDraw();
		}

		private static int AlignQualityAgainst(IntVec3 c, Map map)
		{
			if (!c.InBounds(map))
			{
				return 0;
			}
			if (!c.Walkable(map))
			{
				return 9;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
				{
					return 1;
				}
				Thing thing2 = thing as Blueprint;
				if (thing2 != null)
				{
					if (thing2.def.entityDefToBuild.passability == Traversability.Impassable)
					{
						return 9;
					}
					if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
					{
						return 1;
					}
				}
			}
			return 0;
		}

		public static Rot4 DoorRotationAt(IntVec3 loc, Map map)
		{
			int num = 0;
			int num2 = 0;
			num += Building_Door.AlignQualityAgainst(loc + IntVec3.East, map);
			num += Building_Door.AlignQualityAgainst(loc + IntVec3.West, map);
			num2 += Building_Door.AlignQualityAgainst(loc + IntVec3.North, map);
			num2 += Building_Door.AlignQualityAgainst(loc + IntVec3.South, map);
			if (num >= num2)
			{
				return Rot4.North;
			}
			return Rot4.East;
		}

		public void Notify_ItemSpawnedOrDespawnedOnTop(Thing t)
		{
			base.Map.reachability.ClearCache();
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "CommandToggleDoorHoldOpen".Translate(),
					defaultDesc = "CommandToggleDoorHoldOpenDesc".Translate(),
					hotKey = KeyBindingDefOf.Misc3,
					icon = TexCommand.HoldOpen,
					isActive = (() => this.<>f__this.holdOpenInt),
					toggleAction = delegate
					{
						this.<>f__this.holdOpenInt = !this.<>f__this.holdOpenInt;
						this.<>f__this.Map.reachability.ClearCache();
						if (this.<>f__this.Open && this.<>f__this.holdOpenInt)
						{
							this.<>f__this.freePassageUntilClosed = true;
						}
					}
				};
			}
		}
	}
}
