using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Verse
{
	public class Dialog_DebugOutputMenu : Dialog_DebugOptionLister
	{
		private struct DebugOutputOption
		{
			public string label;

			public string category;

			public Action action;
		}

		private List<Dialog_DebugOutputMenu.DebugOutputOption> debugOutputs = new List<Dialog_DebugOutputMenu.DebugOutputOption>();

		private const string DefaultCategory = "General";

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public Dialog_DebugOutputMenu()
		{
			this.forcePause = true;
			foreach (Type current in GenTypes.AllTypesWithAttribute<HasDebugOutputAttribute>())
			{
				MethodInfo[] methods = current.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				for (int i = 0; i < methods.Length; i++)
				{
					MethodInfo mi = methods[i];
					DebugOutputAttribute debugOutputAttribute;
					if (mi.TryGetAttribute(out debugOutputAttribute))
					{
						string label = GenText.SplitCamelCase(mi.Name);
						Action action = delegate
						{
							mi.Invoke(null, null);
						};
						CategoryAttribute categoryAttribute = null;
						string category;
						if (mi.TryGetAttribute(out categoryAttribute))
						{
							category = categoryAttribute.name;
						}
						else
						{
							category = "General";
						}
						this.debugOutputs.Add(new Dialog_DebugOutputMenu.DebugOutputOption
						{
							label = label,
							category = category,
							action = action
						});
					}
				}
			}
			this.debugOutputs = (from r in this.debugOutputs
			orderby r.category, r.label
			select r).ToList<Dialog_DebugOutputMenu.DebugOutputOption>();
		}

		protected override void DoListingItems()
		{
			string b = null;
			foreach (Dialog_DebugOutputMenu.DebugOutputOption current in this.debugOutputs)
			{
				if (current.category != b)
				{
					base.DoLabel(current.category);
					b = current.category;
				}
				Log.openOnMessage = true;
				try
				{
					base.DebugAction(current.label, current.action);
				}
				finally
				{
					Log.openOnMessage = false;
				}
			}
		}
	}
}
