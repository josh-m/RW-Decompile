using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnCapacityFactor
	{
		public PawnCapacityDef capacity;

		public float weight = 1f;

		public float max = 9999f;

		public bool useReciprocal;

		public float allowedDefect;

		private const float MaxReciprocalFactor = 5f;

		public float GetFactor(float capacityEfficiency)
		{
			float num = capacityEfficiency;
			if (this.allowedDefect != 0f && num < 1f)
			{
				num = Mathf.InverseLerp(0f, 1f - this.allowedDefect, num);
			}
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
