using System;
using System.Collections.Generic;

namespace Verse
{
	public class DiaNodeList
	{
		public string Name = "NeedsName";

		public List<DiaNodeMold> Nodes = new List<DiaNodeMold>();

		public List<string> NodeNames = new List<string>();

		public DiaNodeMold RandomNodeFromList()
		{
			List<DiaNodeMold> list = this.Nodes.ListFullCopy<DiaNodeMold>();
			foreach (string current in this.NodeNames)
			{
				list.Add(DialogDatabase.GetNodeNamed(current));
			}
			foreach (DiaNodeMold current2 in list)
			{
				if (current2.unique && current2.used)
				{
					list.Remove(current2);
				}
			}
			return list.RandomElement<DiaNodeMold>();
		}
	}
}
