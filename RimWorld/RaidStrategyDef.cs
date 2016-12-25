using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RaidStrategyDef : Def
	{
		public Type workerClass;

		public string arrivalTextFriendly;

		public string arrivalTextEnemy;

		public float pointsFactor = 1f;

		public float selectionChance = 1f;

		public string letterLabelEnemy;

		public string letterLabelFriendly;

		public List<PawnsArriveMode> arriveModes;

		public float minPawns = 1f;

		public float minDaysPassed;

		public bool pawnsCanBringFood = true;

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
