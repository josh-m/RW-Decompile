using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class MapConditionDef : Def
	{
		public Type conditionClass = typeof(MapCondition);

		private List<MapConditionDef> exclusiveConditions;

		public string endMessage;

		public bool canBePermanent;

		public PsychicDroneLevel droneLevel = PsychicDroneLevel.BadMedium;

		public bool preventRain;

		public bool CanCoexistWith(MapConditionDef other)
		{
			return this != other && (this.exclusiveConditions == null || !this.exclusiveConditions.Contains(other));
		}

		public static MapConditionDef Named(string defName)
		{
			return DefDatabase<MapConditionDef>.GetNamed(defName, true);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.conditionClass == null)
			{
				yield return "conditionClass is null";
			}
		}
	}
}
