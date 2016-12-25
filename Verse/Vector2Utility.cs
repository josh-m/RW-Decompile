using System;
using UnityEngine;

namespace Verse
{
	public static class Vector2Utility
	{
		public static Vector2 Rotated(this Vector2 v)
		{
			return new Vector2(v.y, v.x);
		}

		public static Vector2 RotatedBy(this Vector2 v, float angle)
		{
			Vector3 point = new Vector3(v.x, 0f, v.y);
			point = Quaternion.AngleAxis(angle, Vector3.up) * point;
			return new Vector2(point.x, point.z);
		}
	}
}
