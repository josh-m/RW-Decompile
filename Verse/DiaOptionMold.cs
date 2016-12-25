using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Verse
{
	public class DiaOptionMold
	{
		public string Text = "OK".Translate();

		[XmlElement("Node")]
		public List<DiaNodeMold> ChildNodes = new List<DiaNodeMold>();

		[XmlElement("NodeName"), DefaultValue("")]
		public List<string> ChildNodeNames = new List<string>();

		public DiaNodeMold RandomLinkNode()
		{
			List<DiaNodeMold> list = this.ChildNodes.ListFullCopy<DiaNodeMold>();
			foreach (string current in this.ChildNodeNames)
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
			if (list.Count == 0)
			{
				return null;
			}
			return list.RandomElement<DiaNodeMold>();
		}
	}
}
