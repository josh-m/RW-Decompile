using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_NamePlayerFaction : Window
	{
		private Pawn suggestingPawn;

		private string curName = NameGenerator.GenerateName(RulePackDefOf.NamerColony, null);

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(500f, 200f);
			}
		}

		public Dialog_NamePlayerFaction()
		{
			this.suggestingPawn = Find.MapPawns.FreeColonists.RandomElement<Pawn>();
			this.forcePause = true;
			this.closeOnEscapeKey = false;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect rect)
		{
			Text.Font = GameFont.Small;
			bool flag = false;
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
			{
				flag = true;
				Event.current.Use();
			}
			Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "NameColonyMessage".Translate(new object[]
			{
				this.suggestingPawn.NameStringShort
			}));
			this.curName = Widgets.TextField(new Rect(0f, rect.height - 35f, rect.width / 2f - 20f, 35f), this.curName);
			if (Widgets.ButtonText(new Rect(rect.width / 2f + 20f, rect.height - 35f, rect.width / 2f - 20f, 35f), "OK".Translate(), true, false, true) || flag)
			{
				if (this.IsValidColonyName(this.curName))
				{
					Faction.OfPlayer.Name = this.curName;
					Find.WindowStack.TryRemove(this, true);
					Messages.Message("ColonyGainsName".Translate(new object[]
					{
						this.curName
					}), MessageSound.Benefit);
				}
				else
				{
					Messages.Message("ColonyNameIsInvalid".Translate(), MessageSound.RejectInput);
				}
				Event.current.Use();
			}
		}

		private bool IsValidColonyName(string s)
		{
			return s.Length != 0 && GenText.IsValidFilename(s);
		}
	}
}
