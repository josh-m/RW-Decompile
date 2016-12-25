using System;
using System.Collections.Generic;

namespace Verse
{
	public abstract class SectionLayer_Things : SectionLayer
	{
		protected bool requireAddToMapMesh;

		public SectionLayer_Things(Section section) : base(section)
		{
		}

		public override void DrawLayer()
		{
			if (!DebugViewSettings.drawThingsPrinted)
			{
				return;
			}
			base.DrawLayer();
		}

		public override void Regenerate()
		{
			base.ClearSubMeshes(MeshParts.All);
			foreach (IntVec3 current in this.section.CellRect)
			{
				List<Thing> list = base.Map.thingGrid.ThingsListAt(current);
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					Thing thing = list[i];
					if (thing.def.drawerType != DrawerType.None)
					{
						if (thing.def.drawerType != DrawerType.RealtimeOnly || !this.requireAddToMapMesh)
						{
							if (thing.def.hideAtSnowDepth >= 1f || base.Map.snowGrid.GetDepth(thing.Position) <= thing.def.hideAtSnowDepth)
							{
								if (thing.Position.x == current.x && thing.Position.z == current.z)
								{
									this.TakePrintFrom(thing);
								}
							}
						}
					}
				}
			}
			base.FinalizeMesh(MeshParts.All);
		}

		protected abstract void TakePrintFrom(Thing t);
	}
}
