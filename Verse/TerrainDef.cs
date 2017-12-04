using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public class TerrainDef : BuildableDef
	{
		public enum TerrainEdgeType : byte
		{
			Hard,
			Fade,
			FadeRough,
			Water
		}

		[NoTranslate]
		public string texturePath;

		public TerrainDef.TerrainEdgeType edgeType;

		[NoTranslate]
		public string waterDepthShader;

		public List<ShaderParameter> waterDepthShaderParameters;

		public int renderPrecedence;

		public List<TerrainAffordance> affordances = new List<TerrainAffordance>();

		public bool layerable;

		[NoTranslate]
		public string scatterType;

		public bool takeFootprints;

		public bool takeSplashes;

		public bool avoidWander;

		public bool changeable = true;

		public TerrainDef smoothedTerrain;

		public bool holdSnow = true;

		public bool extinguishesFire;

		public Color color = Color.white;

		public TerrainDef driesTo;

		[NoTranslate]
		public List<string> tags;

		public TerrainDef burnedDef;

		public ThingDef terrainFilthDef;

		public bool acceptTerrainSourceFilth;

		public bool acceptFilth = true;

		[Unsaved]
		public Material waterDepthMaterial;

		public override Color IconDrawColor
		{
			get
			{
				return this.color;
			}
		}

		public bool Removable
		{
			get
			{
				return this.layerable;
			}
		}

		public bool IsCarpet
		{
			get
			{
				return this.researchPrerequisites != null && this.researchPrerequisites.Contains(ResearchProjectDefOf.CarpetMaking);
			}
		}

		public override void PostLoad()
		{
			this.placingDraggableDimensions = 2;
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Shader shader = null;
				switch (this.edgeType)
				{
				case TerrainDef.TerrainEdgeType.Hard:
					shader = ShaderDatabase.TerrainHard;
					break;
				case TerrainDef.TerrainEdgeType.Fade:
					shader = ShaderDatabase.TerrainFade;
					break;
				case TerrainDef.TerrainEdgeType.FadeRough:
					shader = ShaderDatabase.TerrainFadeRough;
					break;
				case TerrainDef.TerrainEdgeType.Water:
					shader = ShaderDatabase.TerrainWater;
					break;
				}
				this.graphic = GraphicDatabase.Get<Graphic_Terrain>(this.texturePath, shader, Vector2.one, this.color, 2000 + this.renderPrecedence);
				if (shader == ShaderDatabase.TerrainFadeRough || shader == ShaderDatabase.TerrainWater)
				{
					this.graphic.MatSingle.SetTexture("_AlphaAddTex", TexGame.AlphaAddTex);
				}
				if (!this.waterDepthShader.NullOrEmpty())
				{
					this.waterDepthMaterial = new Material(ShaderDatabase.LoadShader(this.waterDepthShader));
					this.waterDepthMaterial.renderQueue = 2000 + this.renderPrecedence;
					this.waterDepthMaterial.SetTexture("_AlphaAddTex", TexGame.AlphaAddTex);
					if (this.waterDepthShaderParameters != null)
					{
						for (int i = 0; i < this.waterDepthShaderParameters.Count; i++)
						{
							this.waterDepthMaterial.SetFloat(this.waterDepthShaderParameters[i].name, this.waterDepthShaderParameters[i].value);
						}
					}
				}
			});
			base.PostLoad();
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string err in base.ConfigErrors())
			{
				yield return err;
			}
			if (this.texturePath.NullOrEmpty())
			{
				yield return "missing texturePath";
			}
			if (this.fertility < 0f)
			{
				yield return "Terrain Def " + this + " has no fertility value set.";
			}
			if (this.renderPrecedence > 400)
			{
				yield return "Render order " + this.renderPrecedence + " is out of range (must be < 400)";
			}
			if (this.terrainFilthDef != null && this.acceptTerrainSourceFilth)
			{
				yield return this.defName + " makes terrain filth and also accepts it.";
			}
			if (this.Flammable() && this.burnedDef == null)
			{
				yield return "flammable but burnedDef is null";
			}
			if (this.burnedDef != null && this.burnedDef.Flammable())
			{
				yield return "burnedDef is flammable";
			}
		}

		public static TerrainDef Named(string defName)
		{
			return DefDatabase<TerrainDef>.GetNamed(defName, true);
		}

		public bool HasTag(string tag)
		{
			return this.tags != null && this.tags.Contains(tag);
		}
	}
}
