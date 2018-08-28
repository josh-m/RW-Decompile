using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar
{
	public class Rule_File : Rule
	{
		[MayTranslate]
		public string path;

		[MayTranslate, TranslationCanChangeCount]
		public List<string> pathList = new List<string>();

		[Unsaved]
		private List<string> cachedStrings = new List<string>();

		public override float BaseSelectionWeight
		{
			get
			{
				return (float)this.cachedStrings.Count;
			}
		}

		public override Rule DeepCopy()
		{
			Rule_File rule_File = (Rule_File)base.DeepCopy();
			rule_File.path = this.path;
			if (this.pathList != null)
			{
				rule_File.pathList = this.pathList.ToList<string>();
			}
			if (this.cachedStrings != null)
			{
				rule_File.cachedStrings = this.cachedStrings.ToList<string>();
			}
			return rule_File;
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
