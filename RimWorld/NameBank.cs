using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class NameBank
	{
		public PawnNameCategory nameType;

		private List<string>[,] names;

		private static readonly int numGenders = Enum.GetValues(typeof(Gender)).Length;

		private static readonly int numSlots = Enum.GetValues(typeof(PawnNameSlot)).Length;

		private IEnumerable<List<string>> AllNameLists
		{
			get
			{
				for (int i = 0; i < NameBank.numGenders; i++)
				{
					for (int j = 0; j < NameBank.numSlots; j++)
					{
						yield return this.names[i, j];
					}
				}
			}
		}

		public NameBank(PawnNameCategory ID)
		{
			this.nameType = ID;
			this.names = new List<string>[NameBank.numGenders, NameBank.numSlots];
			for (int i = 0; i < NameBank.numGenders; i++)
			{
				for (int j = 0; j < NameBank.numSlots; j++)
				{
					this.names[i, j] = new List<string>();
				}
			}
		}

		public void ErrorCheck()
		{
			foreach (List<string> current in this.AllNameLists)
			{
				List<string> list = (from x in current
				group x by x into g
				where g.Count<string>() > 1
				select g.Key).ToList<string>();
				foreach (string current2 in list)
				{
					Log.Error("Duplicated name: " + current2);
				}
				foreach (string current3 in current)
				{
					if (current3.Trim() != current3)
					{
						Log.Error("Trimmable whitespace on name: [" + current3 + "]");
					}
				}
			}
		}

		private List<string> NamesFor(Gender gender, PawnNameSlot slot)
		{
			return this.names[(int)gender, (int)slot];
		}

		public void AddNames(Gender gender, PawnNameSlot slot, IEnumerable<string> namesToAdd)
		{
			foreach (string current in namesToAdd)
			{
				this.NamesFor(gender, slot).Add(current);
			}
		}

		public void AddNamesFromFile(Gender gender, PawnNameSlot slot, string fileName)
		{
			this.AddNames(gender, slot, GenFile.LinesFromFile("Names/" + fileName));
		}

		public string GetName(Gender gender, PawnNameSlot slot)
		{
			List<string> list = this.NamesFor(gender, slot);
			int num = 0;
			if (list.Count == 0)
			{
				Log.Error(string.Concat(new object[]
				{
					"Name list for gender=",
					gender,
					" slot=",
					slot,
					" is empty."
				}));
				return "Errorname";
			}
			string text;
			while (true)
			{
				text = list.RandomElement<string>();
				if (!NameUseChecker.NameWordIsUsed(text))
				{
					break;
				}
				num++;
				if (num > 50)
				{
					return text;
				}
			}
			return text;
		}
	}
}
