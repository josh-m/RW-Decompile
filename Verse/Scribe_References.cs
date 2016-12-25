using System;
using System.Xml;

namespace Verse
{
	public static class Scribe_References
	{
		public static void LookReference<T>(ref T refee, string label, bool saveDestroyedThings = false) where T : ILoadReferenceable
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (refee == null)
				{
					Scribe.WriteElement(label, "null");
					return;
				}
				Thing thing = refee as Thing;
				if (thing != null && Scribe_References.CheckSaveReferenceToDestroyedThing(thing, label, saveDestroyedThings))
				{
					return;
				}
				if (UnityData.isDebugBuild && thing != null)
				{
					if (!thing.def.HasThingIDNumber)
					{
						Log.Error("Trying to cross-reference save Thing which lacks ID number: " + refee);
						Scribe.WriteElement(label, "null");
						return;
					}
					if (thing.IsSaveCompressible())
					{
						Log.Error("Trying to save a reference to a thing that will be compressed away: " + refee);
						Scribe.WriteElement(label, "null");
						return;
					}
				}
				string uniqueLoadID = refee.GetUniqueLoadID();
				Scribe.WriteElement(label, uniqueLoadID);
				DebugLoadIDsSavingErrorsChecker.RegisterReferenced(refee, label);
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				XmlNode xmlNode = Scribe.curParent[label];
				string targetLoadID;
				if (xmlNode != null)
				{
					targetLoadID = xmlNode.InnerText;
				}
				else
				{
					targetLoadID = null;
				}
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(targetLoadID, typeof(T));
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				refee = CrossRefResolver.NextResolvedRef<T>();
			}
		}

		public static bool CheckSaveReferenceToDestroyedThing(Thing th, string label, bool saveDestroyedThings)
		{
			if (!th.Destroyed)
			{
				return false;
			}
			if (!saveDestroyedThings)
			{
				Scribe.WriteElement(label, "null");
				return true;
			}
			if (th.Discarded)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Trying to save reference to a discarded thing ",
					th,
					" with saveDestroyedThings=true. This means that it's not deep-saved anywhere and is no longer managed by anything in the code, so saving its reference will always fail. , label=",
					label
				}));
				Scribe.WriteElement(label, "null");
				return true;
			}
			return false;
		}
	}
}
