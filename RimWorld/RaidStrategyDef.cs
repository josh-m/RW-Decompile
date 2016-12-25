using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RaidStrategyDef : Def
	{
		public Type workerClass;

		[MustTranslate]
		public string arrivalTextFriendly;

		[MustTranslate]
		public string arrivalTextEnemy;

		public float pointsFactor = 1f;

		public float selectionChance = 1f;

		[MustTranslate]
		public string letterLabelEnemy;

		[MustTranslate]
		public string letterLabelFriendly;

		public List<PawnsArriveMode> arriveModes;

		public float minPawns = 1f;

		public float minDaysPassed;

		public bool pawnsCanBringFood;

		private RaidStrategyWorker workerInt;

		public RaidStrategyWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (RaidStrategyWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
