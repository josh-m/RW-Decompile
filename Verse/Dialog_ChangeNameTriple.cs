using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_ChangeNameTriple : Window
	{
		private const int MaxNameLength = 16;

		private Pawn pawn;

		private string curName;

		private NameTriple CurPawnName
		{
			get
			{
				NameTriple nameTriple = this.pawn.Name as NameTriple;
				if (nameTriple != null)
				{
					return new NameTriple(nameTriple.First, this.curName, nameTriple.Last);
				}
				throw new InvalidOperationException();
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(500f, 175f);
			}
		}

		public Dialog_ChangeNameTriple(Pawn pawn)
		{
			this.pawn = pawn;
			this.curName = ((NameTriple)pawn.Name).Nick;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnClickedOutside = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(15f, 15f, 500f, 50f), this.CurPawnName.ToString().Replace(" '' ", " "));
			Text.Font = GameFont.Small;
			string text = Widgets.TextField(new Rect(15f, 50f, inRect.width / 2f - 20f, 35f), this.curName);
			if (text.Length < 16)
			{
				this.curName = text;
			}
			if (Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "OK", true, false, true) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
			{
				if (this.curName.Length < 1)
				{
					this.curName = ((NameTriple)this.pawn.Name).First;
				}
				this.pawn.Name = this.CurPawnName;
				Find.WindowStack.TryRemove(this, true);
				Messages.Message("PawnGainsName".Translate(new object[]
				{
					this.curName
				}), this.pawn, MessageSound.Benefit);
			}
		}
	}
}
