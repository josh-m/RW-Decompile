using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Blueprint_Build : Blueprint
	{
		public ThingDef stuffToUse;

		public override string Label
		{
			get
			{
				string label = base.Label;
				if (this.stuffToUse != null)
				{
					return "ThingMadeOfStuffLabel".Translate(new object[]
					{
						this.stuffToUse.LabelAsStuff,
						label
					});
				}
				return label;
			}
		}

		protected override float WorkTotal
		{
			get
			{
				return this.def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToMake, this.stuffToUse);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<ThingDef>(ref this.stuffToUse, "stuffToUse");
		}

		public override ThingDef UIStuff()
		{
			return this.stuffToUse;
		}

		public override List<ThingCount> MaterialsNeeded()
		{
			return this.def.entityDefToBuild.CostListAdjusted(this.stuffToUse, true);
		}

		protected override Thing MakeSolidThing()
		{
			return ThingMaker.MakeThing(this.def.entityDefToBuild.frameDef, this.stuffToUse);
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo c in base.GetGizmos())
			{
				yield return c;
			}
			Command buildCopy = BuildCopyCommandUtility.BuildCopyCommand(this.def.entityDefToBuild, this.stuffToUse);
			if (buildCopy != null)
			{
				yield return buildCopy;
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetInspectString());
			stringBuilder.AppendLine("ContainedResources".Translate() + ":");
			foreach (ThingCount current in this.MaterialsNeeded())
			{
				stringBuilder.AppendLine(current.thingDef.LabelCap + ": 0 / " + current.count);
			}
			return stringBuilder.ToString();
		}
	}
}
