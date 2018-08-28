using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace RimWorld
{
	public static class BackstoryDatabase
	{
		public static Dictionary<string, Backstory> allBackstories = new Dictionary<string, Backstory>();

		private static Dictionary<Pair<BackstorySlot, string>, List<Backstory>> shuffleableBackstoryList = new Dictionary<Pair<BackstorySlot, string>, List<Backstory>>();

		private static HashSet<Backstory> tmpUniqueBackstories = new HashSet<Backstory>();

		private static Regex regex = new Regex("^[^0-9]*");

		public static void Clear()
		{
			BackstoryDatabase.allBackstories.Clear();
		}

		public static void ReloadAllBackstories()
		{
			foreach (Backstory current in DirectXmlLoader.LoadXmlDataInResourcesFolder<Backstory>("Backstories/Shuffled"))
			{
				current.PostLoad();
				current.ResolveReferences();
				foreach (string current2 in current.ConfigErrors(false))
				{
					Log.Error(current.title + ": " + current2, false);
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
				if (bs == BackstoryDatabase.allBackstories[bs.identifier])
				{
					Log.Error("Tried to add the same backstory twice " + bs.identifier, false);
				}
				else
				{
					Log.Error(string.Concat(new string[]
					{
						"Backstory ",
						bs.title,
						" has same unique save key ",
						bs.identifier,
						" as old backstory ",
						BackstoryDatabase.allBackstories[bs.identifier].title
					}), false);
				}
				return;
			}
			BackstoryDatabase.allBackstories.Add(bs.identifier, bs);
			BackstoryDatabase.shuffleableBackstoryList.Clear();
		}

		public static bool TryGetWithIdentifier(string identifier, out Backstory bs, bool closestMatchWarning = true)
		{
			identifier = BackstoryDatabase.GetIdentifierClosestMatch(identifier, closestMatchWarning);
			return BackstoryDatabase.allBackstories.TryGetValue(identifier, out bs);
		}

		public static string GetIdentifierClosestMatch(string identifier, bool closestMatchWarning = true)
		{
			if (BackstoryDatabase.allBackstories.ContainsKey(identifier))
			{
				return identifier;
			}
			string b = BackstoryDatabase.StripNumericSuffix(identifier);
			foreach (KeyValuePair<string, Backstory> current in BackstoryDatabase.allBackstories)
			{
				Backstory value = current.Value;
				if (BackstoryDatabase.StripNumericSuffix(value.identifier) == b)
				{
					if (closestMatchWarning)
					{
						Log.Warning("Couldn't find exact match for backstory " + identifier + ", using closest match " + value.identifier, false);
					}
					return value.identifier;
				}
			}
			Log.Warning("Couldn't find exact match for backstory " + identifier + ", or any close match.", false);
			return identifier;
		}

		public static Backstory RandomBackstory(BackstorySlot slot)
		{
			return (from bs in BackstoryDatabase.allBackstories
			where bs.Value.slot == slot
			select bs).RandomElement<KeyValuePair<string, Backstory>>().Value;
		}

		public static List<Backstory> ShuffleableBackstoryList(BackstorySlot slot, string tag)
		{
			Pair<BackstorySlot, string> key = new Pair<BackstorySlot, string>(slot, tag);
			if (!BackstoryDatabase.shuffleableBackstoryList.ContainsKey(key))
			{
				BackstoryDatabase.shuffleableBackstoryList[key] = (from bs in BackstoryDatabase.allBackstories.Values
				where bs.shuffleable && bs.slot == slot && bs.spawnCategories.Contains(tag)
				select bs).ToList<Backstory>();
			}
			return BackstoryDatabase.shuffleableBackstoryList[key];
		}

		public static void ShuffleableBackstoryList(BackstorySlot slot, List<string> tags, List<Backstory> outBackstories)
		{
			outBackstories.Clear();
			if (tags.Count == 0)
			{
				return;
			}
			if (tags.Count == 1)
			{
				outBackstories.AddRange(BackstoryDatabase.ShuffleableBackstoryList(slot, tags[0]));
				return;
			}
			BackstoryDatabase.tmpUniqueBackstories.Clear();
			for (int i = 0; i < tags.Count; i++)
			{
				List<Backstory> list = BackstoryDatabase.ShuffleableBackstoryList(slot, tags[i]);
				for (int j = 0; j < list.Count; j++)
				{
					BackstoryDatabase.tmpUniqueBackstories.Add(list[j]);
				}
			}
			foreach (Backstory current in BackstoryDatabase.tmpUniqueBackstories)
			{
				outBackstories.Add(current);
			}
			BackstoryDatabase.tmpUniqueBackstories.Clear();
		}

		public static string StripNumericSuffix(string key)
		{
			return BackstoryDatabase.regex.Match(key).Captures[0].Value;
		}
	}
}
