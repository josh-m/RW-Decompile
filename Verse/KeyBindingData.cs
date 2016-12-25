using System;
using UnityEngine;

namespace Verse
{
	public class KeyBindingData
	{
		public KeyCode keyBindingA;

		public KeyCode keyBindingB;

		public KeyBindingData()
		{
		}

		public KeyBindingData(KeyCode keyBindingA, KeyCode keyBindingB)
		{
			this.keyBindingA = keyBindingA;
			this.keyBindingB = keyBindingB;
		}

		public override string ToString()
		{
			string str = "[";
			if (this.keyBindingA != KeyCode.None)
			{
				str += this.keyBindingA.ToString();
			}
			if (this.keyBindingB != KeyCode.None)
			{
				str = str + ", " + this.keyBindingB.ToString();
			}
			return str + "]";
		}
	}
}
