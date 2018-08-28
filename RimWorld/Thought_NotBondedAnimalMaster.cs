using System;
using UnityEngine;

namespace RimWorld
{
	public class Thought_NotBondedAnimalMaster : Thought_Situational
	{
		private const int MaxAnimals = 3;

		protected override float BaseMoodOffset
		{
			get
			{
				return base.CurStage.baseMoodEffect * (float)Mathf.Min(((ThoughtWorker_NotBondedAnimalMaster)this.def.Worker).GetAnimalsCount(this.pawn), 3);
			}
		}
	}
}
