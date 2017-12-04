using RimWorld;
using System;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_StateTransition : LogEntry
	{
		private RulePackDef transitionDef;

		private Pawn subjectPawn;

		private ThingDef subjectThing;

		private Pawn initiator;

		private HediffDef culpritHediffDef;

		private BodyPartDef culpritHediffTargetDef;

		private BodyPartDef culpritTargetDef;

		private string SubjectName
		{
			get
			{
				return (this.subjectPawn == null) ? "null" : this.subjectPawn.NameStringShort;
			}
		}

		public BattleLogEntry_StateTransition()
		{
		}

		public BattleLogEntry_StateTransition(Thing subject, RulePackDef transitionDef, Pawn initiator, Hediff culpritHediff, BodyPartDef culpritTargetDef)
		{
			if (subject is Pawn)
			{
				this.subjectPawn = (subject as Pawn);
			}
			else if (subject != null)
			{
				this.subjectThing = subject.def;
			}
			this.transitionDef = transitionDef;
			this.initiator = initiator;
			if (culpritHediff != null)
			{
				this.culpritHediffDef = culpritHediff.def;
				if (culpritHediff.Part != null)
				{
					this.culpritHediffTargetDef = culpritHediff.Part.def;
				}
			}
			this.culpritTargetDef = culpritTargetDef;
		}

		public override bool Concerns(Thing t)
		{
			return t == this.subjectPawn || t == this.initiator;
		}

		public override void ClickedFromPOV(Thing pov)
		{
			if (pov == this.subjectPawn)
			{
				CameraJumper.TryJumpAndSelect(this.initiator);
			}
			else
			{
				if (pov != this.initiator)
				{
					throw new NotImplementedException();
				}
				CameraJumper.TryJumpAndSelect(this.subjectPawn);
			}
		}

		public override Texture2D IconFromPOV(Thing pov)
		{
			if (pov == null || pov == this.subjectPawn)
			{
				return (this.transitionDef != RulePackDefOf.Transition_Downed) ? LogEntry.Skull : LogEntry.Downed;
			}
			if (pov == this.initiator)
			{
				return (this.transitionDef != RulePackDefOf.Transition_Downed) ? LogEntry.SkullTarget : LogEntry.DownedTarget;
			}
			return null;
		}

		public override string ToGameStringFromPOV(Thing pov)
		{
			Rand.PushState();
			Rand.Seed = this.randSeed;
			GrammarRequest request = default(GrammarRequest);
			if (this.subjectPawn != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForPawn("subject", this.subjectPawn, request.Constants));
			}
			else if (this.subjectThing != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("subject", this.subjectThing));
			}
			request.Includes.Add(this.transitionDef);
			if (this.initiator != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", this.initiator, request.Constants));
			}
			if (this.culpritHediffDef != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("culpritHediff", this.culpritHediffDef));
			}
			if (this.culpritHediffTargetDef != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("culpritHediff_target", this.culpritHediffTargetDef));
			}
			if (this.culpritTargetDef != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("culpritHediff_originaltarget", this.culpritTargetDef));
			}
			string result = GrammarResolver.Resolve("logentry", request, "state transition", false);
			Rand.PopState();
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<RulePackDef>(ref this.transitionDef, "transitionDef");
			Scribe_References.Look<Pawn>(ref this.subjectPawn, "subjectPawn", true);
			Scribe_Defs.Look<ThingDef>(ref this.subjectThing, "subjectThing");
			Scribe_References.Look<Pawn>(ref this.initiator, "initiator", true);
			Scribe_Defs.Look<HediffDef>(ref this.culpritHediffDef, "culpritHediffDef");
			Scribe_Defs.Look<BodyPartDef>(ref this.culpritHediffTargetDef, "culpritHediffTargetDef");
			Scribe_Defs.Look<BodyPartDef>(ref this.culpritTargetDef, "culpritTargetDef");
		}

		public override string ToString()
		{
			return this.transitionDef.defName + ": " + this.subjectPawn;
		}
	}
}
