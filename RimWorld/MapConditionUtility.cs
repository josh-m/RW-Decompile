using System;
using UnityEngine;

namespace RimWorld
{
	public static class MapConditionUtility
	{
		public static float LerpInOutValue(float timePassed, float timeLeft, float lerpTime, float lerpTarget = 1f)
		{
			float t;
			if (timePassed < lerpTime)
			{
				t = timePassed / lerpTime;
			}
			else if (timeLeft < lerpTime)
			{
				t = timeLeft / lerpTime;
			}
			else
			{
				t = 1f;
			}
			return Mathf.Lerp(0f, lerpTarget, t);
		}
	}
}
