using System;
using Verse.Sound;

namespace Verse.AI
{
	public static class Toils_Effects
	{
		public static Toil MakeSound(string soundDefName)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				SoundDef.Named(soundDefName).PlayOneShot(actor.Position);
			};
			return toil;
		}
	}
}
