using RimWorld;
using System;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_RangedFire : LogEntry
	{
		private Pawn initiatorPawn;

		private ThingDef initiatorThing;

		private Pawn recipientPawn;

		private ThingDef recipientThing;

		private ThingDef weaponDef;

		private ThingDef projectileDef;

		private bool burst;

		private string InitiatorName
		{
			get
			{
				return (this.initiatorPawn == null) ? "null" : this.initiatorPawn.NameStringShort;
			}
		}

		private string RecipientName
		{
			get
			{
				return (this.recipientPawn == null) ? "null" : this.recipientPawn.NameStringShort;
			}
		}

		public BattleLogEntry_RangedFire()
		{
		}

		public BattleLogEntry_RangedFire(Thing initiator, Thing target, ThingDef weaponDef, ThingDef projectileDef, bool burst)
		{
			if (initiator is Pawn)
			{
				this.initiatorPawn = (initiator as Pawn);
			}
			else if (initiator != null)
			{
				this.initiatorThing = initiator.def;
			}
			if (target is Pawn)
			{
				this.recipientPawn = (target as Pawn);
			}
			else if (target != null)
			{
				this.recipientThing = target.def;
			}
			this.weaponDef = weaponDef;
			this.projectileDef = projectileDef;
			this.burst = burst;
		}

		public override bool Concerns(Thing t)
		{
			return t == this.initiatorPawn || t == this.recipientPawn;
		}

		public override void ClickedFromPOV(Thing pov)
		{
			if (this.recipientPawn == null)
			{
				return;
			}
			if (pov == this.initiatorPawn)
			{
				CameraJumper.TryJumpAndSelect(this.recipientPawn);
			}
			else
			{
				if (pov != this.recipientPawn)
				{
					throw new NotImplementedException();
				}
				CameraJumper.TryJumpAndSelect(this.initiatorPawn);
			}
		}

		public override string ToGameStringFromPOV(Thing pov)
		{
			if (this.initiatorPawn == null && this.initiatorThing == null)
			{
				Log.ErrorOnce("BattleLogEntry_RangedFire has a null initiator.", 60465709);
				return "[BattleLogEntry_RangedFire error: null pawn reference]";
			}
			Rand.PushState();
			Rand.Seed = this.randSeed;
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(RulePackDefOf.Combat_RangedFire);
			if (this.initiatorPawn != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", this.initiatorPawn, request.Constants));
			}
			else if (this.initiatorThing != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("initiator", this.initiatorThing));
			}
			else
			{
				request.Constants["initiator_missing"] = "True";
			}
			if (this.recipientPawn != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", this.recipientPawn, request.Constants));
			}
			else if (this.recipientThing != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("recipient", this.recipientThing));
			}
			else
			{
				request.Constants["recipient_missing"] = "True";
			}
			request.Rules.AddRange(PlayLogEntryUtility.RulesForOptionalWeapon("weapon", this.weaponDef, this.projectileDef));
			request.Constants["burst"] = this.burst.ToString();
			string result = GrammarResolver.Resolve("logentry", request, "ranged fire", false);
			Rand.PopState();
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.initiatorPawn, "initiatorPawn", true);
			Scribe_Defs.Look<ThingDef>(ref this.initiatorThing, "initiatorThing");
			Scribe_References.Look<Pawn>(ref this.recipientPawn, "recipientPawn", true);
			Scribe_Defs.Look<ThingDef>(ref this.recipientThing, "recipientThing");
			Scribe_Defs.Look<ThingDef>(ref this.weaponDef, "weaponDef");
			Scribe_Defs.Look<ThingDef>(ref this.projectileDef, "projectileDef");
			Scribe_Values.Look<bool>(ref this.burst, "burst", false, false);
		}

		public override string ToString()
		{
			return "BattleLogEntry_RangedFire: " + this.InitiatorName + "->" + this.RecipientName;
		}
	}
}
