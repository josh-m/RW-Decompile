using System;
using System.Collections.Generic;
using System.Xml;

namespace Verse
{
	public class PatchOperationSequence : PatchOperation
	{
		private List<PatchOperation> operations;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			foreach (PatchOperation current in this.operations)
			{
				if (!current.Apply(xml))
				{
					return false;
				}
			}
			return true;
		}
	}
}
