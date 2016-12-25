using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_Wait : JobDriver
	{
		private const int TargetSearchInterval = 4;

		public override string GetReport()
		{
			if (base.CurJob.def != JobDefOf.WaitCombat)
			{
				return base.GetReport();
			}
			if (this.pawn.RaceProps.Humanlike && this.pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return "ReportStanding".Translate();
			}
			return base.GetReport();
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				initAction = delegate
				{
					Find.PawnDestinationManager.ReserveDestinationFor(this.<>f__this.pawn, this.<>f__this.pawn.Position);
					this.<>f__this.pawn.pather.StopDead();
					this.<>f__this.CheckForAutoAttack();
				},
				tickAction = delegate
				{
					if (this.<>f__this.CurJob.expiryInterval == -1 && this.<>f__this.CurJob.def == JobDefOf.WaitCombat && !this.<>f__this.pawn.Drafted)
					{
						Log.Error(this.<>f__this.pawn + " in eternal WaitCombat without being drafted.");
						this.<>f__this.ReadyForNextToil();
						return;
					}
					if ((Find.TickManager.TicksGame + this.<>f__this.pawn.thingIDNumber) % 4 == 0)
					{
						this.<>f__this.CheckForAutoAttack();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}

		public override void Notify_StanceChanged()
		{
			if (this.pawn.stances.curStance is Stance_Mobile)
			{
				this.CheckForAutoAttack();
			}
		}

		private void CheckForAutoAttack()
		{
			if (this.pawn.Downed)
			{
				return;
			}
			if (this.pawn.stances.FullBodyBusy)
			{
				return;
			}
			if (this.pawn.story == null || !this.pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				bool flag = this.pawn.RaceProps.ToolUser && this.pawn.Faction == Faction.OfPlayer && !this.pawn.story.WorkTagIsDisabled(WorkTags.Firefighting);
				Fire fire = null;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 c = this.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
					if (c.InBounds())
					{
						List<Thing> thingList = c.GetThingList();
						for (int j = 0; j < thingList.Count; j++)
						{
							Pawn pawn = thingList[j] as Pawn;
							if (pawn != null && !pawn.Downed && this.pawn.HostileTo(pawn))
							{
								this.pawn.meleeVerbs.TryMeleeAttack(pawn, null, false);
								return;
							}
							if (flag)
							{
								Fire fire2 = thingList[j] as Fire;
								if (fire2 != null && (fire == null || fire2.fireSize < fire.fireSize || i == 8) && (fire2.parent == null || fire2.parent != this.pawn))
								{
									fire = fire2;
								}
							}
						}
					}
				}
				if (fire != null && (!this.pawn.InMentalState || this.pawn.MentalState.def.allowBeatfire))
				{
					this.pawn.natives.TryBeatFire(fire);
					return;
				}
				if (this.pawn.Faction != null && this.pawn.jobs.curJob.def == JobDefOf.WaitCombat)
				{
					bool allowManualCastWeapons = !this.pawn.IsColonist;
					Verb verb = this.pawn.TryGetAttackVerb(allowManualCastWeapons);
					if (verb != null && !verb.verbProps.MeleeRange)
					{
						TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat;
						if (verb.verbProps.ai_IsIncendiary)
						{
							targetScanFlags |= TargetScanFlags.NeedNonBurning;
						}
						Thing thing = AttackTargetFinder.BestShootTargetFromCurrentPosition(this.pawn, null, verb.verbProps.range, verb.verbProps.minRange, targetScanFlags);
						if (thing != null)
						{
							this.pawn.equipment.TryStartAttack(thing);
							return;
						}
					}
				}
			}
		}
	}
}
