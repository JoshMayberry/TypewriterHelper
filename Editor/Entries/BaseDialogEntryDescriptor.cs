using UnityEditor;
using UnityEngine;
using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter.Editor.Descriptors;
using Aarthificial.Typewriter.Editor.PropertyDrawers;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;


namespace jmayberry.TypewriterHelper.Editor.Descriptors {
	public class BaseDialogEntryDescriptor : RuleEntryDescriptor {
		public override string Name => "Dialogue";
		public override string Color => "#7cd1e2";

		[CustomPropertyDrawer(typeof(string[]))]
		public class StringArrayDrawer : PropertyDrawer {
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
				return EditorGUI.GetPropertyHeight(property, label, true);
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
				EditorGUI.PropertyField(position, property, label, true);
			}
		}
	}
}

namespace jmayberry.TypewriterHelper.Editor.PropertyDrawers {
	public class BaseDialogEntryPropertyDrawer : BaseEntryPropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			// Expand the TextList array by default
			SerializedProperty textListProperty = property.FindPropertyRelative("TextList");
			if (textListProperty != null) {
				textListProperty.isExpanded = true;
				EditorGUI.PropertyField(position, textListProperty, new GUIContent("Dialog"), true);
			}

			EditorGUI.EndProperty();

			base.OnGUI(position, property, label);
		}
	}
}