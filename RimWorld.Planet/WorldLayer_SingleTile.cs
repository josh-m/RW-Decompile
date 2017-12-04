using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldLayer_SingleTile : WorldLayer
	{
		private int lastDrawnTile = -1;

		private List<Vector3> verts = new List<Vector3>();

		protected abstract int Tile
		{
			get;
		}

		protected abstract Material Material
		{
			get;
		}

		public override bool ShouldRegenerate
		{
			get
			{
				return base.ShouldRegenerate || this.Tile != this.lastDrawnTile;
			}
		}

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			int tile = this.Tile;
			if (tile >= 0)
			{
				LayerSubMesh subMesh = base.GetSubMesh(this.Material);
				Find.WorldGrid.GetTileVertices(tile, this.verts);
				int count = subMesh.verts.Count;
				int i = 0;
				int count2 = this.verts.Count;
				while (i < count2)
				{
					subMesh.verts.Add(this.verts[i] + this.verts[i].normalized * 0.012f);
					subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(count2, i) + Vector2.one) / 2f);
					if (i < count2 - 2)
					{
						subMesh.tris.Add(count + i + 2);
						subMesh.tris.Add(count + i + 1);
						subMesh.tris.Add(count);
					}
					i++;
				}
				base.FinalizeMesh(MeshParts.All);
			}
			this.lastDrawnTile = tile;
		}
	}
}
