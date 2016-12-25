using System;

namespace Verse
{
	public class Scribe_Deep
	{
		public static void LookDeep<T>(ref T target, string label, params object[] ctorArgs)
		{
			Scribe_Deep.LookDeep<T>(ref target, false, label, ctorArgs);
		}

		public static void LookDeep<T>(ref T target, bool saveDestroyedThings, string label, params object[] ctorArgs)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Thing thing = target as Thing;
				if (thing != null && thing.Destroyed)
				{
					if (!saveDestroyedThings)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Deep-saving destroyed thing ",
							thing,
							" with saveDestroyedThings==false. label=",
							label
						}));
					}
					else if (thing.Discarded)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Deep-saving discarded thing ",
							thing,
							". This mode means that the thing is no longer managed by anything in the code and should not be deep-saved anywhere. (even with saveDestroyedThings==true) , label=",
							label
						}));
					}
				}
				IExposable exposable = target as IExposable;
				if (target != null && exposable == null)
				{
					Log.Error(string.Concat(new object[]
					{
						"Cannot use LookDeep to save non-IExposable non-null ",
						label,
						" of type ",
						typeof(T)
					}));
					return;
				}
				if (target == null)
				{
					Scribe.EnterNode(label);
					Scribe.WriteAttribute("IsNull", "True");
					Scribe.ExitNode();
				}
				else
				{
					Scribe.EnterNode(label);
					if (target.GetType() != typeof(T))
					{
						Scribe.WriteAttribute("Class", GenTypes.GetTypeNameWithoutIgnoredNamespaces(target.GetType()));
					}
					exposable.ExposeData();
					Scribe.ExitNode();
				}
				DebugLoadIDsSavingErrorsChecker.RegisterDeepSaved(target, label);
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				target = ScribeExtractor.SaveableFromNode<T>(Scribe.curParent[label], ctorArgs);
			}
		}
	}
}
