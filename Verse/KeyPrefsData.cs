using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public class KeyPrefsData
	{
		public Dictionary<KeyBindingDef, KeyBindingData> keyPrefs = new Dictionary<KeyBindingDef, KeyBindingData>();

		public void ResetToDefaults()
		{
			this.keyPrefs.Clear();
			this.AddMissingDefaultBindings();
		}

		public void AddMissingDefaultBindings()
		{
			foreach (KeyBindingDef current in DefDatabase<KeyBindingDef>.AllDefs)
			{
				if (!this.keyPrefs.ContainsKey(current))
				{
					this.keyPrefs.Add(current, new KeyBindingData(current.defaultKeyCodeA, current.defaultKeyCodeB));
				}
			}
		}

		public bool SetBinding(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot, KeyCode keyCode)
		{
			KeyBindingData keyBindingData;
			if (this.keyPrefs.TryGetValue(keyDef, out keyBindingData))
			{
				if (slot != KeyPrefs.BindingSlot.A)
				{
					if (slot != KeyPrefs.BindingSlot.B)
					{
						Log.Error("Tried to set a key binding for \"" + keyDef.LabelCap + "\" on a nonexistent slot: " + slot.ToString());
						return false;
					}
					keyBindingData.keyBindingB = keyCode;
				}
				else
				{
					keyBindingData.keyBindingA = keyCode;
				}
				return true;
			}
			Log.Error("Key not found in keyprefs: \"" + keyDef.LabelCap + "\"");
			return false;
		}

		public KeyCode GetBoundKeyCode(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			KeyBindingData keyBindingData;
			if (!this.keyPrefs.TryGetValue(keyDef, out keyBindingData))
			{
				Log.Error("Key not found in keyprefs: \"" + keyDef.LabelCap + "\"");
				return KeyCode.None;
			}
			if (slot == KeyPrefs.BindingSlot.A)
			{
				return keyBindingData.keyBindingA;
			}
			if (slot != KeyPrefs.BindingSlot.B)
			{
				throw new InvalidOperationException();
			}
			return keyBindingData.keyBindingB;
		}

		[DebuggerHidden]
		private IEnumerable<KeyBindingDef> ConflictingBindings(KeyBindingDef keyDef, KeyCode code)
		{
			foreach (KeyBindingDef def in DefDatabase<KeyBindingDef>.AllDefs)
			{
				KeyBindingData prefData;
				if (def != keyDef && ((def.category == keyDef.category && def.category.selfConflicting) || keyDef.category.checkForConflicts.Contains(def.category) || (keyDef.extraConflictTags != null && def.extraConflictTags != null && keyDef.extraConflictTags.Any((string tag) => def.extraConflictTags.Contains(tag)))) && this.keyPrefs.TryGetValue(def, out prefData) && (prefData.keyBindingA == code || prefData.keyBindingB == code))
				{
					yield return def;
				}
			}
		}

		public void EraseConflictingBindingsForKeyCode(KeyBindingDef keyDef, KeyCode keyCode, Action<KeyBindingDef> callBackOnErase = null)
		{
			foreach (KeyBindingDef current in this.ConflictingBindings(keyDef, keyCode))
			{
				KeyBindingData keyBindingData = this.keyPrefs[current];
				if (keyBindingData.keyBindingA == keyCode)
				{
					keyBindingData.keyBindingA = KeyCode.None;
				}
				if (keyBindingData.keyBindingB == keyCode)
				{
					keyBindingData.keyBindingB = KeyCode.None;
				}
				if (callBackOnErase != null)
				{
					callBackOnErase(current);
				}
			}
		}

		public void CheckConflictsFor(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			KeyCode boundKeyCode = this.GetBoundKeyCode(keyDef, slot);
			if (boundKeyCode != KeyCode.None)
			{
				this.EraseConflictingBindingsForKeyCode(keyDef, boundKeyCode, null);
				this.SetBinding(keyDef, slot, boundKeyCode);
			}
		}

		public KeyPrefsData Clone()
		{
			KeyPrefsData keyPrefsData = new KeyPrefsData();
			foreach (KeyValuePair<KeyBindingDef, KeyBindingData> current in this.keyPrefs)
			{
				keyPrefsData.keyPrefs[current.Key] = new KeyBindingData(current.Value.keyBindingA, current.Value.keyBindingB);
			}
			return keyPrefsData;
		}

		public void ErrorCheck()
		{
			foreach (KeyBindingDef current in DefDatabase<KeyBindingDef>.AllDefs)
			{
				this.ErrorCheckOn(current, KeyPrefs.BindingSlot.A);
				this.ErrorCheckOn(current, KeyPrefs.BindingSlot.B);
			}
		}

		private void ErrorCheckOn(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			KeyCode boundKeyCode = this.GetBoundKeyCode(keyDef, slot);
			if (boundKeyCode != KeyCode.None)
			{
				foreach (KeyBindingDef current in this.ConflictingBindings(keyDef, boundKeyCode))
				{
					bool flag = boundKeyCode != keyDef.GetDefaultKeyCode(slot);
					Log.Error(string.Concat(new object[]
					{
						"Key binding conflict: ",
						current,
						" and ",
						keyDef,
						" are both bound to ",
						boundKeyCode,
						".",
						(!flag) ? string.Empty : " Fixed automatically."
					}));
					if (flag)
					{
						if (slot == KeyPrefs.BindingSlot.A)
						{
							this.keyPrefs[keyDef].keyBindingA = keyDef.defaultKeyCodeA;
						}
						else
						{
							this.keyPrefs[keyDef].keyBindingB = keyDef.defaultKeyCodeB;
						}
						KeyPrefs.Save();
					}
				}
			}
		}
	}
}
