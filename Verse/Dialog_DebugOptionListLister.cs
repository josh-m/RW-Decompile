using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class Dialog_DebugOptionListLister : Dialog_DebugOptionLister
	{
		protected List<DebugMenuOption> options;

		public Dialog_DebugOptionListLister(IEnumerable<DebugMenuOption> options)
		{
			this.options = options.ToList<DebugMenuOption>();
		}

		protected override void DoListingItems()
		{
			foreach (DebugMenuOption current in this.options)
			{
				if (current.mode == DebugMenuOptionMode.Action)
				{
					base.DebugAction(current.label, current.method);
				}
				if (current.mode == DebugMenuOptionMode.Tool)
				{
					base.DebugToolMap(current.label, current.method);
				}
			}
		}

		public static void ShowSimpleDebugMenu<T>(IEnumerable<T> elements, Func<T, string> label, Action<T> chosen)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (T t in elements)
			{
				list.Add(new DebugMenuOption(label(t), DebugMenuOptionMode.Action, delegate
				{
					chosen(t);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}
	}
}
