using System;
using UnityEngine;

namespace RimWorld.Planet
{
	public class Caravan_Tweener
	{
		private const float SpringTightness = 0.09f;

		private Caravan caravan;

		private Vector3 springPos = Vector3.zero;

		private Vector3 lastTickSpringPos;

		public Vector3 TweenedPos
		{
			get
			{
				return this.springPos;
			}
		}

		public Vector3 LastTickTweenedVelocity
		{
			get
			{
				return this.TweenedPos - this.lastTickSpringPos;
			}
		}

		public Vector3 TweenedPosRoot
		{
			get
			{
				return CaravanTweenerUtility.PatherTweenedPosRoot(this.caravan) + CaravanTweenerUtility.CaravanCollisionPosOffsetFor(this.caravan);
			}
		}

		public Caravan_Tweener(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void TweenerTick()
		{
			this.lastTickSpringPos = this.springPos;
			Vector3 a = this.TweenedPosRoot - this.springPos;
			this.springPos += a * 0.09f;
		}

		public void ResetToPosition()
		{
			this.springPos = this.TweenedPosRoot;
		}
	}
}
