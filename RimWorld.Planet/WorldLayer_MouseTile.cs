using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_MouseTile : WorldLayer_SingleTile
	{
		protected override int Tile
		{
			get
			{
				WorldDragBox dragBox = Find.World.UI.selector.dragBox;
				if (dragBox.IsValidAndActive)
				{
					return -1;
				}
				if (Find.WorldTargeter.IsTargeting)
				{
					return -1;
				}
				return GenWorld.MouseTile();
			}
		}

		protected override Material Material
		{
			get
			{
				return WorldMaterials.MouseTile;
			}
		}
	}
}
