using System;
using UnityEngine;

namespace Verse
{
	public class CurveMark
	{
		public float x;

		public string message;

		public Color color;

		public CurveMark(float x, string message, Color color)
		{
			this.x = x;
			this.message = message;
			this.color = color;
		}
	}
}
