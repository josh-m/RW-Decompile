using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalHiveCanReproduce : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			Hive hive = pawn.mindState.duty.focus.Thing as Hive;
			return hive != null && hive.GetComp<CompSpawnerHives>().canSpawnHives;
		}
	}
}
