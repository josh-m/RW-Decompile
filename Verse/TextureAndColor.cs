using System;
using UnityEngine;

namespace Verse
{
	public struct TextureAndColor
	{
		private Texture2D texture;

		private Color color;

		public bool HasValue
		{
			get
			{
				return this.texture != null;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return this.texture;
			}
		}

		public Color Color
		{
			get
			{
				return this.color;
			}
		}

		public static TextureAndColor None
		{
			get
			{
				return new TextureAndColor(null, Color.white);
			}
		}

		public TextureAndColor(Texture2D texture, Color color)
		{
			this.texture = texture;
			this.color = color;
		}

		public static implicit operator TextureAndColor(Texture2D texture)
		{
			return new TextureAndColor(texture, Color.white);
		}
	}
}
