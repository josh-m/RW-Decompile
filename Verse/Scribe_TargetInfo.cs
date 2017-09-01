using RimWorld.Planet;
using System;

namespace Verse
{
	public static class Scribe_TargetInfo
	{
		public static void Look(ref LocalTargetInfo value, string label)
		{
			Scribe_TargetInfo.Look(ref value, false, label, LocalTargetInfo.Invalid);
		}

		public static void Look(ref LocalTargetInfo value, bool saveDestroyedThings, string label)
		{
			Scribe_TargetInfo.Look(ref value, saveDestroyedThings, label, LocalTargetInfo.Invalid);
		}

		public static void Look(ref LocalTargetInfo value, string label, LocalTargetInfo defaultValue)
		{
			Scribe_TargetInfo.Look(ref value, false, label, defaultValue);
		}

		public static void Look(ref LocalTargetInfo value, bool saveDestroyedThings, string label, LocalTargetInfo defaultValue)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!value.Equals(defaultValue))
				{
					if (value.Thing != null && Scribe_References.CheckSaveReferenceToDestroyedThing(value.Thing, label, saveDestroyedThings))
					{
						return;
					}
					Scribe.saver.WriteElement(label, value.ToString());
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.LocalTargetInfoFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveLocalTargetInfo(value, label);
			}
		}

		public static void Look(ref TargetInfo value, string label)
		{
			Scribe_TargetInfo.Look(ref value, false, label, TargetInfo.Invalid);
		}

		public static void Look(ref TargetInfo value, bool saveDestroyedThings, string label)
		{
			Scribe_TargetInfo.Look(ref value, saveDestroyedThings, label, TargetInfo.Invalid);
		}

		public static void Look(ref TargetInfo value, string label, TargetInfo defaultValue)
		{
			Scribe_TargetInfo.Look(ref value, false, label, defaultValue);
		}

		public static void Look(ref TargetInfo value, bool saveDestroyedThings, string label, TargetInfo defaultValue)
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
						Scribe.saver.WriteElement(label, "null");
						return;
					}
					Scribe.saver.WriteElement(label, value.ToString());
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.TargetInfoFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveTargetInfo(value, label);
			}
		}

		public static void Look(ref GlobalTargetInfo value, string label)
		{
			Scribe_TargetInfo.Look(ref value, false, label, GlobalTargetInfo.Invalid);
		}

		public static void Look(ref GlobalTargetInfo value, bool saveDestroyedThings, string label)
		{
			Scribe_TargetInfo.Look(ref value, saveDestroyedThings, label, GlobalTargetInfo.Invalid);
		}

		public static void Look(ref GlobalTargetInfo value, string label, GlobalTargetInfo defaultValue)
		{
			Scribe_TargetInfo.Look(ref value, false, label, defaultValue);
		}

		public static void Look(ref GlobalTargetInfo value, bool saveDestroyedThings, string label, GlobalTargetInfo defaultValue)
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
						Scribe.saver.WriteElement(label, "null");
						return;
					}
					if (!value.HasThing && !value.HasWorldObject && value.Cell.IsValid && (value.Map == null || !Find.Maps.Contains(value.Map)))
					{
						Scribe.saver.WriteElement(label, "null");
						return;
					}
					Scribe.saver.WriteElement(label, value.ToString());
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.GlobalTargetInfoFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveGlobalTargetInfo(value, label);
			}
		}
	}
}
