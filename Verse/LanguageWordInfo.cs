using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Verse
{
	public class LanguageWordInfo
	{
		private Dictionary<string, Gender> genders = new Dictionary<string, Gender>();

		private const string FolderName = "WordInfo";

		private const string GendersFolderName = "Gender";

		private const string MaleFileName = "Male.txt";

		private const string FemaleFileName = "Female.txt";

		private const string NeuterFileName = "Neuter.txt";

		private static StringBuilder tmpLowercase = new StringBuilder();

		public void LoadFrom(string path)
		{
			string path2 = Path.Combine(path, "WordInfo");
			string path3 = Path.Combine(path2, "Gender");
			this.TryLoadFromFile(Path.Combine(path3, "Male.txt"), Gender.Male);
			this.TryLoadFromFile(Path.Combine(path3, "Female.txt"), Gender.Female);
			this.TryLoadFromFile(Path.Combine(path3, "Neuter.txt"), Gender.None);
		}

		public Gender ResolveGender(string str, string fallback = null)
		{
			Gender result;
			if (!this.TryResolveGender(str, out result) && fallback != null)
			{
				this.TryResolveGender(str, out result);
			}
			return result;
		}

		private bool TryResolveGender(string str, out Gender gender)
		{
			LanguageWordInfo.tmpLowercase.Length = 0;
			for (int i = 0; i < str.Length; i++)
			{
				LanguageWordInfo.tmpLowercase.Append(char.ToLower(str[i]));
			}
			string key = LanguageWordInfo.tmpLowercase.ToString();
			if (this.genders.TryGetValue(key, out gender))
			{
				return true;
			}
			gender = Gender.Male;
			return false;
		}

		private void TryLoadFromFile(string filePath, Gender gender)
		{
			string[] array;
			try
			{
				array = File.ReadAllLines(filePath);
			}
			catch (DirectoryNotFoundException)
			{
				return;
			}
			catch (FileNotFoundException)
			{
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].NullOrEmpty() && !this.genders.ContainsKey(array[i]))
				{
					this.genders.Add(array[i], gender);
				}
			}
		}
	}
}
