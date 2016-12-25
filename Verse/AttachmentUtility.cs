using System;

namespace Verse
{
	public static class AttachmentUtility
	{
		public static Thing GetAttachment(this Thing t, ThingDef def)
		{
			CompAttachBase compAttachBase = t.TryGetComp<CompAttachBase>();
			if (compAttachBase == null)
			{
				return null;
			}
			return compAttachBase.GetAttachment(def);
		}

		public static bool HasAttachment(this Thing t, ThingDef def)
		{
			return t.GetAttachment(def) != null;
		}
	}
}
