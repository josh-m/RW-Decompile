using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_GiveName : Window
	{
		protected Pawn suggestingPawn;

		protected string curName;

		protected Func<string> nameGenerator;

		protected string nameMessageKey;

		protected string gainedNameMessageKey;

		protected string invalidNameMessageKey;

		protected bool useSecondName;

		protected string curSecondName;

		protected Func<string> secondNameGenerator;

		protected string secondNameMessageKey;

		protected string invalidSecondNameMessageKey;

		private float Height
		{
			get
			{
				return (!this.useSecondName) ? 200f : 300f;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(640f, this.Height);
			}
		}

		public Dialog_GiveName()
		{
			if (Find.AnyPlayerHomeMap != null && Find.AnyPlayerHomeMap.mapPawns.FreeColonistsCount != 0)
			{
				if (Find.AnyPlayerHomeMap.mapPawns.FreeColonistsSpawnedCount != 0)
				{
					this.suggestingPawn = Find.AnyPlayerHomeMap.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
				}
				else
				{
					this.suggestingPawn = Find.AnyPlayerHomeMap.mapPawns.FreeColonists.RandomElement<Pawn>();
				}
			}
			else
			{
				this.suggestingPawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.RandomElement<Pawn>();
			}
			this.forcePause = true;
			this.closeOnAccept = false;
			this.closeOnCancel = false;
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
			Rect rect2;
			if (!this.useSecondName)
			{
				Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), this.nameMessageKey.Translate(this.suggestingPawn.LabelShort, this.suggestingPawn).CapitalizeFirst());
				if (this.nameGenerator != null && Widgets.ButtonText(new Rect(rect.width / 2f + 90f, 80f, rect.width / 2f - 90f, 35f), "Randomize".Translate(), true, false, true))
				{
					this.curName = this.nameGenerator();
				}
				this.curName = Widgets.TextField(new Rect(0f, 80f, rect.width / 2f + 70f, 35f), this.curName);
				rect2 = new Rect(rect.width / 2f + 90f, rect.height - 35f, rect.width / 2f - 90f, 35f);
			}
			else
			{
				float num = 0f;
				string text = this.nameMessageKey.Translate(this.suggestingPawn.LabelShort, this.suggestingPawn).CapitalizeFirst();
				Widgets.Label(new Rect(0f, num, rect.width, rect.height), text);
				num += Text.CalcHeight(text, rect.width) + 10f;
				if (this.nameGenerator != null && Widgets.ButtonText(new Rect(rect.width / 2f + 90f, num, rect.width / 2f - 90f, 35f), "Randomize".Translate(), true, false, true))
				{
					this.curName = this.nameGenerator();
				}
				this.curName = Widgets.TextField(new Rect(0f, num, rect.width / 2f + 70f, 35f), this.curName);
				num += 60f;
				text = this.secondNameMessageKey.Translate(this.suggestingPawn.LabelShort, this.suggestingPawn);
				Widgets.Label(new Rect(0f, num, rect.width, rect.height), text);
				num += Text.CalcHeight(text, rect.width) + 10f;
				if (this.secondNameGenerator != null && Widgets.ButtonText(new Rect(rect.width / 2f + 90f, num, rect.width / 2f - 90f, 35f), "Randomize".Translate(), true, false, true))
				{
					this.curSecondName = this.secondNameGenerator();
				}
				this.curSecondName = Widgets.TextField(new Rect(0f, num, rect.width / 2f + 70f, 35f), this.curSecondName);
				num += 45f;
				float num2 = rect.width / 2f - 90f;
				rect2 = new Rect(rect.width / 2f - num2 / 2f, rect.height - 35f, num2, 35f);
			}
			if (Widgets.ButtonText(rect2, "OK".Translate(), true, false, true) || flag)
			{
				if (this.IsValidName(this.curName) && (!this.useSecondName || this.IsValidSecondName(this.curSecondName)))
				{
					if (this.useSecondName)
					{
						this.Named(this.curName);
						this.NamedSecond(this.curSecondName);
						Messages.Message(this.gainedNameMessageKey.Translate(this.curName, this.curSecondName), MessageTypeDefOf.TaskCompletion, false);
					}
					else
					{
						this.Named(this.curName);
						Messages.Message(this.gainedNameMessageKey.Translate(this.curName), MessageTypeDefOf.TaskCompletion, false);
					}
					Find.WindowStack.TryRemove(this, true);
				}
				else
				{
					Messages.Message(this.invalidNameMessageKey.Translate(), MessageTypeDefOf.RejectInput, false);
				}
				Event.current.Use();
			}
		}

		protected abstract bool IsValidName(string s);

		protected abstract void Named(string s);

		protected virtual bool IsValidSecondName(string s)
		{
			return true;
		}

		protected virtual void NamedSecond(string s)
		{
		}
	}
}
