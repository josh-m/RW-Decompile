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
					Log.Error(current.title + ": " + current2);
				}
				BackstoryDatabase.AddBackstory(current);
			}
			SolidBioDatabase.LoadAllBios();
		}

		public static void AddBackstory(Backstory bs)
		{
			BackstoryHardcodedData.InjectHardcodedData(bs);
			if (BackstoryDatabase.allBackstories.ContainsKey(bs.uniqueSaveKey))
			{
				Log.Error(string.Concat(new string[]
				{
					"Backstory ",
					bs.title,
					" has same unique save key ",
					bs.uniqueSaveKey,
					" as old backstory ",
					BackstoryDatabase.allBackstories[bs.uniqueSaveKey].title
				}));
				return;
			}
			BackstoryDatabase.allBackstories.Add(bs.uniqueSaveKey, bs);
		}

		public static Backstory GetWithKey(string saveKey)
		{
			if (saveKey == null)
			{
				Log.Error("Null backstory defName requested. Giving random...");
				return BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
			}
			Backstory result;
			if (BackstoryDatabase.allBackstories.TryGetValue(saveKey, out result))
			{
				return result;
			}
			Log.Error("Backstory named " + saveKey + " not found. Giving random...");
			return BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
		}

		public static Backstory RandomBackstory(BackstorySlot slot)
		{
			return (from bs in BackstoryDatabase.allBackstories
			where bs.Value.slot == slot
			select bs).RandomElement<KeyValuePair<string, Backstory>>().Value;
		}
	}
}
