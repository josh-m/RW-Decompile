using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class BackstoryDatabase
	{
		public static Dictionary<string, Backstory> allBackstories = new Dictionary<string, Backstory>();

		public static void Clear()
		{
			BackstoryDatabase.allBackstories.Clear();
		}

		public static void ReloadAllBackstories()
		{
			foreach (Backstory current in XmlLoader.LoadXmlDataInResourcesFolder<Backstory>("Backstories/Shuffled"))
			{
				current.PostLoad();
				current.ResolveReferences();
				foreach (string current2 in current.ConfigErrors(false))
				{
					Log.Error(current.Title + ": " + current2);
				}
				BackstoryDatabase.AddBackstory(current);
			}
			SolidBioDatabase.LoadAllBios();
		}

		public static void AddBackstory(Backstory bs)
		{
			BackstoryHardcodedData.InjectHardcodedData(bs);
			if (BackstoryDatabase.allBackstories.ContainsKey(bs.identifier))
			{
				Log.Error(string.Concat(new string[]
				{
					"Backstory ",
					bs.Title,
					" has same unique save key ",
					bs.identifier,
					" as old backstory ",
					BackstoryDatabase.allBackstories[bs.identifier].Title
				}));
				return;
			}
			BackstoryDatabase.allBackstories.Add(bs.identifier, bs);
		}

		public static bool TryGetWithIdentifier(string identifier, out Backstory bs)
		{
			return BackstoryDatabase.allBackstories.TryGetValue(identifier, out bs);
		}

		public static Backstory RandomBackstory(BackstorySlot slot)
		{
			return (from bs in BackstoryDatabase.allBackstories
			where bs.Value.slot == slot
			select bs).RandomElement<KeyValuePair<string, Backstory>>().Value;
		}
	}
}
