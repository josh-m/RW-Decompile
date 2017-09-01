using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Graphic_Appearances : Graphic
	{
		protected Graphic[] subGraphics;

		public override Material MatSingle
		{
			get
			{
				return this.subGraphics[(int)StuffAppearanceDefOf.Smooth.index].MatSingle;
			}
		}

		public override void Init(GraphicRequest req)
		{
			this.data = req.graphicData;
			this.path = req.path;
			this.color = req.color;
			this.drawSize = req.drawSize;
			List<StuffAppearanceDef> allDefsListForReading = DefDatabase<StuffAppearanceDef>.AllDefsListForReading;
			this.subGraphics = new Graphic[allDefsListForReading.Count];
			for (int i = 0; i < this.subGraphics.Length; i++)
			{
				StuffAppearanceDef stuffAppearance = allDefsListForReading[i];
				string text = req.path;
				if (!stuffAppearance.pathPrefix.NullOrEmpty())
				{
					text = stuffAppearance.pathPrefix + "/" + text.Split(new char[]
					{
						'/'
					}).Last<string>();
				}
				Texture2D texture2D = (from x in ContentFinder<Texture2D>.GetAllInFolder(text)
				where x.name.EndsWith(stuffAppearance.defName)
				select x).FirstOrDefault<Texture2D>();
				if (texture2D != null)
				{
					this.subGraphics[i] = GraphicDatabase.Get<Graphic_Single>(text + "/" + texture2D.name, req.shader, this.drawSize, this.color);
				}
			}
			for (int j = 0; j < this.subGraphics.Length; j++)
			{
				if (this.subGraphics[j] == null)
				{
					this.subGraphics[j] = this.subGraphics[(int)StuffAppearanceDefOf.Smooth.index];
				}
			}
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			if (newColorTwo != Color.white)
			{
				Log.ErrorOnce("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.", 9910251);
			}
			return GraphicDatabase.Get<Graphic_Appearances>(this.path, newShader, this.drawSize, newColor, Color.white, this.data);
		}

		public override Material MatSingleFor(Thing thing)
		{
			StuffAppearanceDef stuffAppearanceDef = StuffAppearanceDefOf.Smooth;
			if (thing != null && thing.Stuff != null && thing.Stuff.stuffProps.appearance != null)
			{
				stuffAppearanceDef = thing.Stuff.stuffProps.appearance;
			}
			Graphic graphic = this.subGraphics[(int)stuffAppearanceDef.index];
			return graphic.MatSingleFor(thing);
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
		{
			StuffAppearanceDef stuffAppearanceDef = StuffAppearanceDefOf.Smooth;
			if (thing != null && thing.Stuff != null)
			{
				stuffAppearanceDef = thing.Stuff.stuffProps.appearance;
			}
			Graphic graphic = this.subGraphics[(int)stuffAppearanceDef.index];
			graphic.DrawWorker(loc, rot, thingDef, thing);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Appearance(path=",
				this.path,
				", color=",
				this.color,
				", colorTwo=unsupported)"
			});
		}
	}
}
