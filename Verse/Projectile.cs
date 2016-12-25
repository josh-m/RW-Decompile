using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Projectile : ThingWithComps
	{
		private const float BasePawnInterceptChance = 0.4f;

		private const float PawnInterceptChanceFactor_LayingDown = 0.1f;

		private const float PawnInterceptChanceFactor_NonWildNonEnemy = 0.4f;

		private const float InterceptChanceOnRandomObjectPerFillPercent = 0.07f;

		private const float InterceptDist_Possible = 4f;

		private const float InterceptDist_Short = 7f;

		private const float InterceptDist_Normal = 10f;

		private const float InterceptChanceFactor_VeryShort = 0.5f;

		private const float InterceptChanceFactor_Short = 0.75f;

		protected Vector3 origin;

		protected Vector3 destination;

		protected Thing assignedTarget;

		private bool interceptWallsInt = true;

		private bool freeInterceptInt = true;

		protected ThingDef equipmentDef;

		protected Thing launcher;

		private Thing neverInterceptTargetInt;

		protected bool landed;

		protected int ticksToImpact;

		private Sustainer ambientSustainer;

		private static List<IntVec3> checkedCells = new List<IntVec3>();

		private static readonly List<Thing> cellThingsFiltered = new List<Thing>();

		public bool InterceptWalls
		{
			get
			{
				return this.def.projectile.alwaysFreeIntercept || (!this.def.projectile.flyOverhead && this.interceptWallsInt);
			}
			set
			{
				if (!value && this.def.projectile.alwaysFreeIntercept)
				{
					Log.Error("Tried to set interceptWalls to false on projectile with alwaysFreeIntercept=true");
					return;
				}
				if (value && this.def.projectile.flyOverhead)
				{
					Log.Error("Tried to set interceptWalls to true on a projectile with flyOverhead=true");
					return;
				}
				this.interceptWallsInt = value;
				if (!this.interceptWallsInt && this is Projectile_Explosive)
				{
					Log.Message("Non interceptWallsInt explosive.");
				}
			}
		}

		public bool FreeIntercept
		{
			get
			{
				return this.def.projectile.alwaysFreeIntercept || (!this.def.projectile.flyOverhead && this.freeInterceptInt);
			}
			set
			{
				if (!value && this.def.projectile.alwaysFreeIntercept)
				{
					Log.Error("Tried to set FreeIntercept to false on projectile with alwaysFreeIntercept=true");
					return;
				}
				if (value && this.def.projectile.flyOverhead)
				{
					Log.Error("Tried to set FreeIntercept to true on a projectile with flyOverhead=true");
					return;
				}
				this.freeInterceptInt = value;
			}
		}

		public Thing ThingToNeverIntercept
		{
			get
			{
				return this.neverInterceptTargetInt;
			}
			set
			{
				if (value.def.Fillage == FillCategory.Full)
				{
					return;
				}
				this.neverInterceptTargetInt = value;
			}
		}

		protected int StartingTicksToImpact
		{
			get
			{
				int num = Mathf.RoundToInt((this.origin - this.destination).magnitude / (this.def.projectile.speed / 100f));
				if (num < 1)
				{
					num = 1;
				}
				return num;
			}
		}

		protected IntVec3 DestinationCell
		{
			get
			{
				return new IntVec3(this.destination);
			}
		}

		public virtual Vector3 ExactPosition
		{
			get
			{
				Vector3 b = (this.destination - this.origin) * (1f - (float)this.ticksToImpact / (float)this.StartingTicksToImpact);
				return this.origin + b + Vector3.up * this.def.Altitude;
			}
		}

		public virtual Quaternion ExactRotation
		{
			get
			{
				return Quaternion.LookRotation(this.destination - this.origin);
			}
		}

		public override Vector3 DrawPos
		{
			get
			{
				return this.ExactPosition;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<Vector3>(ref this.origin, "origin", default(Vector3), false);
			Scribe_Values.LookValue<Vector3>(ref this.destination, "destination", default(Vector3), false);
			Scribe_Values.LookValue<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
			Scribe_References.LookReference<Thing>(ref this.assignedTarget, "assignedTarget", false);
			Scribe_References.LookReference<Thing>(ref this.launcher, "launcher", false);
			Scribe_Defs.LookDef<ThingDef>(ref this.equipmentDef, "equipmentDef");
			Scribe_Values.LookValue<bool>(ref this.interceptWallsInt, "interceptWalls", true, false);
			Scribe_Values.LookValue<bool>(ref this.freeInterceptInt, "interceptRandomTargets", true, false);
			Scribe_Values.LookValue<bool>(ref this.landed, "landed", false, false);
			Scribe_References.LookReference<Thing>(ref this.neverInterceptTargetInt, "neverInterceptTarget", false);
		}

		public void Launch(Thing launcher, LocalTargetInfo targ, Thing equipment = null)
		{
			this.Launch(launcher, base.Position.ToVector3Shifted(), targ, equipment);
		}

		public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing equipment = null)
		{
			this.launcher = launcher;
			this.origin = origin;
			if (equipment != null)
			{
				this.equipmentDef = equipment.def;
			}
			else
			{
				this.equipmentDef = null;
			}
			if (targ.Thing != null)
			{
				this.assignedTarget = targ.Thing;
			}
			this.destination = targ.Cell.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
			this.ticksToImpact = this.StartingTicksToImpact;
			if (!this.def.projectile.soundAmbient.NullOrUndefined())
			{
				SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
				this.ambientSustainer = this.def.projectile.soundAmbient.TrySpawnSustainer(info);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (this.landed)
			{
				return;
			}
			Vector3 exactPosition = this.ExactPosition;
			this.ticksToImpact--;
			if (!this.ExactPosition.InBounds(base.Map))
			{
				this.ticksToImpact++;
				base.Position = this.ExactPosition.ToIntVec3();
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			Vector3 exactPosition2 = this.ExactPosition;
			if (this.CheckForFreeInterceptBetween(exactPosition, exactPosition2))
			{
				return;
			}
			base.Position = this.ExactPosition.ToIntVec3();
			if (this.ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && this.def.projectile.soundImpactAnticipate != null)
			{
				this.def.projectile.soundImpactAnticipate.PlayOneShot(this);
			}
			if (this.ticksToImpact <= 0)
			{
				if (this.DestinationCell.InBounds(base.Map))
				{
					base.Position = this.DestinationCell;
				}
				this.ImpactSomething();
				return;
			}
			if (this.ambientSustainer != null)
			{
				this.ambientSustainer.Maintain();
			}
		}

		private bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
		{
			IntVec3 intVec = lastExactPos.ToIntVec3();
			IntVec3 intVec2 = newExactPos.ToIntVec3();
			if (intVec2 == intVec)
			{
				return false;
			}
			if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
			{
				return false;
			}
			if (intVec2.AdjacentToCardinal(intVec))
			{
				bool flag = this.CheckForFreeIntercept(intVec2);
				if (DebugViewSettings.drawInterceptChecks)
				{
					if (flag)
					{
						MoteMaker.ThrowText(intVec2.ToVector3Shifted(), base.Map, "x", -1f);
					}
					else
					{
						MoteMaker.ThrowText(intVec2.ToVector3Shifted(), base.Map, "o", -1f);
					}
				}
				return flag;
			}
			if (this.origin.ToIntVec3().DistanceToSquared(intVec2) > 16f)
			{
				Vector3 vector = lastExactPos;
				Vector3 v = newExactPos - lastExactPos;
				Vector3 b = v.normalized * 0.2f;
				int num = (int)(v.MagnitudeHorizontal() / 0.2f);
				Projectile.checkedCells.Clear();
				int num2 = 0;
				while (true)
				{
					vector += b;
					IntVec3 intVec3 = vector.ToIntVec3();
					if (!Projectile.checkedCells.Contains(intVec3))
					{
						if (this.CheckForFreeIntercept(intVec3))
						{
							break;
						}
						Projectile.checkedCells.Add(intVec3);
					}
					if (DebugViewSettings.drawInterceptChecks)
					{
						MoteMaker.ThrowText(vector, base.Map, "o", -1f);
					}
					num2++;
					if (num2 > num)
					{
						return false;
					}
					if (intVec3 == intVec2)
					{
						return false;
					}
				}
				if (DebugViewSettings.drawInterceptChecks)
				{
					MoteMaker.ThrowText(vector, base.Map, "x", -1f);
				}
				return true;
			}
			return false;
		}

		private bool CheckForFreeIntercept(IntVec3 c)
		{
			float num = (c.ToVector3Shifted() - this.origin).MagnitudeHorizontalSquared();
			if (num < 16f)
			{
				return false;
			}
			List<Thing> list = base.Map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing != this.ThingToNeverIntercept)
				{
					if (thing != this.launcher)
					{
						if (thing.def.Fillage == FillCategory.Full && this.InterceptWalls)
						{
							this.Impact(thing);
							return true;
						}
						if (this.FreeIntercept)
						{
							float num2 = 0f;
							Pawn pawn = thing as Pawn;
							if (pawn != null)
							{
								num2 = 0.4f;
								if (pawn.GetPosture() != PawnPosture.Standing)
								{
									num2 *= 0.1f;
								}
								if (this.launcher != null && pawn.Faction != null && this.launcher.Faction != null && !pawn.Faction.HostileTo(this.launcher.Faction))
								{
									num2 *= 0.4f;
								}
								num2 *= Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
							}
							else if (thing.def.fillPercent > 0.2f)
							{
								num2 = thing.def.fillPercent * 0.07f;
							}
							if (num2 > 1E-05f)
							{
								if (num < 49f)
								{
									num2 *= 0.5f;
								}
								else if (num < 100f)
								{
									num2 *= 0.75f;
								}
								if (DebugViewSettings.drawShooting)
								{
									MoteMaker.ThrowText(this.ExactPosition, base.Map, num2.ToStringPercent(), -1f);
								}
								if (Rand.Value < num2)
								{
									this.Impact(thing);
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}

		public override void Draw()
		{
			Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.def.DrawMatSingle, 0);
			base.Comps_PostDraw();
		}

		private void ImpactSomething()
		{
			if (this.def.projectile.flyOverhead)
			{
				RoofDef roofDef = base.Map.roofGrid.RoofAt(base.Position);
				if (roofDef != null)
				{
					if (roofDef.isThickRoof)
					{
						this.def.projectile.soundHitThickRoof.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
						this.Destroy(DestroyMode.Vanish);
						return;
					}
					if (base.Position.GetEdifice(base.Map) == null || base.Position.GetEdifice(base.Map).def.Fillage != FillCategory.Full)
					{
						RoofCollapserImmediate.DropRoofInCells(base.Position, base.Map);
					}
				}
			}
			if (this.assignedTarget != null)
			{
				Pawn pawn = this.assignedTarget as Pawn;
				if (pawn != null && pawn.GetPosture() != PawnPosture.Standing && (this.origin - this.destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.2f)
				{
					this.Impact(null);
					return;
				}
				this.Impact(this.assignedTarget);
				return;
			}
			else
			{
				Projectile.cellThingsFiltered.Clear();
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn2 = thingList[i] as Pawn;
					if (pawn2 != null)
					{
						Projectile.cellThingsFiltered.Add(pawn2);
					}
				}
				if (Projectile.cellThingsFiltered.Count > 0)
				{
					this.Impact(Projectile.cellThingsFiltered.RandomElement<Thing>());
					return;
				}
				Projectile.cellThingsFiltered.Clear();
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					if (thing.def.fillPercent > 0f || thing.def.passability != Traversability.Standable)
					{
						Projectile.cellThingsFiltered.Add(thing);
					}
				}
				if (Projectile.cellThingsFiltered.Count > 0)
				{
					this.Impact(Projectile.cellThingsFiltered.RandomElement<Thing>());
					return;
				}
				this.Impact(null);
				return;
			}
		}

		protected virtual void Impact(Thing hitThing)
		{
			this.Destroy(DestroyMode.Vanish);
		}

		public void ForceInstantImpact()
		{
			if (!this.DestinationCell.InBounds(base.Map))
			{
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			this.ticksToImpact = 0;
			base.Position = this.DestinationCell;
			this.ImpactSomething();
		}
	}
}
