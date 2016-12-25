using System;
using UnityEngine;

namespace Verse
{
	public static class OldInjuryUtility
	{
		private const float PainFactorLowerGaussianWidthFactor = 1.5f;

		private const float PainFactorUpperGaussianWidthFactor = 2.53f;

		private const float MaxPainFactor = 12f;

		public static float GetRandomPainFactor()
		{
			float value = Rand.GaussianAsymmetric(1f, 1.5f, 2.53f);
			return Mathf.Clamp(value, 0f, 12f);
		}
	}
}
