using RimWorld;
using System;
using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_ExplosionImpact : LogEntry, IDamageResultLog
	{
		private Pawn initiatorPawn;

		private ThingDef initiatorThing;

		private Pawn recipientPawn;

		private ThingDef recipientThing;

		private ThingDef weaponDef;

		private ThingDef projectileDef;

		private DamageDef damageDef;

		private List<BodyPartDef> damagedParts;

		private List<bool> damagedPartsDestroyed;

		private string InitiatorName
		{
			get
			{
				if (this.initiatorPawn != null)
				{
					return this.initiatorPawn.NameStringShort;
				}
				if (this.initiatorThing != null)
				{
					return this.initiatorThing.defName;
				}
				return "null";
			}
		}

		private string RecipientName
		{
			get
			{
				if (this.recipientPawn != null)
				{
					return this.recipientPawn.NameStringShort;
				}
				if (this.recipientThing != null)
				{
					return this.recipientThing.defName;
				}
				return "null";
			}
		}

		public BattleLogEntry_ExplosionImpact()
		{
		}

		public BattleLogEntry_ExplosionImpact(Thing initiator, Thing recipient, ThingDef weaponDef, ThingDef projectileDef, DamageDef damageDef)
		{
			if (initiator is Pawn)
			{
				this.initiatorPawn = (initiator as Pawn);
			}
			else if (initiator != null)
			{
				this.initiatorThing = initiator.def;
			}
			if (recipient is Pawn)
			{
				this.recipientPawn = (recipient as Pawn);
			}
			else if (recipient != null)
			{
				this.recipientThing = recipient.def;
			}
			this.weaponDef = weaponDef;
			this.projectileDef = projectileDef;
			this.damageDef = damageDef;
		}

		public void FillTargets(List<BodyPartDef> recipientParts, List<bool> recipientPartsDestroyed)
		{
			this.damagedParts = recipientParts;
			this.damagedPartsDestroyed = recipientPartsDestroyed;
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
			Rand.PushState();
			Rand.Seed = this.randSeed;
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(RulePackDefOf.Combat_ExplosionImpact);
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
			if (this.projectileDef != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("projectile", this.projectileDef));
			}
			if (this.damageDef != null && this.damageDef.combatLogRules != null)
			{
				request.Includes.Add(this.damageDef.combatLogRules);
			}
			request.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("recipient_part", this.damagedParts, this.damagedPartsDestroyed, request.Constants));
			string result = GrammarResolver.Resolve("logentry", request, "ranged explosion", false);
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
			Scribe_Defs.Look<DamageDef>(ref this.damageDef, "damageDef");
			Scribe_Collections.Look<BodyPartDef>(ref this.damagedParts, "damagedParts", LookMode.Def, new object[0]);
			Scribe_Collections.Look<bool>(ref this.damagedPartsDestroyed, "damagedPartsDestroyed", LookMode.Value, new object[0]);
		}

		public override string ToString()
		{
			return "BattleLogEntry_ExplosionImpact: " + this.InitiatorName + "->" + this.RecipientName;
		}
	}
}
