using System;
using UnityEngine;

namespace Verse
{
	public class Graphic_Random : Graphic_Collection
	{
		public override Material MatSingle
		{
			get
			{
				return this.subGraphics[Rand.Range(0, this.subGraphics.Length)].MatSingle;
			}
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			if (newColorTwo != Color.white)
			{
				Log.ErrorOnce("Cannot use Graphic_Random.GetColoredVersion with a non-white colorTwo.", 9910251);
			}
			return GraphicDatabase.Get<Graphic_Random>(this.path, newShader, this.drawSize, newColor, Color.white, this.data);
		}

		public override Material MatAt(Rot4 rot, Thing thing = null)
		{
			if (thing == null)
			{
				return this.MatSingle;
			}
			return this.MatSingleFor(thing);
		}

		public override Material MatSingleFor(Thing thing)
		{
			if (thing == null)
			{
				return this.MatSingle;
			}
			return this.SubGraphicFor(thing).MatSingle;
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
		{
			Graphic graphic;
			if (thing != null)
			{
				graphic = this.SubGraphicFor(thing);
			}
			else
			{
				graphic = this.subGraphics[0];
			}
			graphic.DrawWorker(loc, rot, thingDef, thing);
		}

		public Graphic SubGraphicFor(Thing thing)
		{
			return this.subGraphics[thing.thingIDNumber % this.subGraphics.Length];
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Random(path=",
				this.path,
				", count=",
				this.subGraphics.Length,
				")"
			});
		}
	}
}
