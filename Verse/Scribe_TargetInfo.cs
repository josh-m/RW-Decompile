using System;

namespace Verse
{
	public static class Scribe_TargetInfo
	{
		public static readonly IntVec3 PackShouldLoadThingRefValue = new IntVec3(-66666, 9001, 0);

		public static void LookTargetInfo(ref TargetInfo value, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, false, label, TargetInfo.NullThing);
		}

		public static void LookTargetInfo(ref TargetInfo value, bool saveDestroyedThings, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, saveDestroyedThings, label, TargetInfo.NullThing);
		}

		public static void LookTargetInfo(ref TargetInfo value, string label, TargetInfo defaultValue)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, false, label, defaultValue);
		}

		public static void LookTargetInfo(ref TargetInfo value, bool saveDestroyedThings, string label, TargetInfo defaultValue)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!value.Equals(defaultValue))
				{
					if (value.Thing != null && Scribe_References.CheckSaveReferenceToDestroyedThing(value.Thing, label, saveDestroyedThings))
					{
						return;
					}
					Scribe.WriteElement(label, value.ToString());
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.TargetInfoFromNode(Scribe.curParent[label], defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				Thing thing = CrossRefResolver.NextResolvedRef<Thing>();
				if (value.Cell == Scribe_TargetInfo.PackShouldLoadThingRefValue)
				{
					value = new TargetInfo(thing);
				}
			}
		}
	}
}
