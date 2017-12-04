using System;

namespace RimWorld
{
	public struct Signal
	{
		public string tag;

		public object[] args;

		public Signal(string tag)
		{
			this.tag = tag;
			this.args = null;
		}

		public Signal(string tag, object[] args)
		{
			this.tag = tag;
			this.args = args;
		}
	}
}
