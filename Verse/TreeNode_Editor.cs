using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public class TreeNode_Editor : TreeNode
	{
		public object obj;

		public FieldInfo owningField;

		public int owningIndex = -1;

		private MethodInfo editWidgetsMethod;

		public EditTreeNodeType nodeType;

		private int indexToDelete = -1;

		public object ParentObj
		{
			get
			{
				return ((TreeNode_Editor)this.parentNode).obj;
			}
		}

		public Type ObjectType
		{
			get
			{
				if (this.owningField != null)
				{
					return this.owningField.FieldType;
				}
				if (this.IsListItem)
				{
					return this.ListRootObject.GetType().GetGenericArguments()[0];
				}
				if (this.obj != null)
				{
					return this.obj.GetType();
				}
				throw new InvalidOperationException();
			}
		}

		public object Value
		{
			get
			{
				if (this.owningField != null)
				{
					return this.owningField.GetValue(this.ParentObj);
				}
				if (this.IsListItem)
				{
					return this.ListRootObject.GetType().GetProperty("Item").GetValue(this.ListRootObject, new object[]
					{
						this.owningIndex
					});
				}
				throw new InvalidOperationException();
			}
			set
			{
				if (this.owningField != null)
				{
					this.owningField.SetValue(this.ParentObj, value);
				}
				if (this.IsListItem)
				{
					this.ListRootObject.GetType().GetProperty("Item").SetValue(this.ListRootObject, value, new object[]
					{
						this.owningIndex
					});
				}
			}
		}

		public bool IsListItem
		{
			get
			{
				return this.owningIndex >= 0;
			}
		}

		private object ListRootObject
		{
			get
			{
				return this.ParentObj;
			}
		}

		public override bool Openable
		{
			get
			{
				return this.obj != null && this.nodeType != EditTreeNodeType.TerminalValue && (this.nodeType != EditTreeNodeType.ListRoot || (int)this.obj.GetType().GetProperty("Count").GetValue(this.obj, null) != 0);
			}
		}

		public bool HasContentLines
		{
			get
			{
				return this.nodeType != EditTreeNodeType.TerminalValue;
			}
		}

		public bool HasNewButton
		{
			get
			{
				return (this.nodeType == EditTreeNodeType.ComplexObject && this.obj == null) || (this.owningField != null && this.owningField.FieldType.HasAttribute<EditorReplaceableAttribute>());
			}
		}

		public bool HasDeleteButton
		{
			get
			{
				return this.IsListItem || (this.owningField != null && this.owningField.FieldType.HasAttribute<EditorNullableAttribute>());
			}
		}

		public string ExtraInfoText
		{
			get
			{
				if (this.obj == null)
				{
					return "null";
				}
				if (this.obj.GetType().HasAttribute<EditorShowClassNameAttribute>())
				{
					return this.obj.GetType().Name;
				}
				if (this.obj.GetType().IsGenericType && this.obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
				{
					int num = (int)this.obj.GetType().GetProperty("Count").GetValue(this.obj, null);
					return string.Concat(new string[]
					{
						"(",
						num.ToString(),
						" ",
						(num != 1) ? "elements" : "element",
						")"
					});
				}
				return string.Empty;
			}
		}

		public string LabelText
		{
			get
			{
				if (this.owningField != null)
				{
					return this.owningField.Name;
				}
				if (this.IsListItem)
				{
					return this.owningIndex.ToString();
				}
				return this.ObjectType.Name;
			}
		}

		private TreeNode_Editor()
		{
		}

		public static TreeNode_Editor NewRootNode(object rootObj)
		{
			if (rootObj.GetType().IsValueEditable())
			{
				throw new ArgumentException();
			}
			TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
			treeNode_Editor.owningField = null;
			treeNode_Editor.obj = rootObj;
			treeNode_Editor.nestDepth = 0;
			treeNode_Editor.RebuildChildNodes();
			treeNode_Editor.InitiallyCacheData();
			return treeNode_Editor;
		}

		public static TreeNode_Editor NewChildNodeFromField(TreeNode_Editor parent, FieldInfo fieldInfo)
		{
			TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
			treeNode_Editor.parentNode = parent;
			treeNode_Editor.nestDepth = parent.nestDepth + 1;
			treeNode_Editor.owningField = fieldInfo;
			if (!fieldInfo.FieldType.IsValueEditable())
			{
				treeNode_Editor.obj = fieldInfo.GetValue(parent.obj);
				treeNode_Editor.RebuildChildNodes();
			}
			treeNode_Editor.InitiallyCacheData();
			return treeNode_Editor;
		}

		private static TreeNode_Editor NewChildNodeFromListItem(TreeNode_Editor parent, int listIndex)
		{
			TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
			treeNode_Editor.parentNode = parent;
			treeNode_Editor.nestDepth = parent.nestDepth + 1;
			treeNode_Editor.owningIndex = listIndex;
			object obj = parent.obj;
			Type type = obj.GetType();
			Type type2 = type.GetGenericArguments()[0];
			if (!type2.IsValueEditable())
			{
				object value = type.GetProperty("Item").GetValue(obj, new object[]
				{
					listIndex
				});
				treeNode_Editor.obj = value;
				treeNode_Editor.RebuildChildNodes();
			}
			treeNode_Editor.InitiallyCacheData();
			return treeNode_Editor;
		}

		private void InitiallyCacheData()
		{
			if (this.obj != null && this.obj.GetType().IsGenericType && this.obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
			{
				this.nodeType = EditTreeNodeType.ListRoot;
			}
			else if (this.ObjectType.IsValueEditable())
			{
				this.nodeType = EditTreeNodeType.TerminalValue;
			}
			else
			{
				this.nodeType = EditTreeNodeType.ComplexObject;
			}
			if (this.obj != null)
			{
				this.editWidgetsMethod = this.obj.GetType().GetMethod("DoEditWidgets");
			}
		}

		public void RebuildChildNodes()
		{
			if (this.obj == null)
			{
				return;
			}
			this.children = new List<TreeNode>();
			Type objType = this.obj.GetType();
			if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
			{
				int num = (int)objType.GetProperty("Count").GetValue(this.obj, null);
				for (int i = 0; i < num; i++)
				{
					TreeNode_Editor item = TreeNode_Editor.NewChildNodeFromListItem(this, i);
					this.children.Add(item);
				}
			}
			else
			{
				foreach (FieldInfo current in from f in this.obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				orderby this.InheritanceDistanceBetween(objType, f.DeclaringType) descending
				select f)
				{
					if (current.GetCustomAttributes(typeof(UnsavedAttribute), true).Length <= 0 && current.GetCustomAttributes(typeof(EditorHiddenAttribute), true).Length <= 0)
					{
						TreeNode_Editor item2 = TreeNode_Editor.NewChildNodeFromField(this, current);
						this.children.Add(item2);
					}
				}
			}
		}

		private int InheritanceDistanceBetween(Type childType, Type parentType)
		{
			Type type = childType;
			int num = 0;
			while (type != parentType)
			{
				type = type.BaseType;
				num++;
				if (type == null)
				{
					Log.Error(childType + " is not a subclass of " + parentType);
					return -1;
				}
			}
			return num;
		}

		public void CheckLatentDelete()
		{
			if (this.indexToDelete >= 0)
			{
				this.obj.GetType().GetMethod("RemoveAt").Invoke(this.obj, new object[]
				{
					this.indexToDelete
				});
				this.RebuildChildNodes();
				this.indexToDelete = -1;
			}
		}

		public void Delete()
		{
			if (this.owningField != null)
			{
				this.owningField.SetValue(this.obj, null);
			}
			else
			{
				if (!this.IsListItem)
				{
					throw new InvalidOperationException();
				}
				((TreeNode_Editor)this.parentNode).indexToDelete = this.owningIndex;
			}
		}

		public void DoSpecialPreElements(Listing_TreeDefs listing)
		{
			if (this.obj == null)
			{
				return;
			}
			if (this.editWidgetsMethod != null)
			{
				WidgetRow widgetRow = listing.StartWidgetsRow(this.nestDepth);
				this.editWidgetsMethod.Invoke(this.obj, new object[]
				{
					widgetRow
				});
			}
			Editable editable = this.obj as Editable;
			if (editable != null)
			{
				GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
				foreach (string current in editable.ConfigErrors())
				{
					listing.InfoText(current, this.nestDepth);
				}
				GUI.color = Color.white;
			}
		}

		public override string ToString()
		{
			string text = "EditTreeNode(";
			if (this.ParentObj != null)
			{
				text = text + " owningObj=" + this.ParentObj;
			}
			if (this.owningField != null)
			{
				text = text + " owningField=" + this.owningField;
			}
			if (this.owningIndex >= 0)
			{
				text = text + " owningIndex=" + this.owningIndex;
			}
			return text + ")";
		}
	}
}
