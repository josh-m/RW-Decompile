using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_SelectedTile : WorldLayer_SingleTile
	{
		protected override int Tile
		{
			get
			{
				return Find.WorldSelector.selectedTile;
			}
		}

		protected override Material Material
		{
			get
			{
				return WorldMaterials.SelectedTile;
			}
		}
	}
}
