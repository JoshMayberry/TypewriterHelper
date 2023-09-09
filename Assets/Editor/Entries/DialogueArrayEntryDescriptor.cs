using UnityEditor;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Editor.Descriptors;
using Aarthificial.Typewriter.Editor.Lists;
using Aarthificial.Typewriter.References;
using UnityEngine.UIElements;

namespace Aarthificial.Typewriter.Entries {
    [CustomEntryDescriptor(typeof(DialogueArrayEntry))]
    public class DialogueArrayEntryDescriptor : RuleEntryDescriptor {
        public override string Name => "Dialogue Array";

        public override string Color => "#2196f3";
    }
}

//namespace Aarthificial.Typewriter.Editor.Lists {
//    public class DialogueEditableListView : EditableListView {
//        public DialogueEditableListView() : base() {
//            // Customize the list item display
//            List.makeItem = MakeDialogueItem;
//            List.bindItem = BindDialogueItem;
//        }

//        private VisualElement MakeDialogueItem() {
//            var item = new VisualElement();

//            // Create fields for speaker and text
//            var speakerButton = new Button(() => {
//                // Open a selection dialog for the speaker here
//            }) {
//                text = "Select Speaker"
//            };
//            var textField = new TextField("Text");

//            item.Add(speakerButton);
//            item.Add(textField);

//            return item;
//        }

//        private void BindDialogueItem(VisualElement element, int index) {
//            if (_property == null || index < 0 || index >= _property.arraySize) return;

//            var itemProperty = _property.GetArrayElementAtIndex(index);
//            var speakerProperty = itemProperty.FindPropertyRelative("_speaker");
//            var textProperty = itemProperty.FindPropertyRelative("Text");

//            var speakerField = element.Q<ObjectField>("Speaker");
//            var textField = element.Q<TextField>("Text");

//            speakerField.Bind(speakerProperty);
//            textField.Bind(textProperty);
//        }

//        // If you want to further customize the addition and removal of items, you can override the HandleAdd and HandleRemove methods here.
//    }
//}


//[CustomEditor(typeof(DialogueArrayEntry))]
//public class DialogueArrayEntryEditor : Editor {

//    public override void OnInspectorGUI() {
//        serializedObject.Update();

//        // Draw default properties
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("Speed"));
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("IsChoice"));

//        serializedObject.ApplyModifiedProperties();
//    }
//}
