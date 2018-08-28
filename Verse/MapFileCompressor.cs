using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class MapFileCompressor : IExposable
	{
		private Map map;

		private byte[] compressedData;

		public CompressibilityDecider compressibilityDecider;

		public MapFileCompressor(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			DataExposeUtility.ByteArray(ref this.compressedData, "compressedThingMap");
		}

		public void BuildCompressedString()
		{
			this.compressibilityDecider = new CompressibilityDecider(this.map);
			this.compressibilityDecider.DetermineReferences();
			this.compressedData = MapSerializeUtility.SerializeUshort(this.map, new Func<IntVec3, ushort>(this.HashValueForSquare));
		}

		private ushort HashValueForSquare(IntVec3 curSq)
		{
			ushort num = 0;
			foreach (Thing current in this.map.thingGrid.ThingsAt(curSq))
			{
				if (current.IsSaveCompressible())
				{
					if (num != 0)
					{
						Log.Error(string.Concat(new object[]
						{
							"Found two compressible things in ",
							curSq,
							". The last was ",
							current
						}), false);
					}
					num = current.def.shortHash;
				}
			}
			return num;
		}

		public IEnumerable<Thing> ThingsToSpawnAfterLoad()
		{
			Dictionary<ushort, ThingDef> thingDefsByShortHash = new Dictionary<ushort, ThingDef>();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (thingDefsByShortHash.ContainsKey(current.shortHash))
				{
					Log.Error(string.Concat(new object[]
					{
						"Hash collision between ",
						current,
						" and  ",
						thingDefsByShortHash[current.shortHash],
						": both have short hash ",
						current.shortHash
					}), false);
				}
				else
				{
					thingDefsByShortHash.Add(current.shortHash, current);
				}
			}
			int major = VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion);
			int minor = VersionControl.MinorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion);
			List<Thing> loadables = new List<Thing>();
			MapSerializeUtility.LoadUshort(this.compressedData, this.map, delegate(IntVec3 c, ushort val)
			{
				if (val == 0)
				{
					return;
				}
				ThingDef thingDef = BackCompatibility.BackCompatibleThingDefWithShortHash_Force(val, major, minor);
				if (thingDef == null)
				{
					try
					{
						thingDef = thingDefsByShortHash[val];
					}
					catch (KeyNotFoundException)
					{
						ThingDef thingDef2 = BackCompatibility.BackCompatibleThingDefWithShortHash(val);
						if (thingDef2 != null)
						{
							thingDef = thingDef2;
							thingDefsByShortHash.Add(val, thingDef2);
						}
						else
						{
							Log.Error("Map compressor decompression error: No thingDef with short hash " + val + ". Adding as null to dictionary.", false);
							thingDefsByShortHash.Add(val, null);
						}
					}
				}
				if (thingDef != null)
				{
					try
					{
						Thing thing = ThingMaker.MakeThing(thingDef, null);
						thing.SetPositionDirect(c);
						loadables.Add(thing);
					}
					catch (Exception arg)
					{
						Log.Error("Could not instantiate compressed thing: " + arg, false);
					}
				}
			});
			return loadables;
		}
	}
}
