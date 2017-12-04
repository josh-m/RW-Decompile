using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class FeatureDef : Def
	{
		public Type workerClass = typeof(FeatureWorker);

		public float order;

		public int minSize = 50;

		public int maxSize = 2147483647;

		public bool canTouchWorldEdge = true;

		public RulePackDef nameMaker;

		public int maxPossiblyAllowedSizeToTake = 30;

		public float maxPossiblyAllowedSizePctOfMeToTake = 0.5f;

		public List<BiomeDef> rootBiomes = new List<BiomeDef>();

		public List<BiomeDef> acceptableBiomes = new List<BiomeDef>();

		public int maxSpaceBetweenRootGroups = 5;

		public int minRootGroupsInCluster = 3;

		public int minRootGroupSize = 10;

		public int maxRootGroupSize = 2147483647;

		public int maxPassageWidth = 3;

		public float maxPctOfWholeArea = 0.1f;

		[Unsaved]
		private FeatureWorker workerInt;

		public FeatureWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (FeatureWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
