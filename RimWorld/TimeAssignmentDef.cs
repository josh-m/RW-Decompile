using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TimeAssignmentDef : Def
	{
		public Color color;

		public bool allowRest = true;

		public bool allowJoy = true;

		private Texture2D colorTextureInt;

		public Texture2D ColorTexture
		{
			get
			{
				if (this.colorTextureInt == null)
				{
					this.colorTextureInt = SolidColorMaterials.NewSolidColorTexture(this.color);
				}
				return this.colorTextureInt;
			}
		}
	}
}
