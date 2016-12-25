using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class DamageGraphicData
	{
		public bool enabled = true;

		public Rect rectN;

		public Rect rectE;

		public Rect rectS;

		public Rect rectW;

		public Rect rect;

		public List<string> scratches;

		public string cornerTL;

		public string cornerTR;

		public string cornerBL;

		public string cornerBR;

		public string edgeLeft;

		public string edgeRight;

		public string edgeTop;

		public string edgeBot;

		[Unsaved]
		public List<Material> scratchMats;

		[Unsaved]
		public Material cornerTLMat;

		[Unsaved]
		public Material cornerTRMat;

		[Unsaved]
		public Material cornerBLMat;

		[Unsaved]
		public Material cornerBRMat;

		[Unsaved]
		public Material edgeLeftMat;

		[Unsaved]
		public Material edgeRightMat;

		[Unsaved]
		public Material edgeTopMat;

		[Unsaved]
		public Material edgeBotMat;

		public void ResolveReferencesSpecial()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (this.scratches != null)
				{
					this.scratchMats = new List<Material>();
					for (int i = 0; i < this.scratches.Count; i++)
					{
						this.scratchMats[i] = MaterialPool.MatFrom(this.scratches[i], ShaderDatabase.Transparent);
					}
				}
				if (this.cornerTL != null)
				{
					this.cornerTLMat = MaterialPool.MatFrom(this.cornerTL, ShaderDatabase.Transparent);
				}
				if (this.cornerTR != null)
				{
					this.cornerTRMat = MaterialPool.MatFrom(this.cornerTR, ShaderDatabase.Transparent);
				}
				if (this.cornerBL != null)
				{
					this.cornerBLMat = MaterialPool.MatFrom(this.cornerBL, ShaderDatabase.Transparent);
				}
				if (this.cornerBR != null)
				{
					this.cornerBRMat = MaterialPool.MatFrom(this.cornerBR, ShaderDatabase.Transparent);
				}
				if (this.edgeTop != null)
				{
					this.edgeTopMat = MaterialPool.MatFrom(this.edgeTop, ShaderDatabase.Transparent);
				}
				if (this.edgeBot != null)
				{
					this.edgeBotMat = MaterialPool.MatFrom(this.edgeBot, ShaderDatabase.Transparent);
				}
				if (this.edgeLeft != null)
				{
					this.edgeLeftMat = MaterialPool.MatFrom(this.edgeLeft, ShaderDatabase.Transparent);
				}
				if (this.edgeRight != null)
				{
					this.edgeRightMat = MaterialPool.MatFrom(this.edgeRight, ShaderDatabase.Transparent);
				}
			});
		}
	}
}
