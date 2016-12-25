using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Pawn_DraftController : IExposable
	{
		public Pawn pawn;

		public DraftStateHandler draftStateHandler;

		public bool Drafted
		{
			get
			{
				return this.draftStateHandler != null && this.draftStateHandler.Drafted;
			}
			set
			{
				this.draftStateHandler.Drafted = value;
			}
		}

		public Pawn_DraftController(Pawn pawn)
		{
			this.pawn = pawn;
			this.draftStateHandler = new DraftStateHandler(pawn);
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<DraftStateHandler>(ref this.draftStateHandler, "drafter", new object[]
			{
				this.pawn
			});
		}

		public void PlayerControllerTick()
		{
			this.draftStateHandler.DrafterTick();
		}

		public bool CanTakeOrderedJob()
		{
			return !this.pawn.HasAttachment(ThingDefOf.Fire) && (this.pawn.CurJob == null || this.pawn.CurJob.def.playerInterruptible);
		}

		public void TakeOrderedJob(Job newJob)
		{
			if (this.pawn.jobs.debugLog)
			{
				this.pawn.jobs.DebugLogEvent("TakeOrderedJob " + newJob);
			}
			if (!this.CanTakeOrderedJob())
			{
				if (this.pawn.jobs.debugLog)
				{
					this.pawn.jobs.DebugLogEvent("    CanTakePlayerJob is false. Returning.");
				}
				return;
			}
			GenJob.ValidateJob(newJob);
			if (this.pawn.jobs.curJob != null && this.pawn.jobs.curJob.JobIsSameAs(newJob))
			{
				return;
			}
			this.pawn.stances.CancelBusyStanceSoft();
			Find.PawnDestinationManager.UnreserveAllFor(this.pawn);
			if (newJob.def == JobDefOf.Goto)
			{
				Find.PawnDestinationManager.ReserveDestinationFor(this.pawn, newJob.targetA.Cell);
			}
			if (this.pawn.jobs.debugLog)
			{
				this.pawn.jobs.DebugLogEvent("    Queueing job");
			}
			if (this.pawn.jobs.jobQueue == null)
			{
				this.pawn.jobs.jobQueue = new JobQueue();
			}
			this.pawn.jobs.jobQueue.Clear();
			this.pawn.jobs.jobQueue.EnqueueFirst(newJob);
			if (this.pawn.jobs.curJob != null)
			{
				this.pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
			}
			else
			{
				this.pawn.jobs.CheckForJobOverride();
			}
		}

		public static IntVec3 BestGotoDestNear(IntVec3 root, Pawn searcher)
		{
			Predicate<IntVec3> predicate = (IntVec3 c) => !Find.PawnDestinationManager.DestinationIsReserved(c, searcher) && c.Standable() && searcher.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
			if (predicate(root))
			{
				return root;
			}
			int num = 1;
			IntVec3 result = default(IntVec3);
			float num2 = -1000f;
			bool flag = false;
			while (true)
			{
				IntVec3 intVec = root + GenRadial.RadialPattern[num];
				if (predicate(intVec))
				{
					float num3 = CoverUtility.TotalSurroundingCoverScore(intVec);
					if (num3 > num2)
					{
						num2 = num3;
						result = intVec;
						flag = true;
					}
				}
				if (num >= 8 && flag)
				{
					break;
				}
				num++;
			}
			return result;
		}

		[DebuggerHidden]
		internal IEnumerable<Gizmo> GetGizmos()
		{
			Command_Toggle draft = new Command_Toggle();
			draft.hotKey = KeyBindingDefOf.CommandColonistDraft;
			draft.isActive = (() => this.<>f__this.Drafted);
			draft.toggleAction = delegate
			{
				this.<>f__this.Drafted = !this.<>f__this.Drafted;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
			};
			draft.defaultDesc = "CommandToggleDraftDesc".Translate();
			draft.icon = TexCommand.Draft;
			draft.turnOnSound = SoundDef.Named("DraftOn");
			draft.turnOffSound = SoundDef.Named("DraftOff");
			if (!this.Drafted)
			{
				draft.defaultLabel = "CommandDraftLabel".Translate();
			}
			else
			{
				draft.defaultLabel = "CommandUndraftLabel".Translate();
			}
			if (this.pawn.Downed)
			{
				draft.Disable("IsIncapped".Translate(new object[]
				{
					this.pawn.NameStringShort
				}));
			}
			if (!this.Drafted)
			{
				draft.tutorTag = "Draft";
			}
			else
			{
				draft.tutorTag = "Undraft";
			}
			yield return draft;
		}
	}
}
