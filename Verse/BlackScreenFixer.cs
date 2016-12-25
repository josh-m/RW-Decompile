using System;
using UnityEngine;

namespace Verse
{
	internal class BlackScreenFixer : MonoBehaviour
	{
		private void Start()
		{
			Screen.SetResolution(UI.screenWidth, UI.screenHeight, Screen.fullScreen);
		}
	}
}
