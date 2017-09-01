using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class FactionBase : Settlement
	{
		private string nameInt;

		public bool namedByPlayer;

		private Material cachedMat;

		public string Name
		{
			get
			{
				return this.nameInt;
			}
			set
			{
				this.nameInt = value;
			}
		}

		public override Texture2D ExpandingIcon
		{
			get
			{
				return base.Faction.def.ExpandingIconTexture;
			}
		}

		public override string Label
		{
			get
			{
				return (this.nameInt == null) ? base.Label : this.nameInt;
			}
		}

		public override Material Material
		{
			get
			{
				if (this.cachedMat == null)
				{
					this.cachedMat = MaterialPool.MatFrom(base.Faction.def.homeIconPath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
				}
				return this.cachedMat;
			}
		}

		public override MapGeneratorDef MapGeneratorDef
		{
			get
			{
				if (base.Faction == Faction.OfPlayer)
				{
					return null;
				}
				return MapGeneratorDefOf.FactionBase;
			}
		}

		public FactionBase()
		{
			this.trader = new FactionBase_TraderTracker(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.nameInt, "nameInt", null, false);
			Scribe_Values.Look<bool>(ref this.namedByPlayer, "namedByPlayer", false, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.FactionBasePostLoadInit(this);
			}
		}

		public override void Tick()
		{
			base.Tick();
			FactionBaseDefeatUtility.CheckDefeated(this);
		}
	}
}
