using System;
using System.Collections.Generic;

namespace Verse
{
	public static class LoadedObjectDirectory
	{
		private static Dictionary<string, ILoadReferenceable> allObjectsByLoadID = new Dictionary<string, ILoadReferenceable>();

		public static void Clear()
		{
			LoadedObjectDirectory.allObjectsByLoadID.Clear();
		}

		public static void RegisterLoaded(ILoadReferenceable reffable)
		{
			if (Prefs.DevMode)
			{
				string text = "[excepted]";
				try
				{
					text = reffable.GetUniqueLoadID();
				}
				catch (Exception)
				{
				}
				string text2 = "[excepted]";
				try
				{
					text2 = reffable.ToString();
				}
				catch (Exception)
				{
				}
				ILoadReferenceable loadReferenceable;
				if (LoadedObjectDirectory.allObjectsByLoadID.TryGetValue(reffable.GetUniqueLoadID(), out loadReferenceable))
				{
					Log.Error(string.Concat(new object[]
					{
						"Cannot register ",
						reffable.GetType(),
						" ",
						text2,
						", (id=",
						text,
						" in loaded object directory. Id already used by ",
						loadReferenceable.GetType(),
						" ",
						loadReferenceable.ToString(),
						"."
					}));
					return;
				}
			}
			try
			{
				LoadedObjectDirectory.allObjectsByLoadID.Add(reffable.GetUniqueLoadID(), reffable);
			}
			catch (Exception ex)
			{
				string text3 = "[excepted]";
				try
				{
					text3 = reffable.GetUniqueLoadID();
				}
				catch (Exception)
				{
				}
				string text4 = "[excepted]";
				try
				{
					text4 = reffable.ToString();
				}
				catch (Exception)
				{
				}
				Log.Error(string.Concat(new object[]
				{
					"Exception registering ",
					reffable.GetType(),
					" ",
					text4,
					" in loaded object directory with unique load ID ",
					text3,
					": ",
					ex
				}));
			}
		}

		public static T ObjectWithLoadID<T>(string loadID)
		{
			if (loadID.NullOrEmpty() || loadID == "null")
			{
				return default(T);
			}
			ILoadReferenceable loadReferenceable;
			if (LoadedObjectDirectory.allObjectsByLoadID.TryGetValue(loadID, out loadReferenceable))
			{
				if (loadReferenceable == null)
				{
					return default(T);
				}
				try
				{
					T result = (T)((object)loadReferenceable);
					return result;
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception getting object with load id ",
						loadID,
						" of type ",
						typeof(T),
						". What we loaded was ",
						loadReferenceable.ToString(),
						". Exception:\n",
						ex.ToString()
					}));
					T result = default(T);
					return result;
				}
			}
			Log.Warning(string.Concat(new object[]
			{
				"Could not resolve reference to object with loadID ",
				loadID,
				" of type ",
				typeof(T),
				". Was it compressed away, destroyed, had no ID number, or not saved/loaded right?"
			}));
			return default(T);
		}
	}
}
