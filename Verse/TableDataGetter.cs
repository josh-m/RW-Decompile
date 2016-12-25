using System;

namespace Verse
{
	public class TableDataGetter<T>
	{
		public string label;

		public Func<T, string> getter;

		public TableDataGetter(string label, Func<T, string> getter)
		{
			this.label = label;
			this.getter = getter;
		}
	}
}
