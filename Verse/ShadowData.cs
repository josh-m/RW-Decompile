using System;
using UnityEngine;

namespace Verse
{
	public class ShadowData
	{
		public Vector3 volume = Vector3.one;

		public Vector3 offset = Vector3.zero;

		public float BaseX
		{
			get
			{
				return this.volume.x;
			}
		}

		public float BaseY
		{
			get
			{
				return this.volume.y;
			}
		}

		public float BaseZ
		{
			get
			{
				return this.volume.z;
			}
		}
	}
}
