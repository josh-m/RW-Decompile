using System;
using UnityEngine;

namespace Verse
{
	public struct GraphicRequest : IEquatable<GraphicRequest>
	{
		public Type graphicClass;

		public string path;

		public Shader shader;

		public Vector2 drawSize;

		public Color color;

		public Color colorTwo;

		public GraphicData graphicData;

		public GraphicRequest(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData graphicData)
		{
			this.graphicClass = graphicClass;
			this.path = path;
			this.shader = shader;
			this.drawSize = drawSize;
			this.color = color;
			this.colorTwo = colorTwo;
			this.graphicData = graphicData;
		}

		public override int GetHashCode()
		{
			if (this.path == null)
			{
				this.path = BaseContent.BadTexPath;
			}
			int seed = 0;
			seed = Gen.HashCombine<Type>(seed, this.graphicClass);
			seed = Gen.HashCombine<string>(seed, this.path);
			seed = Gen.HashCombine<Shader>(seed, this.shader);
			seed = Gen.HashCombineStruct<Vector2>(seed, this.drawSize);
			seed = Gen.HashCombineStruct<Color>(seed, this.color);
			seed = Gen.HashCombineStruct<Color>(seed, this.colorTwo);
			return Gen.HashCombine<GraphicData>(seed, this.graphicData);
		}

		public override bool Equals(object obj)
		{
			return obj is GraphicRequest && this.Equals((GraphicRequest)obj);
		}

		public bool Equals(GraphicRequest other)
		{
			return this.graphicClass == other.graphicClass && this.path == other.path && this.shader == other.shader && this.drawSize == other.drawSize && this.color == other.color && this.colorTwo == other.colorTwo && this.graphicData == other.graphicData;
		}

		public static bool operator ==(GraphicRequest lhs, GraphicRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(GraphicRequest lhs, GraphicRequest rhs)
		{
			return !(lhs == rhs);
		}
	}
}
