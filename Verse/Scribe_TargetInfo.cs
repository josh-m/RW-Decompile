using RimWorld.Planet;
using System;

namespace Verse
{
	public static class Scribe_TargetInfo
	{
		public static void LookTargetInfo(ref LocalTargetInfo value, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, false, label, LocalTargetInfo.Invalid);
		}

		public static void LookTargetInfo(ref LocalTargetInfo value, bool saveDestroyedThings, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, saveDestroyedThings, label, LocalTargetInfo.Invalid);
		}

		public static void LookTargetInfo(ref LocalTargetInfo value, string label, LocalTargetInfo defaultValue)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, false, label, defaultValue);
		}

		public static void LookTargetInfo(ref LocalTargetInfo value, bool saveDestroyedThings, string label, LocalTargetInfo defaultValue)
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
				value = ScribeExtractor.LocalTargetInfoFromNode(Scribe.curParent[label], defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveLocalTargetInfo(value);
			}
		}

		public static void LookTargetInfo(ref TargetInfo value, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, false, label, TargetInfo.Invalid);
		}

		public static void LookTargetInfo(ref TargetInfo value, bool saveDestroyedThings, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, saveDestroyedThings, label, TargetInfo.Invalid);
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
					if (!value.HasThing && value.Cell.IsValid && (value.Map == null || !Find.Maps.Contains(value.Map)))
					{
						Scribe.WriteElement(label, "null");
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
				value = ScribeExtractor.ResolveTargetInfo(value);
			}
		}

		public static void LookTargetInfo(ref GlobalTargetInfo value, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, false, label, GlobalTargetInfo.Invalid);
		}

		public static void LookTargetInfo(ref GlobalTargetInfo value, bool saveDestroyedThings, string label)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, saveDestroyedThings, label, GlobalTargetInfo.Invalid);
		}

		public static void LookTargetInfo(ref GlobalTargetInfo value, string label, GlobalTargetInfo defaultValue)
		{
			Scribe_TargetInfo.LookTargetInfo(ref value, false, label, defaultValue);
		}

		public static void LookTargetInfo(ref GlobalTargetInfo value, bool saveDestroyedThings, string label, GlobalTargetInfo defaultValue)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!value.Equals(defaultValue))
				{
					if (value.Thing != null && Scribe_References.CheckSaveReferenceToDestroyedThing(value.Thing, label, saveDestroyedThings))
					{
						return;
					}
					if (value.WorldObject != null && !value.WorldObject.Spawned)
					{
						Scribe.WriteElement(label, "null");
						return;
					}
					if (!value.HasThing && !value.HasWorldObject && value.Cell.IsValid && (value.Map == null || !Find.Maps.Contains(value.Map)))
					{
						Scribe.WriteElement(label, "null");
						return;
					}
					Scribe.WriteElement(label, value.ToString());
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.GlobalTargetInfoFromNode(Scribe.curParent[label], defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveGlobalTargetInfo(value);
			}
		}
	}
}
