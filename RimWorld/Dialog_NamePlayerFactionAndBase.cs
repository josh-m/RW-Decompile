using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class Dialog_NamePlayerFactionAndBase : Dialog_GiveName
	{
		private FactionBase factionBase;

		public Dialog_NamePlayerFactionAndBase(FactionBase factionBase)
		{
			this.factionBase = factionBase;
			if (factionBase.HasMap && factionBase.Map.mapPawns.FreeColonistsSpawnedCount != 0)
			{
				this.suggestingPawn = factionBase.Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
			}
			this.curName = NameGenerator.GenerateName(RulePackDefOf.NamerFactionPlayerRandomized, null, false);
			this.nameMessageKey = "NamePlayerFactionMessage";
			this.invalidNameMessageKey = "PlayerFactionNameIsInvalid";
			this.useSecondName = true;
			this.curSecondName = NameGenerator.GenerateName(RulePackDefOf.NamerFactionBasePlayerRandomized, null, false);
			this.secondNameMessageKey = "NamePlayerFactionBaseMessage_NameFactionContinuation";
			this.invalidSecondNameMessageKey = "PlayerFactionBaseNameIsInvalid";
			this.gainedNameMessageKey = "PlayerFactionAndBaseGainsName";
		}

		public override void PostOpen()
		{
			base.PostOpen();
			if (this.factionBase.Map != null)
			{
				Current.Game.VisibleMap = this.factionBase.Map;
			}
		}

		protected override bool IsValidName(string s)
		{
			return NamePlayerFactionDialogUtility.IsValidName(s);
		}

		protected override bool IsValidSecondName(string s)
		{
			return NamePlayerFactionBaseDialogUtility.IsValidName(s);
		}

		protected override void Named(string s)
		{
			NamePlayerFactionDialogUtility.Named(s);
		}

		protected override void NamedSecond(string s)
		{
			NamePlayerFactionBaseDialogUtility.Named(this.factionBase, s);
		}
	}
}
