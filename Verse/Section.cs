using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Section
	{
		public const int Size = 17;

		public IntVec3 botLeft;

		public Map map;

		public MapMeshFlag dirtyFlags;

		private List<SectionLayer> layers = new List<SectionLayer>();

		private bool foundRect;

		private CellRect calculatedRect;

		public CellRect CellRect
		{
			get
			{
				if (!this.foundRect)
				{
					this.calculatedRect = new CellRect(this.botLeft.x, this.botLeft.z, 17, 17);
					this.calculatedRect.ClipInsideMap(this.map);
					this.foundRect = true;
				}
				return this.calculatedRect;
			}
		}

		public Section(IntVec3 sectCoords, Map map)
		{
			this.botLeft = sectCoords * 17;
			this.map = map;
			foreach (Type current in typeof(SectionLayer).AllLeafSubclasses())
			{
				this.layers.Add((SectionLayer)Activator.CreateInstance(current, new object[]
				{
					this
				}));
			}
		}

		public void DrawSection(bool drawSunShadowsOnly)
		{
			int count = this.layers.Count;
			for (int i = 0; i < count; i++)
			{
				if (!drawSunShadowsOnly || this.layers[i] is SectionLayer_SunShadows)
				{
					this.layers[i].DrawLayer();
				}
			}
			if (!drawSunShadowsOnly && DebugViewSettings.drawSectionEdges)
			{
				GenDraw.DrawLineBetween(this.botLeft.ToVector3(), this.botLeft.ToVector3() + new Vector3(0f, 0f, 17f));
				GenDraw.DrawLineBetween(this.botLeft.ToVector3(), this.botLeft.ToVector3() + new Vector3(17f, 0f, 0f));
			}
		}

		public void RegenerateAllLayers()
		{
			for (int i = 0; i < this.layers.Count; i++)
			{
				if (this.layers[i].Visible)
				{
					this.layers[i].Regenerate();
				}
			}
		}

		public void RegenerateLayers(MapMeshFlag changeType)
		{
			for (int i = 0; i < this.layers.Count; i++)
			{
				SectionLayer sectionLayer = this.layers[i];
				if ((sectionLayer.relevantChangeTypes & changeType) != MapMeshFlag.None)
				{
					sectionLayer.Regenerate();
				}
			}
		}

		public SectionLayer GetLayer(Type type)
		{
			return (from sect in this.layers
			where sect.GetType() == type
			select sect).FirstOrDefault<SectionLayer>();
		}
	}
}
