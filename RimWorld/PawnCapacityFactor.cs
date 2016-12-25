using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnCapacityFactor
	{
		private const float MaxReciprocal = 5f;

		public PawnCapacityDef capacity;

		public float weight = 1f;

		public bool useReciprocal;

		public float max = 9999f;

		public float GetFactor(float capacityEfficiency)
		{
			float num = capacityEfficiency;
			if (num > this.max)
			{
				num = this.max;
			}
			if (this.useReciprocal)
			{
				if (Mathf.Abs(num) < 0.001f)
				{
					num = 5f;
				}
				else
				{
					num = Mathf.Min(1f / num, 5f);
				}
			}
			return num;
		}
	}
}
