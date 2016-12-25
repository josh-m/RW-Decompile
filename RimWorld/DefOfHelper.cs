using System;
using System.Reflection;
using Verse;

namespace RimWorld
{
	public static class DefOfHelper
	{
		private static bool earlyTry = true;

		public static void RebindAllDefOfs(bool earlyTryMode)
		{
			DefOfHelper.earlyTry = earlyTryMode;
			foreach (Type current in GenTypes.AllTypesWithAttribute<DefOf>())
			{
				DefOfHelper.BindDefsFor(current);
			}
		}

		private static void BindDefsFor(Type type)
		{
			FieldInfo[] fields = type.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo fieldInfo = fields[i];
				Type fieldType = fieldInfo.FieldType;
				if (!typeof(Def).IsAssignableFrom(fieldType))
				{
					Log.Error(fieldType + " is not a Def.");
				}
				else if (fieldType == typeof(SoundDef))
				{
					if (!DefOfHelper.earlyTry)
					{
						fieldInfo.SetValue(null, SoundDef.Named(fieldInfo.Name));
					}
				}
				else
				{
					Def def = GenDefDatabase.GetDef(fieldType, fieldInfo.Name, !DefOfHelper.earlyTry);
					fieldInfo.SetValue(null, def);
				}
			}
		}
	}
}
