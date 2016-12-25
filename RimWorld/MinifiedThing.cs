using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MinifiedThing : ThingWithComps
	{
		private const float MaxMinifiedGraphicSize = 1.1f;

		private const float CrateToGraphicScale = 1.16f;

		protected Thing innerThing;

		private Graphic cachedGraphic;

		private Graphic crateFrontGraphic;

		public Thing InnerThing
		{
			get
			{
				return this.innerThing;
			}
			set
			{
				this.innerThing = value;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				if (this.cachedGraphic == null)
				{
					this.cachedGraphic = this.innerThing.Graphic.ExtractInnerGraphicFor(this.innerThing);
					if ((float)this.innerThing.def.size.x > 1.1f || (float)this.innerThing.def.size.z > 1.1f)
					{
						Vector2 minifiedDrawSize = this.GetMinifiedDrawSize(this.innerThing.def.size.ToVector2(), 1.1f);
						Vector2 newDrawSize = new Vector2(minifiedDrawSize.x / (float)this.innerThing.def.size.x * this.cachedGraphic.drawSize.x, minifiedDrawSize.y / (float)this.innerThing.def.size.z * this.cachedGraphic.drawSize.y);
						this.cachedGraphic = this.cachedGraphic.GetCopy(newDrawSize);
					}
				}
				return this.cachedGraphic;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				return this.innerThing.LabelNoCount;
			}
		}

		public override Thing SplitOff(int count)
		{
			MinifiedThing minifiedThing = (MinifiedThing)base.SplitOff(count);
			if (minifiedThing != this)
			{
				minifiedThing.innerThing = ThingMaker.MakeThing(this.innerThing.def, this.innerThing.Stuff);
				ThingWithComps thingWithComps = this.innerThing as ThingWithComps;
				if (thingWithComps != null)
				{
					for (int i = 0; i < thingWithComps.AllComps.Count; i++)
					{
						thingWithComps.AllComps[i].PostSplitOff(minifiedThing.innerThing);
					}
				}
			}
			return minifiedThing;
		}

		public override bool CanStackWith(Thing other)
		{
			MinifiedThing minifiedThing = other as MinifiedThing;
			return minifiedThing != null && base.CanStackWith(other) && this.innerThing.CanStackWith(minifiedThing.innerThing);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.LookDeep<Thing>(ref this.innerThing, "innerThing", new object[0]);
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(this);
			if (blueprint_Install != null)
			{
				GenDraw.DrawLineBetween(this.TrueCenter(), blueprint_Install.TrueCenter());
			}
		}

		public override void DrawAt(Vector3 drawLoc)
		{
			if (this.crateFrontGraphic == null)
			{
				this.crateFrontGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Item/Minified/CrateFront", ShaderDatabase.Cutout, this.GetMinifiedDrawSize(this.innerThing.def.size.ToVector2(), 1.1f) * 1.16f, Color.white);
			}
			this.crateFrontGraphic.DrawFromDef(drawLoc + Altitudes.AltIncVect * 0.1f, Rot4.North, null);
			if (this.Graphic is Graphic_Single)
			{
				this.Graphic.Draw(drawLoc, Rot4.North, this);
			}
			else
			{
				this.Graphic.Draw(drawLoc, Rot4.South, this);
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			bool spawned = base.Spawned;
			base.Destroy(mode);
			InstallBlueprintUtility.CancelBlueprintsFor(this);
			if (mode == DestroyMode.Deconstruct)
			{
				SoundDef.Named("BuildingDeconstructed").PlayOneShot(base.Position);
				if (spawned)
				{
					GenLeaving.DoLeavingsFor(this.innerThing, mode, this.OccupiedRect());
				}
			}
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PreTraded(action, playerNegotiator, trader);
			InstallBlueprintUtility.CancelBlueprintsFor(this);
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo c in base.GetGizmos())
			{
				yield return c;
			}
			yield return InstallationDesignatorDatabase.DesignatorFor(this.def);
		}

		public override string GetInspectString()
		{
			string str = "NotInstalled".Translate();
			str += "\n";
			return str + this.InnerThing.GetInspectString();
		}

		private Vector2 GetMinifiedDrawSize(Vector2 drawSize, float maxSideLength)
		{
			float num = maxSideLength / Mathf.Max(drawSize.x, drawSize.y);
			if (num >= 1f)
			{
				return drawSize;
			}
			return drawSize * num;
		}
	}
}
