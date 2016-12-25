using System;
using UnityEngine;

namespace Verse.Noise
{
	public class Perlin : ModuleBase
	{
		private double m_frequency = 1.0;

		private double m_lacunarity = 2.0;

		private QualityMode m_quality = QualityMode.Medium;

		private int m_octaveCount = 6;

		private double m_persistence = 0.5;

		private int m_seed;

		public double Frequency
		{
			get
			{
				return this.m_frequency;
			}
			set
			{
				this.m_frequency = value;
			}
		}

		public double Lacunarity
		{
			get
			{
				return this.m_lacunarity;
			}
			set
			{
				this.m_lacunarity = value;
			}
		}

		public QualityMode Quality
		{
			get
			{
				return this.m_quality;
			}
			set
			{
				this.m_quality = value;
			}
		}

		public int OctaveCount
		{
			get
			{
				return this.m_octaveCount;
			}
			set
			{
				this.m_octaveCount = Mathf.Clamp(value, 1, 30);
			}
		}

		public double Persistence
		{
			get
			{
				return this.m_persistence;
			}
			set
			{
				this.m_persistence = value;
			}
		}

		public int Seed
		{
			get
			{
				return this.m_seed;
			}
			set
			{
				this.m_seed = value;
			}
		}

		public Perlin() : base(0)
		{
		}

		public Perlin(double frequency, double lacunarity, double persistence, int octaves, int seed, QualityMode quality) : base(0)
		{
			this.Frequency = frequency;
			this.Lacunarity = lacunarity;
			this.OctaveCount = octaves;
			this.Persistence = persistence;
			this.Seed = seed;
			this.Quality = quality;
		}

		public override double GetValue(double x, double y, double z)
		{
			double num = 0.0;
			double num2 = 1.0;
			x *= this.m_frequency;
			y *= this.m_frequency;
			z *= this.m_frequency;
			for (int i = 0; i < this.m_octaveCount; i++)
			{
				double x2 = Utils.MakeInt32Range(x);
				double y2 = Utils.MakeInt32Range(y);
				double z2 = Utils.MakeInt32Range(z);
				long seed = (long)(this.m_seed + i) & (long)((ulong)-1);
				double num3 = Utils.GradientCoherentNoise3D(x2, y2, z2, seed, this.m_quality);
				num += num3 * num2;
				x *= this.m_lacunarity;
				y *= this.m_lacunarity;
				z *= this.m_lacunarity;
				num2 *= this.m_persistence;
			}
			return num;
		}
	}
}
