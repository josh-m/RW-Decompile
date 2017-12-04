using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class StorageSettingsClipboard
	{
		private static StorageSettings clipboard = new StorageSettings();

		private static bool copied = false;

		public static bool HasCopiedSettings
		{
			get
			{
				return StorageSettingsClipboard.copied;
			}
		}

		public static void Copy(StorageSettings s)
		{
			StorageSettingsClipboard.clipboard.CopyFrom(s);
			StorageSettingsClipboard.copied = true;
		}

		public static void PasteInto(StorageSettings s)
		{
			s.CopyFrom(StorageSettingsClipboard.clipboard);
		}

		[DebuggerHidden]
		public static IEnumerable<Gizmo> CopyPasteGizmosFor(StorageSettings s)
		{
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings", true),
				defaultLabel = "CommandCopyZoneSettingsLabel".Translate(),
				defaultDesc = "CommandCopyZoneSettingsDesc".Translate(),
				action = delegate
				{
					SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
					StorageSettingsClipboard.Copy(s);
				},
				hotKey = KeyBindingDefOf.Misc4
			};
			Command_Action paste = new Command_Action();
			paste.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings", true);
			paste.defaultLabel = "CommandPasteZoneSettingsLabel".Translate();
			paste.defaultDesc = "CommandPasteZoneSettingsDesc".Translate();
			paste.action = delegate
			{
				SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
				StorageSettingsClipboard.PasteInto(s);
			};
			paste.hotKey = KeyBindingDefOf.Misc5;
			if (!StorageSettingsClipboard.HasCopiedSettings)
			{
				paste.Disable(null);
			}
			yield return paste;
		}
	}
}
