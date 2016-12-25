using System;
using System.Collections.Generic;

namespace Verse.Grammar
{
	public class Rule_File : Rule
	{
		private string path;

		private List<string> pathList = new List<string>();

		private List<string> cachedStrings = new List<string>();

		public override float BaseSelectionWeight
		{
			get
			{
				return (float)this.cachedStrings.Count;
			}
		}

		public override string Generate()
		{
			return this.cachedStrings.RandomElement<string>();
		}

		public override void Init()
		{
			if (!this.path.NullOrEmpty())
			{
				this.LoadStringsFromFile(this.path);
			}
			foreach (string current in this.pathList)
			{
				this.LoadStringsFromFile(current);
			}
		}

		private void LoadStringsFromFile(string filePath)
		{
			List<string> list;
			if (Translator.TryGetTranslatedStringsForFile(filePath, out list))
			{
				foreach (string current in list)
				{
					this.cachedStrings.Add(current);
				}
			}
		}

		public override string ToString()
		{
			if (!this.path.NullOrEmpty())
			{
				return string.Concat(new object[]
				{
					this.keyword,
					"->(",
					this.cachedStrings.Count,
					" strings from file: ",
					this.path,
					")"
				});
			}
			if (this.pathList.Count > 0)
			{
				return string.Concat(new object[]
				{
					this.keyword,
					"->(",
					this.cachedStrings.Count,
					" strings from ",
					this.pathList.Count,
					" files)"
				});
			}
			return this.keyword + "->(Rule_File with no configuration)";
		}
	}
}
