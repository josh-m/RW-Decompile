using System;

namespace RimWorld
{
	public interface ISocialThought
	{
		float OpinionOffset();

		int OtherPawnID();
	}
}
