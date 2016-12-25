using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class FadedMaterialPool
	{
		private struct FadedMatRequest : IEquatable<FadedMaterialPool.FadedMatRequest>
		{
			private Material mat;

			private int alphaIndex;

			public FadedMatRequest(Material mat, int alphaIndex)
			{
				this.mat = mat;
				this.alphaIndex = alphaIndex;
			}

			public override bool Equals(object obj)
			{
				return obj != null && obj is FadedMaterialPool.FadedMatRequest && this.Equals((FadedMaterialPool.FadedMatRequest)obj);
			}

			public bool Equals(FadedMaterialPool.FadedMatRequest other)
			{
				return this.mat == other.mat && this.alphaIndex == other.alphaIndex;
			}

			public override int GetHashCode()
			{
				return Gen.HashCombineInt(this.mat.GetHashCode(), this.alphaIndex);
			}

			public static bool operator ==(FadedMaterialPool.FadedMatRequest lhs, FadedMaterialPool.FadedMatRequest rhs)
			{
				return lhs.Equals(rhs);
			}

			public static bool operator !=(FadedMaterialPool.FadedMatRequest lhs, FadedMaterialPool.FadedMatRequest rhs)
			{
				return !(lhs == rhs);
			}
		}

		private class FadedMatRequestComparer : IEqualityComparer<FadedMaterialPool.FadedMatRequest>
		{
			public static readonly FadedMaterialPool.FadedMatRequestComparer Instance = new FadedMaterialPool.FadedMatRequestComparer();

			public bool Equals(FadedMaterialPool.FadedMatRequest x, FadedMaterialPool.FadedMatRequest y)
			{
				return x.Equals(y);
			}

			public int GetHashCode(FadedMaterialPool.FadedMatRequest obj)
			{
				return obj.GetHashCode();
			}
		}

		private const int NumFadeSteps = 30;

		private static Dictionary<FadedMaterialPool.FadedMatRequest, Material> cachedMats = new Dictionary<FadedMaterialPool.FadedMatRequest, Material>(FadedMaterialPool.FadedMatRequestComparer.Instance);

		public static int TotalMaterialCount
		{
			get
			{
				return FadedMaterialPool.cachedMats.Count;
			}
		}

		public static int TotalMaterialBytes
		{
			get
			{
				int num = 0;
				foreach (KeyValuePair<FadedMaterialPool.FadedMatRequest, Material> current in FadedMaterialPool.cachedMats)
				{
					num += Profiler.GetRuntimeMemorySize(current.Value);
				}
				return num;
			}
		}

		public static Material FadedVersionOf(Material sourceMat, float alpha)
		{
			int num = FadedMaterialPool.IndexFromAlpha(alpha);
			if (num == 0)
			{
				return BaseContent.ClearMat;
			}
			if (num == 29)
			{
				return sourceMat;
			}
			FadedMaterialPool.FadedMatRequest key = new FadedMaterialPool.FadedMatRequest(sourceMat, num);
			Material material;
			if (!FadedMaterialPool.cachedMats.TryGetValue(key, out material))
			{
				material = new Material(sourceMat);
				material.color = new Color(1f, 1f, 1f, (float)FadedMaterialPool.IndexFromAlpha(alpha) / 30f);
				FadedMaterialPool.cachedMats.Add(key, material);
			}
			return material;
		}

		private static int IndexFromAlpha(float alpha)
		{
			int num = Mathf.FloorToInt(alpha * 30f);
			if (num == 30)
			{
				num = 29;
			}
			return num;
		}
	}
}
