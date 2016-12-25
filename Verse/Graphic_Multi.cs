using System;
using UnityEngine;

namespace Verse
{
	public class Graphic_Multi : Graphic
	{
		private Material[] mats = new Material[3];

		public string GraphicPath
		{
			get
			{
				return this.path;
			}
		}

		public override Material MatSingle
		{
			get
			{
				return this.mats[2];
			}
		}

		public override Material MatFront
		{
			get
			{
				return this.mats[2];
			}
		}

		public override Material MatSide
		{
			get
			{
				return this.mats[1];
			}
		}

		public override Material MatBack
		{
			get
			{
				return this.mats[0];
			}
		}

		public override bool ShouldDrawRotated
		{
			get
			{
				return this.MatSide == this.MatBack;
			}
		}

		public override void Init(GraphicRequest req)
		{
			this.data = req.graphicData;
			this.path = req.path;
			this.color = req.color;
			this.colorTwo = req.colorTwo;
			this.drawSize = req.drawSize;
			Texture2D[] array = new Texture2D[3];
			array[0] = ContentFinder<Texture2D>.Get(req.path + "_back", false);
			if (array[0] == null)
			{
				Log.Error("Failed to find any texture while constructing " + this.ToString());
				return;
			}
			array[1] = ContentFinder<Texture2D>.Get(req.path + "_side", false);
			if (array[1] == null)
			{
				array[1] = array[0];
			}
			array[2] = ContentFinder<Texture2D>.Get(req.path + "_front", false);
			if (array[2] == null)
			{
				array[2] = array[0];
			}
			Texture2D[] array2 = new Texture2D[3];
			if (req.shader.SupportsMaskTex())
			{
				array2[0] = ContentFinder<Texture2D>.Get(req.path + "_backm", false);
				if (array2[0] != null)
				{
					array2[1] = ContentFinder<Texture2D>.Get(req.path + "_sidem", false);
					if (array2[1] == null)
					{
						array2[1] = array2[0];
					}
					array2[2] = ContentFinder<Texture2D>.Get(req.path + "_frontm", false);
					if (array2[2] == null)
					{
						array2[2] = array2[0];
					}
				}
			}
			for (int i = 0; i < 3; i++)
			{
				MaterialRequest req2 = default(MaterialRequest);
				req2.mainTex = array[i];
				req2.shader = req.shader;
				req2.color = this.color;
				req2.colorTwo = this.colorTwo;
				req2.maskTex = array2[i];
				this.mats[i] = MaterialPool.MatFrom(req2);
			}
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return GraphicDatabase.Get<Graphic_Multi>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Multi(initPath=",
				this.path,
				", color=",
				this.color,
				", colorTwo=",
				this.colorTwo,
				")"
			});
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine<string>(seed, this.path);
			seed = Gen.HashCombineStruct<Color>(seed, this.color);
			return Gen.HashCombineStruct<Color>(seed, this.colorTwo);
		}
	}
}
