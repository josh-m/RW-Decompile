using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Graphic_Flicker : Graphic_Collection
	{
		private const int BaseTicksPerFrameChange = 15;

		private const int ExtraTicksPerFrameChange = 10;

		private const float MaxOffset = 0.05f;

		public override Material MatSingle
		{
			get
			{
				return this.subGraphics[Rand.Range(0, this.subGraphics.Length)].MatSingle;
			}
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
		{
			if (thingDef == null)
			{
				Log.ErrorOnce("Fire DrawWorker with null thingDef: " + loc, 3427324);
				return;
			}
			if (this.subGraphics == null)
			{
				Log.ErrorOnce("Graphic_Flicker has no subgraphics " + thingDef, 358773632);
				return;
			}
			int num = Find.TickManager.TicksGame;
			int num2 = 0;
			int num3 = 0;
			float num4 = 1f;
			CompFireOverlay compFireOverlay = null;
			if (thing != null)
			{
				compFireOverlay = thing.TryGetComp<CompFireOverlay>();
				num += Mathf.Abs(thing.thingIDNumber ^ 8453458);
				num2 = num / 15;
				num3 = Mathf.Abs(num2 ^ thing.thingIDNumber * 391) % this.subGraphics.Length;
				Fire fire = thing as Fire;
				if (fire != null)
				{
					num4 = fire.fireSize;
				}
				else if (compFireOverlay != null)
				{
					num4 = compFireOverlay.Props.fireSize;
				}
			}
			if (num3 < 0 || num3 >= this.subGraphics.Length)
			{
				Log.ErrorOnce("Fire drawing out of range: " + num3, 7453435);
				num3 = 0;
			}
			Graphic graphic = this.subGraphics[num3];
			float num5 = Mathf.Min(num4 / 1.2f, 1.2f);
			Vector3 a = GenRadial.RadialPattern[num2 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius;
			a *= 0.05f;
			Vector3 vector = loc + a * num4;
			if (compFireOverlay != null)
			{
				vector += compFireOverlay.Props.offset;
			}
			Vector3 s = new Vector3(num5, 1f, num5);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(vector, Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Flicker(subGraphic[0]=",
				this.subGraphics[0].ToString(),
				", count=",
				this.subGraphics.Length,
				")"
			});
		}
	}
}
