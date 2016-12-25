using System;
using Verse;

namespace RimWorld
{
	public interface ICommunicable
	{
		string GetCallLabel();

		string GetInfoText();

		void TryOpenComms(Pawn negotiator);
	}
}
