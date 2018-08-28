using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public class EditWindow_TweakValues : EditWindow
	{
		private struct TweakInfo
		{
			public FieldInfo field;

			public TweakValue tweakValue;

			public float initial;
		}

		[TweakValue("TweakValue", 0f, 300f)]
		public static float CategoryWidth = 180f;

		[TweakValue("TweakValue", 0f, 300f)]
		public static float TitleWidth = 300f;

		[TweakValue("TweakValue", 0f, 300f)]
		public static float NumberWidth = 140f;

		private Vector2 scrollPosition;

		private static List<EditWindow_TweakValues.TweakInfo> tweakValueFields;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1000f, 600f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public EditWindow_TweakValues()
		{
			this.optionalTitle = "TweakValues";
			if (EditWindow_TweakValues.tweakValueFields == null)
			{
				EditWindow_TweakValues.tweakValueFields = (from field in this.FindAllTweakables()
				select new EditWindow_TweakValues.TweakInfo
				{
					field = field,
					tweakValue = field.TryGetAttribute<TweakValue>(),
					initial = this.GetAsFloat(field)
				} into ti
				orderby string.Format("{0}.{1}", ti.tweakValue.category, ti.field.DeclaringType.Name)
				select ti).ToList<EditWindow_TweakValues.TweakInfo>();
			}
		}

		[DebuggerHidden]
		private IEnumerable<FieldInfo> FindAllTweakables()
		{
			foreach (Type type in GenTypes.AllTypes)
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				for (int i = 0; i < fields.Length; i++)
				{
					FieldInfo field = fields[i];
					TweakValue tv = field.TryGetAttribute<TweakValue>();
					if (tv != null)
					{
						if (!field.IsStatic)
						{
							Log.Error(string.Format("Field {0}.{1} is marked with TweakValue, but isn't static; TweakValue won't work", field.DeclaringType.FullName, field.Name), false);
						}
						else if (field.IsLiteral)
						{
							Log.Error(string.Format("Field {0}.{1} is marked with TweakValue, but is const; TweakValue won't work", field.DeclaringType.FullName, field.Name), false);
						}
						else if (field.IsInitOnly)
						{
							Log.Error(string.Format("Field {0}.{1} is marked with TweakValue, but is readonly; TweakValue won't work", field.DeclaringType.FullName, field.Name), false);
						}
						else
						{
							yield return field;
						}
					}
				}
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Rect rect = inRect.ContractedBy(4f);
			Rect rect2 = rect;
			rect2.xMax -= 33f;
			Rect rect3 = new Rect(0f, 0f, EditWindow_TweakValues.CategoryWidth, Text.CalcHeight("test", 1000f));
			Rect rect4 = new Rect(rect3.xMax, 0f, EditWindow_TweakValues.TitleWidth, rect3.height);
			Rect rect5 = new Rect(rect4.xMax, 0f, EditWindow_TweakValues.NumberWidth, rect3.height);
			Rect rect6 = new Rect(rect5.xMax, 0f, rect2.width - rect5.xMax, rect3.height);
			Widgets.BeginScrollView(rect, ref this.scrollPosition, new Rect(0f, 0f, rect2.width, rect3.height * (float)EditWindow_TweakValues.tweakValueFields.Count), true);
			foreach (EditWindow_TweakValues.TweakInfo current in EditWindow_TweakValues.tweakValueFields)
			{
				Widgets.Label(rect3, current.tweakValue.category);
				Widgets.Label(rect4, string.Format("{0}.{1}", current.field.DeclaringType.Name, current.field.Name));
				float num;
				bool flag;
				if (current.field.FieldType == typeof(float) || current.field.FieldType == typeof(int) || current.field.FieldType == typeof(ushort))
				{
					float asFloat = this.GetAsFloat(current.field);
					num = Widgets.HorizontalSlider(rect6, this.GetAsFloat(current.field), current.tweakValue.min, current.tweakValue.max, false, null, null, null, -1f);
					this.SetFromFloat(current.field, num);
					flag = (asFloat != num);
				}
				else if (current.field.FieldType == typeof(bool))
				{
					bool flag2 = (bool)current.field.GetValue(null);
					bool flag3 = flag2;
					Widgets.Checkbox(rect6.xMin, rect6.yMin, ref flag3, 24f, false, false, null, null);
					current.field.SetValue(null, flag3);
					num = (float)((!flag3) ? 0 : 1);
					flag = (flag2 != flag3);
				}
				else
				{
					Log.ErrorOnce(string.Format("Attempted to tweakvalue unknown field type {0}", current.field.FieldType), 83944645, false);
					flag = false;
					num = current.initial;
				}
				if (num != current.initial)
				{
					GUI.color = Color.red;
					Widgets.Label(rect5, string.Format("{0} -> {1}", current.initial, num));
					GUI.color = Color.white;
					if (Widgets.ButtonInvisible(rect5, false))
					{
						flag = true;
						if (current.field.FieldType == typeof(float) || current.field.FieldType == typeof(int) || current.field.FieldType == typeof(ushort))
						{
							this.SetFromFloat(current.field, current.initial);
						}
						else if (current.field.FieldType == typeof(bool))
						{
							current.field.SetValue(null, current.initial != 0f);
						}
						else
						{
							Log.ErrorOnce(string.Format("Attempted to tweakvalue unknown field type {0}", current.field.FieldType), 83944646, false);
						}
					}
				}
				else
				{
					Widgets.Label(rect5, string.Format("{0}", current.initial));
				}
				if (flag)
				{
					MethodInfo method = current.field.DeclaringType.GetMethod(current.field.Name + "_Changed", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null)
					{
						method.Invoke(null, null);
					}
				}
				rect3.y += rect3.height;
				rect4.y += rect3.height;
				rect5.y += rect3.height;
				rect6.y += rect3.height;
			}
			Widgets.EndScrollView();
		}

		private float GetAsFloat(FieldInfo field)
		{
			if (field.FieldType == typeof(float))
			{
				return (float)field.GetValue(null);
			}
			if (field.FieldType == typeof(bool))
			{
				return (float)((!(bool)field.GetValue(null)) ? 0 : 1);
			}
			if (field.FieldType == typeof(int))
			{
				return (float)((int)field.GetValue(null));
			}
			if (field.FieldType == typeof(ushort))
			{
				return (float)((ushort)field.GetValue(null));
			}
			Log.ErrorOnce(string.Format("Attempted to return unknown field type {0} as a float", field.FieldType), 83944644, false);
			return 0f;
		}

		private void SetFromFloat(FieldInfo field, float input)
		{
			if (field.FieldType == typeof(float))
			{
				field.SetValue(null, input);
			}
			else if (field.FieldType == typeof(bool))
			{
				field.SetValue(null, input != 0f);
			}
			else if (field.FieldType == typeof(int))
			{
				field.SetValue(field, (int)input);
			}
			else if (field.FieldType == typeof(ushort))
			{
				field.SetValue(field, (ushort)input);
			}
			else
			{
				Log.ErrorOnce(string.Format("Attempted to set unknown field type {0} from a float", field.FieldType), 83944645, false);
			}
		}
	}
}
