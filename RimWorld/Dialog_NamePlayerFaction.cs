using System;

namespace RimWorld
{
	public class Dialog_NamePlayerFaction : Dialog_GiveName
	{
		public Dialog_NamePlayerFaction()
		{
			this.curName = NameGenerator.GenerateName(RulePackDefOf.NamerFactionPlayerRandomized, null, false);
			this.nameMessageKey = "NamePlayerFactionMessage";
			this.gainedNameMessageKey = "PlayerFactionGainsName";
			this.invalidNameMessageKey = "PlayerFactionNameIsInvalid";
		}

		protected override bool IsValidName(string s)
		{
			return NamePlayerFactionDialogUtility.IsValidName(s);
		}

		protected override void Named(string s)
		{
			NamePlayerFactionDialogUtility.Named(s);
		}
	}
}
