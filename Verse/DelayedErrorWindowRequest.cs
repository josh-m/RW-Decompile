using System;
using System.Collections.Generic;

namespace Verse
{
	public static class DelayedErrorWindowRequest
	{
		private struct Request
		{
			public string text;

			public string title;
		}

		private static List<DelayedErrorWindowRequest.Request> requests = new List<DelayedErrorWindowRequest.Request>();

		public static void DelayedErrorWindowRequestOnGUI()
		{
			try
			{
				for (int i = 0; i < DelayedErrorWindowRequest.requests.Count; i++)
				{
					WindowStack arg_47_0 = Find.WindowStack;
					string title = DelayedErrorWindowRequest.requests[i].title;
					arg_47_0.Add(new Dialog_MessageBox(DelayedErrorWindowRequest.requests[i].text, "OK".Translate(), null, null, null, title, false));
				}
			}
			finally
			{
				DelayedErrorWindowRequest.requests.Clear();
			}
		}

		public static void Add(string text, string title = null)
		{
			DelayedErrorWindowRequest.Request item = default(DelayedErrorWindowRequest.Request);
			item.text = text;
			item.title = title;
			DelayedErrorWindowRequest.requests.Add(item);
		}
	}
}
