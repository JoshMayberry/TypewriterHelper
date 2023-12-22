using UnityEngine;

using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;

public class DebugManager : MonoBehaviour {

    [EntryFilter(Variant = EntryVariant.Event)] public EntryReference testConversation;

    public void OnTestConversation() {
        DialogManager.instance.TrySequence(testConversation);
    }
}
