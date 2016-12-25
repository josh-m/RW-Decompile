using System;

namespace RimWorld
{
	public interface IOpenable
	{
		bool CanOpen
		{
			get;
		}

		void Open();
	}
}
