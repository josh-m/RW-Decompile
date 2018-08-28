using System;
using UnityEngine;

namespace RimWorld
{
	public class Thought_BondedAnimalMaster : Thought_Situational
	{
		private const int MaxAnimals = 3;

		protected override float BaseMoodOffset
		{
			get
			{
				return base.CurStage.baseMoodEffect * (float)Mathf.Min(((ThoughtWorker_BondedAnimalMaster)this.def.Worker).GetAnimalsCount(this.pawn), 3);
			}
		}
	}
}
