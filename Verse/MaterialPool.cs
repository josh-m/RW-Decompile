using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MaterialPool
	{
		private static Dictionary<MaterialRequest, Material> matDictionary = new Dictionary<MaterialRequest, Material>();

		public static Material MatFrom(string texPath, bool reportFailure)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, reportFailure));
			return MaterialPool.MatFrom(req);
		}

		public static Material MatFrom(string texPath)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, true));
			return MaterialPool.MatFrom(req);
		}

		public static Material MatFrom(Texture2D srcTex)
		{
			MaterialRequest req = new MaterialRequest(srcTex);
			return MaterialPool.MatFrom(req);
		}

		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color)
		{
			MaterialRequest req = new MaterialRequest(srcTex, shader, color);
			return MaterialPool.MatFrom(req);
		}

		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color, int renderQueue)
		{
			return MaterialPool.MatFrom(new MaterialRequest(srcTex, shader, color)
			{
				renderQueue = renderQueue
			});
		}

		public static Material MatFrom(string texPath, Shader shader)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader);
			return MaterialPool.MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader, int renderQueue)
		{
			return MaterialPool.MatFrom(new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader)
			{
				renderQueue = renderQueue
			});
		}

		public static Material MatFrom(string texPath, Shader shader, Color color)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader, color);
			return MaterialPool.MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader, Color color, int renderQueue)
		{
			return MaterialPool.MatFrom(new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader, color)
			{
				renderQueue = renderQueue
			});
		}

		public static Material MatFrom(MaterialRequest req)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get a material from a different thread.");
				return null;
			}
			if (req.mainTex == null)
			{
				Log.Error("MatFrom with null sourceTex.");
				return BaseContent.BadMat;
			}
			if (req.shader == null)
			{
				Log.Warning("Matfrom with null shader.");
				return BaseContent.BadMat;
			}
			if (req.maskTex != null && !req.shader.SupportsMaskTex())
			{
				Log.Error("MaterialRequest has maskTex but shader does not support it. req=" + req.ToString());
				req.maskTex = null;
			}
			Material material;
			if (!MaterialPool.matDictionary.TryGetValue(req, out material))
			{
				material = new Material(req.shader);
				material.name = req.shader.name + "_" + req.mainTex.name;
				material.mainTexture = req.mainTex;
				material.color = req.color;
				if (req.maskTex != null)
				{
					material.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
					material.SetColor(ShaderPropertyIDs.ColorTwo, req.colorTwo);
				}
				if (req.renderQueue != 0)
				{
					material.renderQueue = req.renderQueue;
				}
				MaterialPool.matDictionary.Add(req, material);
				if (!MaterialPool.matDictionary.ContainsKey(req))
				{
					Log.Error("MaterialRequest is not present in the dictionary even though we've just added it there. The equality operators are most likely defined incorrectly.");
				}
				if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
				{
					WindManager.Notify_PlantMaterialCreated(material);
				}
			}
			return material;
		}
	}
}
