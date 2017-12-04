using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class DropPodIncoming : Skyfaller, IActiveDropPod, IThingHolder
	{
		public ActiveDropPodInfo Contents
		{
			get
			{
				return ((ActiveDropPod)this.innerContainer[0]).Contents;
			}
			set
			{
				((ActiveDropPod)this.innerContainer[0]).Contents = value;
			}
		}

		protected override void Impact()
		{
			for (int i = 0; i < 6; i++)
			{
				Vector3 loc = base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f);
				MoteMaker.ThrowDustPuff(loc, base.Map, 1.2f);
			}
			MoteMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
			base.Impact();
		}
	}
}
