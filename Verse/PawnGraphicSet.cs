using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class PawnGraphicSet
	{
		public Pawn pawn;

		public Graphic nakedGraphic;

		public Graphic rottingGraphic;

		public Graphic dessicatedGraphic;

		public Graphic packGraphic;

		public DamageFlasher flasher;

		public Graphic headGraphic;

		public Graphic desiccatedHeadGraphic;

		public Graphic skullGraphic;

		public Graphic hairGraphic;

		public List<ApparelGraphicRecord> apparelGraphics = new List<ApparelGraphicRecord>();

		private List<Material> cachedMatsBodyBase = new List<Material>();

		private int cachedMatsBodyBaseHash = -1;

		public static readonly Color RottingColor = new Color(0.34f, 0.32f, 0.3f);

		public bool AllResolved
		{
			get
			{
				return this.nakedGraphic != null;
			}
		}

		public GraphicMeshSet HairMeshSet
		{
			get
			{
				if (this.pawn.story.crownType == CrownType.Average)
				{
					return MeshPool.humanlikeHairSetAverage;
				}
				if (this.pawn.story.crownType == CrownType.Narrow)
				{
					return MeshPool.humanlikeHairSetNarrow;
				}
				Log.Error("Unknown crown type: " + this.pawn.story.crownType);
				return MeshPool.humanlikeHairSetAverage;
			}
		}

		public PawnGraphicSet(Pawn pawn)
		{
			this.pawn = pawn;
			this.flasher = new DamageFlasher(pawn);
		}

		public List<Material> MatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
		{
			int num = facing.AsInt + 1000 * (int)bodyCondition;
			if (num != this.cachedMatsBodyBaseHash)
			{
				this.cachedMatsBodyBase.Clear();
				this.cachedMatsBodyBaseHash = num;
				if (bodyCondition == RotDrawMode.Fresh)
				{
					this.cachedMatsBodyBase.Add(this.nakedGraphic.MatAt(facing, null));
				}
				else if (bodyCondition == RotDrawMode.Rotting || this.dessicatedGraphic == null)
				{
					this.cachedMatsBodyBase.Add(this.rottingGraphic.MatAt(facing, null));
				}
				else if (bodyCondition == RotDrawMode.Dessicated)
				{
					this.cachedMatsBodyBase.Add(this.dessicatedGraphic.MatAt(facing, null));
				}
				for (int i = 0; i < this.apparelGraphics.Count; i++)
				{
					if (this.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayer.Shell && this.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayer.Overhead)
					{
						this.cachedMatsBodyBase.Add(this.apparelGraphics[i].graphic.MatAt(facing, null));
					}
				}
			}
			return this.cachedMatsBodyBase;
		}

		public Material HeadMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
		{
			Material baseMat = null;
			if (bodyCondition == RotDrawMode.Fresh)
			{
				baseMat = this.headGraphic.MatAt(facing, null);
			}
			else if (bodyCondition == RotDrawMode.Rotting)
			{
				baseMat = this.desiccatedHeadGraphic.MatAt(facing, null);
			}
			else if (bodyCondition == RotDrawMode.Dessicated)
			{
				baseMat = this.skullGraphic.MatAt(facing, null);
			}
			return this.flasher.GetDamagedMat(baseMat);
		}

		public Material HairMatAt(Rot4 facing)
		{
			Material baseMat = this.hairGraphic.MatAt(facing, null);
			return this.flasher.GetDamagedMat(baseMat);
		}

		public void ClearCache()
		{
			this.cachedMatsBodyBaseHash = -1;
		}

		public void ResolveAllGraphics()
		{
			this.ClearCache();
			if (this.pawn.RaceProps.Humanlike)
			{
				this.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(this.pawn.story.bodyType, ShaderDatabase.CutoutSkin, this.pawn.story.SkinColor);
				this.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(this.pawn.story.bodyType, ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor);
				this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
				this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, this.pawn.story.SkinColor);
				this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, PawnGraphicSet.RottingColor);
				this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
				this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(this.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, this.pawn.story.hairColor);
				this.ResolveApparelGraphics();
			}
			else
			{
				PawnKindLifeStage curKindLifeStage = this.pawn.ageTracker.CurKindLifeStage;
				if (this.pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
				{
					this.nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
				}
				else
				{
					this.nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
				}
				this.rottingGraphic = this.nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor);
				if (this.pawn.RaceProps.packAnimal)
				{
					this.packGraphic = GraphicDatabase.Get<Graphic_Multi>(this.nakedGraphic.path + "Pack", ShaderDatabase.Cutout, this.nakedGraphic.drawSize, Color.white);
				}
				if (curKindLifeStage.dessicatedBodyGraphicData != null)
				{
					this.dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(this.pawn);
				}
			}
		}

		public void ResolveApparelGraphics()
		{
			this.ClearCache();
			this.apparelGraphics.Clear();
			foreach (Apparel current in this.pawn.apparel.WornApparelInDrawOrder)
			{
				ApparelGraphicRecord item;
				if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, this.pawn.story.bodyType, out item))
				{
					this.apparelGraphics.Add(item);
				}
			}
		}
	}
}
