using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker
	{
		public PawnRelationDef def;

		public virtual bool InRelation(Pawn me, Pawn other)
		{
			if (this.def.implied)
			{
				throw new NotImplementedException(this.def + " lacks InRelation implementation.");
			}
			return me.relations.DirectRelationExists(this.def, other);
		}

		public virtual float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return 0f;
		}

		public virtual void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			if (!this.def.implied)
			{
				generated.relations.AddDirectRelation(this.def, other);
				return;
			}
			throw new NotImplementedException(this.def + " lacks CreateRelation implementation.");
		}

		public float BaseGenerationChanceFactor(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			float num = 1f;
			if (generated.Faction != other.Faction)
			{
				num *= 0.65f;
			}
			if (generated.HostileTo(other))
			{
				num *= 0.7f;
			}
			if (other.Faction != null && other.Faction.IsPlayer)
			{
				num *= request.ColonistRelationChanceFactor;
			}
			TechLevel techLevel = (generated.Faction == null) ? TechLevel.Undefined : generated.Faction.def.techLevel;
			TechLevel techLevel2 = (other.Faction == null) ? TechLevel.Undefined : other.Faction.def.techLevel;
			if (techLevel != TechLevel.Undefined && techLevel2 != TechLevel.Undefined && techLevel != techLevel2)
			{
				num *= 0.85f;
			}
			if ((techLevel.IsNeolithicOrWorse() && !techLevel2.IsNeolithicOrWorse()) || (!techLevel.IsNeolithicOrWorse() && techLevel2.IsNeolithicOrWorse()))
			{
				num *= 0.03f;
			}
			return num;
		}
	}
}
