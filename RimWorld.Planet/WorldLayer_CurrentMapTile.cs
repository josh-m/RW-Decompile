using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_CurrentMapTile : WorldLayer_SingleTile
	{
		protected override int Tile
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					return -1;
				}
				if (Find.VisibleMap == null)
				{
					return -1;
				}
				return Find.VisibleMap.Tile;
			}
		}

		protected override Material Material
		{
			get
			{
				return WorldMaterials.CurrentMapTile;
			}
		}
	}
}
