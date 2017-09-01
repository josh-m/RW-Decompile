using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Siege : LordToilData
	{
		public IntVec3 siegeCenter;

		public float baseRadius = 16f;

		public float blueprintPoints;

		public float desiredBuilderFraction = 0.5f;

		public List<Blueprint> blueprints = new List<Blueprint>();

		public override void ExposeData()
		{
			Scribe_Values.Look<IntVec3>(ref this.siegeCenter, "siegeCenter", default(IntVec3), false);
			Scribe_Values.Look<float>(ref this.baseRadius, "baseRadius", 16f, false);
			Scribe_Values.Look<float>(ref this.blueprintPoints, "blueprintPoints", 0f, false);
			Scribe_Values.Look<float>(ref this.desiredBuilderFraction, "desiredBuilderFraction", 0.5f, false);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.blueprints.RemoveAll((Blueprint blue) => blue.Destroyed);
			}
			Scribe_Collections.Look<Blueprint>(ref this.blueprints, "blueprints", LookMode.Reference, new object[0]);
		}
	}
}
