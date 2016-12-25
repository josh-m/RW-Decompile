using RimWorld;
using System;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public class Dialog_DebugSettingsMenu : Dialog_DebugOptionLister
	{
		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public Dialog_DebugSettingsMenu()
		{
			this.forcePause = true;
		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.ToggleDebugSettingsMenu.KeyDownEvent)
			{
				Event.current.Use();
				this.Close(true);
			}
			Text.Font = GameFont.Small;
			this.listing.Label("Gameplay");
			FieldInfo[] fields = typeof(DebugSettings).GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo fi = fields[i];
				this.DoField(fi);
			}
			this.listing.Gap(36f);
			Text.Font = GameFont.Small;
			this.listing.Label("View");
			FieldInfo[] fields2 = typeof(DebugViewSettings).GetFields();
			for (int j = 0; j < fields2.Length; j++)
			{
				FieldInfo fi2 = fields2[j];
				this.DoField(fi2);
			}
		}

		private void DoField(FieldInfo fi)
		{
			if (fi.IsLiteral)
			{
				return;
			}
			string label = GenText.SplitCamelCase(fi.Name).CapitalizeFirst();
			bool flag = (bool)fi.GetValue(null);
			bool flag2 = flag;
			base.CheckboxLabeledDebug(label, ref flag);
			if (flag != flag2)
			{
				fi.SetValue(null, flag);
			}
		}
	}
}
