using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_DamageTaken : LogEntry, IDamageResultLog
	{
		private Pawn initiatorPawn;

		private Pawn recipientPawn;

		private RulePackDef ruleDef;

		private List<BodyPartDef> damagedParts;

		private List<bool> damagedPartsDestroyed;

		private string RecipientName
		{
			get
			{
				return (this.recipientPawn == null) ? "null" : this.recipientPawn.NameStringShort;
			}
		}

		public BattleLogEntry_DamageTaken()
		{
		}

		public BattleLogEntry_DamageTaken(Pawn recipient, RulePackDef ruleDef, Pawn initiator = null)
		{
			this.initiatorPawn = initiator;
			this.recipientPawn = recipient;
			this.ruleDef = ruleDef;
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
			CameraJumper.TryJumpAndSelect(this.recipientPawn);
		}

		public override Texture2D IconFromPOV(Thing pov)
		{
			return LogEntry.Blood;
		}

		public override string ToGameStringFromPOV(Thing pov)
		{
			if (this.recipientPawn == null)
			{
				Log.ErrorOnce("BattleLogEntry_DamageTaken has a null recipient.", 60465709);
				return "[BattleLogEntry_DamageTaken error: null pawn reference]";
			}
			Rand.PushState();
			Rand.Seed = this.randSeed;
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(this.ruleDef);
			request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", this.recipientPawn, request.Constants));
			request.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("recipient_part", this.damagedParts, this.damagedPartsDestroyed, request.Constants));
			string result = GrammarResolver.Resolve("logentry", request, "damage taken", false);
			Rand.PopState();
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.initiatorPawn, "initiatorPawn", true);
			Scribe_References.Look<Pawn>(ref this.recipientPawn, "recipientPawn", true);
			Scribe_Defs.Look<RulePackDef>(ref this.ruleDef, "ruleDef");
			Scribe_Collections.Look<BodyPartDef>(ref this.damagedParts, "damagedParts", LookMode.Def, new object[0]);
			Scribe_Collections.Look<bool>(ref this.damagedPartsDestroyed, "damagedPartsDestroyed", LookMode.Value, new object[0]);
		}

		public override string ToString()
		{
			return "BattleLogEntry_DamageTaken: " + this.RecipientName;
		}
	}
}
