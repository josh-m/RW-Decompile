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
				if (!myPawn.CanReserve(this, 1))
				{
					FloatMenuOption failer = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield return failer;
				}
				else if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					FloatMenuOption failer2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield return failer2;
				}
				else
				{
					JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
					string jobStr = "EnterCryptosleepCasket".Translate();
					Action jobAction = delegate
					{
						Job job = new Job(this.<jobDef>__4, this.<>f__this);
						this.myPawn.jobs.TryTakeOrderedJob(job);
					};
					yield return new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null);
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
			foreach (Thing current in this.innerContainer)
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

		public static Building_CryptosleepCasket FindCryptosleepCasketFor(Pawn p, Pawn traveler)
		{
			IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
			where typeof(Building_CryptosleepCasket).IsAssignableFrom(def.thingClass)
			select def;
			foreach (ThingDef current in enumerable)
			{
				Predicate<Thing> validator = (Thing x) => ((Building_CryptosleepCasket)x).GetInnerContainer().Count == 0;
				Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(current), PathEndMode.InteractionCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
				if (building_CryptosleepCasket != null)
				{
					return building_CryptosleepCasket;
				}
			}
			return null;
		}
	}
}
