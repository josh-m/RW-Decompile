using System;
using System.Collections.Generic;

namespace Verse
{
	public static class DebugLoadIDsSavingErrorsChecker
	{
		private struct ReferencedObject : IEquatable<DebugLoadIDsSavingErrorsChecker.ReferencedObject>
		{
			public string loadID;

			public string label;

			public ReferencedObject(string loadID, string label)
			{
				this.loadID = loadID;
				this.label = label;
			}

			public override bool Equals(object obj)
			{
				return obj is DebugLoadIDsSavingErrorsChecker.ReferencedObject && this.Equals((DebugLoadIDsSavingErrorsChecker.ReferencedObject)obj);
			}

			public bool Equals(DebugLoadIDsSavingErrorsChecker.ReferencedObject other)
			{
				return this.loadID == other.loadID && this.label == other.label;
			}

			public override int GetHashCode()
			{
				int seed = 0;
				seed = Gen.HashCombine<string>(seed, this.loadID);
				return Gen.HashCombine<string>(seed, this.label);
			}

			public static bool operator ==(DebugLoadIDsSavingErrorsChecker.ReferencedObject lhs, DebugLoadIDsSavingErrorsChecker.ReferencedObject rhs)
			{
				return lhs.Equals(rhs);
			}

			public static bool operator !=(DebugLoadIDsSavingErrorsChecker.ReferencedObject lhs, DebugLoadIDsSavingErrorsChecker.ReferencedObject rhs)
			{
				return !(lhs == rhs);
			}
		}

		private static HashSet<string> deepSaved = new HashSet<string>();

		private static HashSet<DebugLoadIDsSavingErrorsChecker.ReferencedObject> referenced = new HashSet<DebugLoadIDsSavingErrorsChecker.ReferencedObject>();

		public static void Clear()
		{
			if (!Prefs.DevMode)
			{
				return;
			}
			DebugLoadIDsSavingErrorsChecker.deepSaved.Clear();
			DebugLoadIDsSavingErrorsChecker.referenced.Clear();
		}

		public static void CheckForErrorsAndClear()
		{
			if (!Prefs.DevMode)
			{
				return;
			}
			if (!Scribe.writingForDebug)
			{
				foreach (DebugLoadIDsSavingErrorsChecker.ReferencedObject current in DebugLoadIDsSavingErrorsChecker.referenced)
				{
					if (!DebugLoadIDsSavingErrorsChecker.deepSaved.Contains(current.loadID))
					{
						Log.Warning(string.Concat(new string[]
						{
							"Object with load ID ",
							current.loadID,
							" is referenced (xml node name: ",
							current.label,
							") but is not deep-saved. This will cause errors during loading."
						}));
					}
				}
			}
			DebugLoadIDsSavingErrorsChecker.Clear();
		}

		public static void RegisterDeepSaved(object obj, string label)
		{
			if (!Prefs.DevMode)
			{
				return;
			}
			if (obj == null)
			{
				return;
			}
			ILoadReferenceable loadReferenceable = obj as ILoadReferenceable;
			if (loadReferenceable != null && !DebugLoadIDsSavingErrorsChecker.deepSaved.Add(loadReferenceable.GetUniqueLoadID()))
			{
				Log.Warning(string.Concat(new string[]
				{
					"DebugLoadIDsSavingErrorsChecker error: tried to register deep-saved object with loadID ",
					loadReferenceable.GetUniqueLoadID(),
					", but it's already here. label=",
					label,
					" (not cleared after the previous save? different objects have the same load ID? the same object is deep-saved twice?)"
				}));
			}
		}

		public static void RegisterReferenced(ILoadReferenceable obj, string label)
		{
			if (!Prefs.DevMode)
			{
				return;
			}
			if (obj == null)
			{
				return;
			}
			DebugLoadIDsSavingErrorsChecker.referenced.Add(new DebugLoadIDsSavingErrorsChecker.ReferencedObject(obj.GetUniqueLoadID(), label));
		}
	}
}
