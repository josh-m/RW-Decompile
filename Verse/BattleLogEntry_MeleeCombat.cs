using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_MeleeCombat : LogEntry, IDamageResultLog
	{
		private RulePackDef outcomeRuleDef;

		private RulePackDef maneuverRuleDef;

		private Pawn initiator;

		private Pawn recipientPawn;

		private ThingDef recipientThing;

		private ImplementOwnerTypeDef implementType;

		private ThingDef ownerEquipmentDef;

		private HediffDef ownerHediffDef;

		private string toolLabel;

		private List<BodyPartDef> damagedParts;

		private List<bool> damagedPartsDestroyed;

		private string InitiatorName
		{
			get
			{
				return (this.initiator == null) ? "null" : this.initiator.NameStringShort;
			}
		}

		private string RecipientName
		{
			get
			{
				return (this.recipientPawn == null) ? "null" : this.recipientPawn.NameStringShort;
			}
		}

		public BattleLogEntry_MeleeCombat()
		{
		}

		public BattleLogEntry_MeleeCombat(RulePackDef outcomeRuleDef, RulePackDef maneuverRuleDef, Pawn initiator, Thing recipient, ImplementOwnerTypeDef implementType, string toolLabel, ThingDef ownerEquipmentDef = null, HediffDef ownerHediffDef = null)
		{
			this.outcomeRuleDef = outcomeRuleDef;
			this.maneuverRuleDef = maneuverRuleDef;
			this.initiator = initiator;
			this.implementType = implementType;
			this.ownerEquipmentDef = ownerEquipmentDef;
			this.ownerHediffDef = ownerHediffDef;
			this.toolLabel = toolLabel;
			if (recipient is Pawn)
			{
				this.recipientPawn = (recipient as Pawn);
			}
			else if (recipient != null)
			{
				this.recipientThing = recipient.def;
			}
			if (ownerEquipmentDef != null && ownerHediffDef != null)
			{
				Log.ErrorOnce(string.Format("Combat log owned by both equipment {0} and hediff {1}, may produce unexpected results", ownerEquipmentDef.label, ownerHediffDef.label), 96474669);
			}
		}

		public void FillTargets(List<BodyPartDef> damagedParts, List<bool> damagedPartsDestroyed)
		{
			this.damagedParts = damagedParts;
			this.damagedPartsDestroyed = damagedPartsDestroyed;
		}

		public override bool Concerns(Thing t)
		{
			return t == this.initiator || t == this.recipientPawn;
		}

		public override void ClickedFromPOV(Thing pov)
		{
			if (pov == this.initiator && this.recipientPawn != null)
			{
				CameraJumper.TryJumpAndSelect(this.recipientPawn);
			}
			else if (pov == this.recipientPawn)
			{
				CameraJumper.TryJumpAndSelect(this.initiator);
			}
			else if (this.recipientPawn != null)
			{
				throw new NotImplementedException();
			}
		}

		public override Texture2D IconFromPOV(Thing pov)
		{
			if (this.damagedParts.NullOrEmpty<BodyPartDef>())
			{
				return null;
			}
			if (pov == null || pov == this.recipientPawn)
			{
				return LogEntry.Blood;
			}
			if (pov == this.initiator)
			{
				return LogEntry.BloodTarget;
			}
			return null;
		}

		public override string ToGameStringFromPOV(Thing pov)
		{
			Rand.PushState();
			Rand.Seed = this.randSeed;
			GrammarRequest request = default(GrammarRequest);
			request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", this.initiator, request.Constants));
			if (this.recipientPawn != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", this.recipientPawn, request.Constants));
			}
			else if (this.recipientThing != null)
			{
				request.Rules.AddRange(GrammarUtility.RulesForDef("recipient", this.recipientThing));
			}
			request.Includes.Add(this.outcomeRuleDef);
			request.Includes.Add(this.maneuverRuleDef);
			if (!this.toolLabel.NullOrEmpty())
			{
				request.Rules.Add(new Rule_String("tool_label", this.toolLabel));
			}
			if (this.implementType != null && !this.implementType.implementOwnerRuleName.NullOrEmpty())
			{
				if (this.ownerEquipmentDef != null)
				{
					request.Rules.AddRange(GrammarUtility.RulesForDef(this.implementType.implementOwnerRuleName, this.ownerEquipmentDef));
				}
				else if (this.ownerHediffDef != null)
				{
					request.Rules.AddRange(GrammarUtility.RulesForDef(this.implementType.implementOwnerRuleName, this.ownerHediffDef));
				}
			}
			if (this.implementType != null && !this.implementType.implementOwnerTypeValue.NullOrEmpty())
			{
				request.Constants["implementOwnerType"] = this.implementType.implementOwnerTypeValue;
			}
			request.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("recipient_part", this.damagedParts, this.damagedPartsDestroyed, request.Constants));
			string result = GrammarResolver.Resolve("logentry", request, "combat interaction", false);
			Rand.PopState();
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<RulePackDef>(ref this.outcomeRuleDef, "outcomeRuleDef");
			Scribe_Defs.Look<RulePackDef>(ref this.maneuverRuleDef, "maneuverRuleDef");
			Scribe_References.Look<Pawn>(ref this.initiator, "initiator", true);
			Scribe_References.Look<Pawn>(ref this.recipientPawn, "recipientPawn", true);
			Scribe_Defs.Look<ThingDef>(ref this.recipientThing, "recipientThing");
			Scribe_Defs.Look<ImplementOwnerTypeDef>(ref this.implementType, "implementType");
			Scribe_Defs.Look<ThingDef>(ref this.ownerEquipmentDef, "ownerDef");
			Scribe_Collections.Look<BodyPartDef>(ref this.damagedParts, "damagedParts", LookMode.Def, new object[0]);
			Scribe_Collections.Look<bool>(ref this.damagedPartsDestroyed, "damagedPartsDestroyed", LookMode.Value, new object[0]);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.outcomeRuleDef.defName,
				": ",
				this.InitiatorName,
				"->",
				this.RecipientName
			});
		}
	}
}
