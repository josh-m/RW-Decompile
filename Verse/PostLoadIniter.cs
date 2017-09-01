using System;
using System.Collections.Generic;

namespace Verse
{
	public class PostLoadIniter
	{
		private HashSet<IExposable> saveablesToPostLoad = new HashSet<IExposable>();

		public void RegisterForPostLoadInit(IExposable s)
		{
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				Log.Error(string.Concat(new object[]
				{
					"Registered ",
					s,
					" for post load init, but current mode is ",
					Scribe.mode
				}));
				return;
			}
			if (s == null)
			{
				Log.Warning("Trying to register null in RegisterforPostLoadInit.");
				return;
			}
			if (this.saveablesToPostLoad.Contains(s))
			{
				Log.Warning("Tried to register in RegisterforPostLoadInit when already registered: " + s);
				return;
			}
			this.saveablesToPostLoad.Add(s);
		}

		public void DoAllPostLoadInits()
		{
			Scribe.mode = LoadSaveMode.PostLoadInit;
			foreach (IExposable current in this.saveablesToPostLoad)
			{
				try
				{
					Scribe.loader.curParent = current;
					Scribe.loader.curPathRelToParent = null;
					current.ExposeData();
				}
				catch (Exception arg)
				{
					Log.Error("Could not do PostLoadInit: " + arg);
				}
			}
			this.Clear();
			Scribe.loader.curParent = null;
			Scribe.loader.curPathRelToParent = null;
			Scribe.mode = LoadSaveMode.Inactive;
		}

		public void Clear()
		{
			this.saveablesToPostLoad.Clear();
		}
	}
}
