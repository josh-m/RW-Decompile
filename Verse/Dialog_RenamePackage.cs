using System;
using System.IO;
using UnityEngine;

namespace Verse
{
	public class Dialog_RenamePackage : Window
	{
		private DefPackage renamingPackage;

		private string proposedName;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(400f, 200f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public Dialog_RenamePackage(DefPackage renamingPackage)
		{
			this.renamingPackage = renamingPackage;
			this.proposedName = renamingPackage.fileName;
			this.closeOnEscapeKey = true;
			this.doCloseX = true;
			this.forcePause = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			this.proposedName = Widgets.TextField(new Rect(0f, 0f, 200f, 32f), this.proposedName);
			if (Widgets.ButtonText(new Rect(0f, 40f, 100f, 32f), "Rename", true, false, true) && this.TrySave())
			{
				this.Close(true);
			}
		}

		private bool TrySave()
		{
			if (string.IsNullOrEmpty(this.proposedName) || this.proposedName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				Messages.Message("Invalid filename.", MessageSound.RejectInput);
				return false;
			}
			if (Path.GetExtension(this.proposedName) != ".xml")
			{
				Messages.Message("Data package file names must end with .xml", MessageSound.RejectInput);
				return false;
			}
			this.renamingPackage.fileName = this.proposedName;
			return true;
		}
	}
}
