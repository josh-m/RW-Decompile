using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public class PlayLogEntry_Interaction : PlayLogEntry
	{
		private InteractionDef intDef;

		private Pawn initiator;

		private Pawn recipient;

		private List<RulePackDef> extraSentencePacks;

		public override Texture2D Icon
		{
			get
			{
				return this.intDef.Symbol;
			}
		}

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
				return (this.recipient == null) ? "null" : this.recipient.NameStringShort;
			}
		}

		public PlayLogEntry_Interaction()
		{
		}

		public PlayLogEntry_Interaction(InteractionDef intDef, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			this.ticksAbs = Find.TickManager.TicksAbs;
			this.intDef = intDef;
			this.initiator = initiator;
			this.recipient = recipient;
			this.extraSentencePacks = extraSentencePacks;
		}

		public override bool Concerns(Thing t)
		{
			return t == this.initiator || t == this.recipient;
		}

		public override void ClickedFromPOV(Thing pov)
		{
			if (pov == this.initiator)
			{
				JumpToTargetUtility.TryJumpAndSelect(this.recipient);
			}
			else
			{
				if (pov != this.recipient)
				{
					throw new NotImplementedException();
				}
				JumpToTargetUtility.TryJumpAndSelect(this.initiator);
			}
		}

		public override string ToGameStringFromPOV(Thing pov)
		{
			if (this.initiator == null || this.recipient == null)
			{
				Log.ErrorOnce("PlayLogEntry_Interaction has a null pawn reference.", 34422);
				return "[" + this.intDef.label + " error: null pawn reference]";
			}
			Rand.PushSeed();
			Rand.Seed = this.ticksAbs * 61261;
			List<Rule> list = new List<Rule>();
			string text;
			if (pov == this.initiator)
			{
				list.AddRange(this.intDef.logRulesInitiator.Rules);
				list.AddRange(GrammarUtility.RulesForPawn("me", this.initiator.Name, this.initiator.kindDef, this.initiator.gender, this.initiator.Faction));
				list.AddRange(GrammarUtility.RulesForPawn("other", this.recipient.Name, this.recipient.kindDef, this.recipient.gender, this.recipient.Faction));
				text = GrammarResolver.Resolve("logentry", list, "interaction from initiator");
			}
			else if (pov == this.recipient)
			{
				if (this.intDef.logRulesRecipient != null)
				{
					list.AddRange(this.intDef.logRulesRecipient.Rules);
				}
				else
				{
					list.AddRange(this.intDef.logRulesInitiator.Rules);
				}
				list.AddRange(GrammarUtility.RulesForPawn("me", this.recipient.Name, this.recipient.kindDef, this.recipient.gender, this.recipient.Faction));
				list.AddRange(GrammarUtility.RulesForPawn("other", this.initiator.Name, this.initiator.kindDef, this.initiator.gender, this.initiator.Faction));
				text = GrammarResolver.Resolve("logentry", list, "interaction from recipient");
			}
			else
			{
				Log.ErrorOnce("Cannot display PlayLogEntry_Interaction from POV who isn't initiator or recipient.", 51251);
				text = this.ToString();
			}
			if (this.extraSentencePacks != null)
			{
				for (int i = 0; i < this.extraSentencePacks.Count; i++)
				{
					list.Clear();
					list.AddRange(this.extraSentencePacks[i].Rules);
					list.AddRange(GrammarUtility.RulesForPawn("initiator", this.initiator.Name, this.initiator.kindDef, this.initiator.gender, this.initiator.Faction));
					list.AddRange(GrammarUtility.RulesForPawn("recipient", this.recipient.Name, this.recipient.kindDef, this.recipient.gender, this.recipient.Faction));
					text = text + " " + GrammarResolver.Resolve(this.extraSentencePacks[i].Rules[0].keyword, list, "extraSentencePack");
				}
			}
			Rand.PopSeed();
			return text;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			WorldPawns worldPawns = Find.WorldPawns;
			if (worldPawns.Contains(this.initiator))
			{
				worldPawns.DiscardIfUnimportant(this.initiator);
			}
			if (worldPawns.Contains(this.recipient))
			{
				worldPawns.DiscardIfUnimportant(this.recipient);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<InteractionDef>(ref this.intDef, "intDef");
			Scribe_References.LookReference<Pawn>(ref this.initiator, "initiator", true);
			Scribe_References.LookReference<Pawn>(ref this.recipient, "recipient", true);
			Scribe_Collections.LookList<RulePackDef>(ref this.extraSentencePacks, "extras", LookMode.Undefined, new object[0]);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.intDef.label,
				": ",
				this.InitiatorName,
				"->",
				this.RecipientName
			});
		}
	}
}
