using System;
using UnityEngine;

namespace Verse.Noise
{
	public class RidgedMultifractal : ModuleBase
	{
		private double frequency = 1.0;

		private double lacunarity = 2.0;

		private QualityMode quality = QualityMode.Medium;

		private int octaveCount = 6;

		private int seed;

		private double[] weights = new double[30];

		public double Frequency
		{
			get
			{
				return this.frequency;
			}
			set
			{
				this.frequency = value;
			}
		}

		public double Lacunarity
		{
			get
			{
				return this.lacunarity;
			}
			set
			{
				this.lacunarity = value;
				this.UpdateWeights();
			}
		}

		public QualityMode Quality
		{
			get
			{
				return this.quality;
			}
			set
			{
				this.quality = value;
			}
		}

		public int OctaveCount
		{
			get
			{
				return this.octaveCount;
			}
			set
			{
				this.octaveCount = Mathf.Clamp(value, 1, 30);
			}
		}

		public int Seed
		{
			get
			{
				return this.seed;
			}
			set
			{
				this.seed = value;
			}
		}

		public RidgedMultifractal() : base(0)
		{
			this.UpdateWeights();
		}

		public RidgedMultifractal(double frequency, double lacunarity, int octaves, int seed, QualityMode quality) : base(0)
		{
			this.Frequency = frequency;
			this.Lacunarity = lacunarity;
			this.OctaveCount = octaves;
			this.Seed = seed;
			this.Quality = quality;
		}

		private void UpdateWeights()
		{
			double num = 1.0;
			for (int i = 0; i < 30; i++)
			{
				this.weights[i] = Math.Pow(num, -1.0);
				num *= this.lacunarity;
			}
		}

		public override double GetValue(double x, double y, double z)
		{
			x *= this.frequency;
			y *= this.frequency;
			z *= this.frequency;
			double num = 0.0;
			double num2 = 1.0;
			double num3 = 1.0;
			double num4 = 2.0;
			for (int i = 0; i < this.octaveCount; i++)
			{
				double x2 = Utils.MakeInt32Range(x);
				double y2 = Utils.MakeInt32Range(y);
				double z2 = Utils.MakeInt32Range(z);
				long num5 = (long)(this.seed + i & 2147483647);
				double num6 = Utils.GradientCoherentNoise3D(x2, y2, z2, num5, this.quality);
				num6 = Math.Abs(num6);
				num6 = num3 - num6;
				num6 *= num6;
				num6 *= num2;
				num2 = num6 * num4;
				if (num2 > 1.0)
				{
					num2 = 1.0;
				}
				if (num2 < 0.0)
				{
					num2 = 0.0;
				}
				num += num6 * this.weights[i];
				x *= this.lacunarity;
				y *= this.lacunarity;
				z *= this.lacunarity;
			}
			return num * 1.25 - 1.0;
		}
	}
}
