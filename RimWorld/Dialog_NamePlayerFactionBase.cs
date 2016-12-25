using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class Dialog_NamePlayerFactionBase : Dialog_GiveName
	{
		private FactionBase factionBase;

		public Dialog_NamePlayerFactionBase(FactionBase factionBase)
		{
			this.factionBase = factionBase;
			if (factionBase.HasMap && factionBase.Map.mapPawns.FreeColonistsSpawnedCount != 0)
			{
				this.suggestingPawn = factionBase.Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
			}
			this.curName = NameGenerator.GenerateName(RulePackDefOf.NamerFactionBasePlayerRandomized, null, false);
			this.nameMessageKey = "NamePlayerFactionBaseMessage";
			this.gainedNameMessageKey = "PlayerFactionBaseGainsName";
			this.invalidNameMessageKey = "PlayerFactionBaseNameIsInvalid";
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
			return NamePlayerFactionBaseDialogUtility.IsValidName(s);
		}

		protected override void Named(string s)
		{
			NamePlayerFactionBaseDialogUtility.Named(this.factionBase, s);
		}
	}
}
