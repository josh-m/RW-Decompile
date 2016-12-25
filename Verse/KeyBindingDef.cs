using System;
using UnityEngine;

namespace Verse
{
	public class KeyBindingDef : Def
	{
		public KeyBindingCategoryDef category;

		public KeyCode defaultKeyCodeA;

		public KeyCode defaultKeyCodeB;

		public bool devModeOnly;

		public KeyCode MainKey
		{
			get
			{
				KeyBindingData keyBindingData;
				if (KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out keyBindingData))
				{
					if (keyBindingData.keyBindingA != KeyCode.None)
					{
						return keyBindingData.keyBindingA;
					}
					if (keyBindingData.keyBindingB != KeyCode.None)
					{
						return keyBindingData.keyBindingB;
					}
				}
				return KeyCode.None;
			}
		}

		public string MainKeyLabel
		{
			get
			{
				return this.MainKey.ToStringReadable();
			}
		}

		public bool KeyDownEvent
		{
			get
			{
				KeyBindingData keyBindingData;
				return Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None && KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out keyBindingData) && (Event.current.keyCode == keyBindingData.keyBindingA || Event.current.keyCode == keyBindingData.keyBindingB);
			}
		}

		public bool JustPressed
		{
			get
			{
				KeyBindingData keyBindingData;
				return KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out keyBindingData) && (Input.GetKeyDown(keyBindingData.keyBindingA) || Input.GetKeyDown(keyBindingData.keyBindingB));
			}
		}

		public bool IsDown
		{
			get
			{
				KeyBindingData keyBindingData;
				return KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out keyBindingData) && (Input.GetKey(keyBindingData.keyBindingA) || Input.GetKey(keyBindingData.keyBindingB));
			}
		}

		public static KeyBindingDef Named(string name)
		{
			return DefDatabase<KeyBindingDef>.GetNamedSilentFail(name);
		}
	}
}
