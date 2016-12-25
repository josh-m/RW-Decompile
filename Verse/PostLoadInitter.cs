using System;
using System.Collections.Generic;

namespace Verse
{
	public static class PostLoadInitter
	{
		private static HashSet<IExposable> saveablesToPostLoad = new HashSet<IExposable>();

		public static void RegisterForPostLoadInit(IExposable s)
		{
			if (s == null)
			{
				Log.Warning("Trying to register null in RegisterforPostLoadInit.");
				return;
			}
			if (PostLoadInitter.saveablesToPostLoad.Contains(s))
			{
				Log.Warning("Tried to register in RegisterforPostLoadInit when already registered: " + s.ToString());
				return;
			}
			PostLoadInitter.saveablesToPostLoad.Add(s);
		}

		public static void DoAllPostLoadInits()
		{
			Scribe.mode = LoadSaveMode.PostLoadInit;
			foreach (IExposable current in PostLoadInitter.saveablesToPostLoad)
			{
				current.ExposeData();
			}
			PostLoadInitter.Clear();
			Scribe.mode = LoadSaveMode.Inactive;
		}

		public static void Clear()
		{
			PostLoadInitter.saveablesToPostLoad.Clear();
		}
	}
}
