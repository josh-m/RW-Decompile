using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class NativeVerbPropertiesDatabase
	{
		public static List<VerbProperties> allVerbDefs;

		static NativeVerbPropertiesDatabase()
		{
			NativeVerbPropertiesDatabase.allVerbDefs = VerbDefsHardcodedNative.AllVerbDefs().ToList<VerbProperties>();
		}

		public static VerbProperties VerbWithCategory(VerbCategory id)
		{
			VerbProperties verbProperties = (from v in NativeVerbPropertiesDatabase.allVerbDefs
			where v.category == id
			select v).FirstOrDefault<VerbProperties>();
			if (verbProperties == null)
			{
				Log.Error("Failed to find Verb with id " + id);
			}
			return verbProperties;
		}
	}
}
