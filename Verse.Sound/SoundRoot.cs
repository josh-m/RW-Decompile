using System;

namespace Verse.Sound
{
	public class SoundRoot
	{
		public AudioSourcePool sourcePool;

		public SampleOneShotManager oneShotManager;

		public SustainerManager sustainerManager;

		public SoundRoot()
		{
			this.sourcePool = new AudioSourcePool();
			this.sustainerManager = new SustainerManager();
			this.oneShotManager = new SampleOneShotManager();
		}

		public void Update()
		{
			this.sustainerManager.SustainerManagerUpdate();
			this.oneShotManager.SampleOneShotManagerUpdate();
		}
	}
}
