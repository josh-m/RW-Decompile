using System;
using System.Collections.Generic;

namespace Verse
{
	public static class CrossRefResolver
	{
		private static List<IExposable> crossReferencingExposables = new List<IExposable>();

		public static void RegisterForCrossRefResolve(IExposable s)
		{
			if (s == null)
			{
				return;
			}
			if (DebugViewSettings.logMapLoad)
			{
				LogSimple.Message("RegisterForCrossRefResolve " + ((s == null) ? "null" : s.GetType().ToString()));
			}
			CrossRefResolver.crossReferencingExposables.Add(s);
		}

		public static void ResolveAllCrossReferences()
		{
			Scribe.mode = LoadSaveMode.ResolvingCrossRefs;
			if (DebugViewSettings.logMapLoad)
			{
				LogSimple.Message("==================Register the saveables all so we can find them later");
			}
			foreach (IExposable current in CrossRefResolver.crossReferencingExposables)
			{
				ILoadReferenceable loadReferenceable = current as ILoadReferenceable;
				if (loadReferenceable != null)
				{
					if (DebugViewSettings.logMapLoad)
					{
						LogSimple.Message("RegisterLoaded " + loadReferenceable.GetType());
					}
					LoadedObjectDirectory.RegisterLoaded(loadReferenceable);
				}
			}
			if (DebugViewSettings.logMapLoad)
			{
				LogSimple.Message("==================Fill all cross-references to the saveables");
			}
			foreach (IExposable current2 in CrossRefResolver.crossReferencingExposables)
			{
				if (DebugViewSettings.logMapLoad)
				{
					LogSimple.Message("ResolvingCrossRefs ExposeData " + current2.GetType());
				}
				current2.ExposeData();
			}
			CrossRefResolver.Clear();
		}

		public static T NextResolvedRef<T>() where T : ILoadReferenceable
		{
			string next = LoadIDsWantedBank.GetNext(typeof(T));
			return LoadedObjectDirectory.ObjectWithLoadID<T>(next);
		}

		public static List<T> NextResolvedRefList<T>()
		{
			List<string> nextList = LoadIDsWantedBank.GetNextList();
			List<T> list = new List<T>();
			foreach (string current in nextList)
			{
				list.Add(LoadedObjectDirectory.ObjectWithLoadID<T>(current));
			}
			return list;
		}

		public static void Clear()
		{
			LoadIDsWantedBank.ConfirmClear();
			CrossRefResolver.crossReferencingExposables.Clear();
			LoadedObjectDirectory.Clear();
		}
	}
}
