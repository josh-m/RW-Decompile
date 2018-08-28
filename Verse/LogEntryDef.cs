using System;
using UnityEngine;

namespace Verse
{
	public class LogEntryDef : Def
	{
		[NoTranslate]
		public string iconMiss;

		[NoTranslate]
		public string iconDamaged;

		[NoTranslate]
		public string iconDamagedFromInstigator;

		[Unsaved]
		public Texture2D iconMissTex;

		[Unsaved]
		public Texture2D iconDamagedTex;

		[Unsaved]
		public Texture2D iconDamagedFromInstigatorTex;

		public override void PostLoad()
		{
			base.PostLoad();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!this.iconMiss.NullOrEmpty())
				{
					this.iconMissTex = ContentFinder<Texture2D>.Get(this.iconMiss, true);
				}
				if (!this.iconDamaged.NullOrEmpty())
				{
					this.iconDamagedTex = ContentFinder<Texture2D>.Get(this.iconDamaged, true);
				}
				if (!this.iconDamagedFromInstigator.NullOrEmpty())
				{
					this.iconDamagedFromInstigatorTex = ContentFinder<Texture2D>.Get(this.iconDamagedFromInstigator, true);
				}
			});
		}
	}
}
