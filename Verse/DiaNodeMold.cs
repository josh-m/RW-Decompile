using System;
using System.Collections.Generic;

namespace Verse
{
	public class DiaNodeMold
	{
		public string name = "Unnamed";

		public bool unique;

		public List<string> texts = new List<string>();

		public List<DiaOptionMold> optionList = new List<DiaOptionMold>();

		[Unsaved]
		public bool isRoot = true;

		[Unsaved]
		public bool used;

		[Unsaved]
		public DiaNodeType nodeType;

		public void PostLoad()
		{
			int num = 0;
			foreach (string current in this.texts.ListFullCopy<string>())
			{
				this.texts[num] = current.Replace("\\n", Environment.NewLine);
				num++;
			}
			foreach (DiaOptionMold current2 in this.optionList)
			{
				foreach (DiaNodeMold current3 in current2.ChildNodes)
				{
					current3.PostLoad();
				}
			}
		}
	}
}
