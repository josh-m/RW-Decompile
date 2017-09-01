using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Building_CryptosleepCasket : Building_Casket
	{
		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				if (allowSpecialEffects)
				{
					SoundDef.Named("CryptosleepCasketAccept").PlayOneShot(new TargetInfo(base.Position, base.Map, false));
				}
				return true;
			}
			return false;
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
			{
				yield return o;
			}
			if (this.innerContainer.Count == 0)
			{
				if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield return failer;
				}
				else
				{
					JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
					string jobStr = "EnterCryptosleepCasket".Translate();
					Action jobAction = delegate
					{
						Job job = new Job(this.<jobDef>__3, this.<>f__this);
						this.myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					};
					yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
				}
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo c in base.GetGizmos())
			{
				yield return c;
			}
			if (base.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && this.def.building.isPlayerEjectable)
			{
				Command_Action eject = new Command_Action();
				eject.action = new Action(this.EjectContents);
				eject.defaultLabel = "CommandPodEject".Translate();
				eject.defaultDesc = "CommandPodEjectDesc".Translate();
				if (this.innerContainer.Count == 0)
				{
					eject.Disable("CommandPodEjectFailEmpty".Translate());
				}
				eject.hotKey = KeyBindingDefOf.Misc1;
				eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
				yield return eject;
			}
		}

		public override void EjectContents()
		{
			ThingDef filthSlime = ThingDefOf.FilthSlime;
			foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
			{
				Pawn pawn = current as Pawn;
				if (pawn != null)
				{
					PawnComponentsUtility.AddComponentsForSpawn(pawn);
					pawn.filth.GainFilth(filthSlime);
					pawn.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null);
				}
			}
			if (!base.Destroyed)
			{
				SoundDef.Named("CryptosleepCasketEject").PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
			}
			base.EjectContents();
		}

		public static Building_CryptosleepCasket FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
		{
			IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
			where typeof(Building_CryptosleepCasket).IsAssignableFrom(def.thingClass)
			select def;
			foreach (ThingDef current in enumerable)
			{
				Predicate<Thing> validator = delegate(Thing x)
				{
					bool arg_2F_0;
					if (!((Building_CryptosleepCasket)x).HasAnyContents)
					{
						bool ignoreOtherReservations2 = ignoreOtherReservations;
						arg_2F_0 = traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations2);
					}
					else
					{
						arg_2F_0 = false;
					}
					return arg_2F_0;
				};
				Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(current), PathEndMode.InteractionCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (building_CryptosleepCasket != null)
				{
					return building_CryptosleepCasket;
				}
			}
			return null;
		}
	}
}
