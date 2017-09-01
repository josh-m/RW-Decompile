using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Flickable : CompProperties
	{
		public string commandTexture = "UI/Commands/DesirePower";

		public string commandLabelKey = "CommandDesignateTogglePowerLabel";

		public string commandDescKey = "CommandDesignateTogglePowerDesc";

		public CompProperties_Flickable()
		{
			this.compClass = typeof(CompFlickable);
		}
	}
}
