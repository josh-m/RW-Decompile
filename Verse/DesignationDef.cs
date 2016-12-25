using System;
using UnityEngine;

namespace Verse
{
	public class DesignationDef : Def
	{
		[NoTranslate]
		public string texturePath;

		public TargetType targetType;

		public bool removeIfBuildingDespawned;

		public bool designateCancelable = true;

		[Unsaved]
		public Material iconMat;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.iconMat = MaterialPool.MatFrom(this.texturePath, ShaderDatabase.MetaOverlay);
			});
		}
	}
}
