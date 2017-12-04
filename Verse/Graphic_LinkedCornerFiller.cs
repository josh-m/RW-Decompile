using System;
using UnityEngine;

namespace Verse
{
	public class Graphic_LinkedCornerFiller : Graphic_Linked
	{
		private const float ShiftUp = 0.09f;

		private const float CoverSize = 0.5f;

		private static readonly float CoverSizeCornerCorner;

		private static readonly float DistCenterCorner;

		private static readonly float CoverOffsetDist;

		private static readonly Vector2[] CornerFillUVs;

		public override LinkDrawerType LinkerType
		{
			get
			{
				return LinkDrawerType.CornerFiller;
			}
		}

		public Graphic_LinkedCornerFiller(Graphic subGraphic) : base(subGraphic)
		{
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return new Graphic_LinkedCornerFiller(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
			{
				data = this.data
			};
		}

		public override void Print(SectionLayer layer, Thing thing)
		{
			base.Print(layer, thing);
			IntVec3 position = thing.Position;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = thing.Position + GenAdj.DiagonalDirectionsAround[i];
				if (this.ShouldLinkWith(c, thing))
				{
					if (i != 0 || (this.ShouldLinkWith(position + IntVec3.West, thing) && this.ShouldLinkWith(position + IntVec3.South, thing)))
					{
						if (i != 1 || (this.ShouldLinkWith(position + IntVec3.West, thing) && this.ShouldLinkWith(position + IntVec3.North, thing)))
						{
							if (i != 2 || (this.ShouldLinkWith(position + IntVec3.East, thing) && this.ShouldLinkWith(position + IntVec3.North, thing)))
							{
								if (i != 3 || (this.ShouldLinkWith(position + IntVec3.East, thing) && this.ShouldLinkWith(position + IntVec3.South, thing)))
								{
									Vector3 vector = thing.DrawPos + GenAdj.DiagonalDirectionsAround[i].ToVector3().normalized * Graphic_LinkedCornerFiller.CoverOffsetDist + Altitudes.AltIncVect + new Vector3(0f, 0f, 0.09f);
									Vector2 vector2 = new Vector2(0.5f, 0.5f);
									if (!c.InBounds(thing.Map))
									{
										if (c.x == -1)
										{
											vector.x -= 1f;
											vector2.x *= 5f;
										}
										if (c.z == -1)
										{
											vector.z -= 1f;
											vector2.y *= 5f;
										}
										if (c.x == thing.Map.Size.x)
										{
											vector.x += 1f;
											vector2.x *= 5f;
										}
										if (c.z == thing.Map.Size.z)
										{
											vector.z += 1f;
											vector2.y *= 5f;
										}
									}
									Vector3 center = vector;
									Vector2 size = vector2;
									Material mat = base.LinkedDrawMatFrom(thing, thing.Position);
									float rot = 0f;
									Vector2[] cornerFillUVs = Graphic_LinkedCornerFiller.CornerFillUVs;
									Printer_Plane.PrintPlane(layer, center, size, mat, rot, false, cornerFillUVs, null, 0.01f);
								}
							}
						}
					}
				}
			}
		}

		static Graphic_LinkedCornerFiller()
		{
			// Note: this type is marked as 'beforefieldinit'.
			Vector2 vector = new Vector2(0.5f, 0.5f);
			Graphic_LinkedCornerFiller.CoverSizeCornerCorner = vector.magnitude;
			Vector2 vector2 = new Vector2(0.5f, 0.5f);
			Graphic_LinkedCornerFiller.DistCenterCorner = vector2.magnitude;
			Graphic_LinkedCornerFiller.CoverOffsetDist = Graphic_LinkedCornerFiller.DistCenterCorner - Graphic_LinkedCornerFiller.CoverSizeCornerCorner * 0.5f;
			Graphic_LinkedCornerFiller.CornerFillUVs = new Vector2[]
			{
				new Vector2(0.5f, 0.6f),
				new Vector2(0.5f, 0.6f),
				new Vector2(0.5f, 0.6f),
				new Vector2(0.5f, 0.6f)
			};
		}
	}
}
