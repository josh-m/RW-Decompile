using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class HairDef : Def
	{
		public string texPath;

		public HairGender hairGender = HairGender.Any;

		public List<string> hairTags = new List<string>();
	}
}
