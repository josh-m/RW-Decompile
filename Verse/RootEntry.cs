using RimWorld;
using System;

namespace Verse
{
	public class RootEntry : Root
	{
		public override void Update()
		{
			base.Update();
			if (LongEventHandler.ShouldWaitForEvent)
			{
				return;
			}
			MusicManagerEntry.MusicManagerEntryUpdate();
		}
	}
}
