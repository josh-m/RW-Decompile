using System;
using UnityEngine;

namespace Verse
{
	public struct MaterialRequest : IEquatable<MaterialRequest>
	{
		public Shader shader;

		public Texture2D mainTex;

		public Color color;

		public Color colorTwo;

		public Texture2D maskTex;

		public string BaseTexPath
		{
			set
			{
				this.mainTex = ContentFinder<Texture2D>.Get(value, true);
			}
		}

		public MaterialRequest(Texture2D tex)
		{
			this.shader = ShaderDatabase.Cutout;
			this.mainTex = tex;
			this.color = Color.white;
			this.colorTwo = Color.white;
			this.maskTex = null;
		}

		public MaterialRequest(Texture2D tex, Shader shader)
		{
			this.shader = shader;
			this.mainTex = tex;
			this.color = Color.white;
			this.colorTwo = Color.white;
			this.maskTex = null;
		}

		public MaterialRequest(Texture2D tex, Shader shader, Color color)
		{
			this.shader = shader;
			this.mainTex = tex;
			this.color = color;
			this.colorTwo = Color.white;
			this.maskTex = null;
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine<Shader>(seed, this.shader);
			seed = Gen.HashCombineStruct<Color>(seed, this.color);
			seed = Gen.HashCombineStruct<Color>(seed, this.colorTwo);
			seed = Gen.HashCombine<Texture2D>(seed, this.mainTex);
			return Gen.HashCombine<Texture2D>(seed, this.maskTex);
		}

		public override bool Equals(object obj)
		{
			return obj is MaterialRequest && this.Equals((MaterialRequest)obj);
		}

		public bool Equals(MaterialRequest other)
		{
			return other.shader == this.shader && other.mainTex == this.mainTex && other.color == this.color && other.colorTwo == this.colorTwo && other.maskTex == this.maskTex;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"MaterialRequest(",
				this.shader.name,
				", ",
				this.mainTex.name,
				", ",
				this.color.ToString(),
				", ",
				this.colorTwo.ToString(),
				", ",
				this.maskTex.ToString(),
				")"
			});
		}

		public static bool operator ==(MaterialRequest lhs, MaterialRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(MaterialRequest lhs, MaterialRequest rhs)
		{
			return !(lhs == rhs);
		}
	}
}
