using UnityEngine;

using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;

public class DebugManager : MonoBehaviour {

    [EntryFilter(Variant = EntryVariant.Event)] public EntryReference testConversation;

    public void OnStartTestConversation() {
        DialogManager.instance.TryStartSequence(testConversation);
    }

    public void OnStopConversation() {
        DialogManager.instance.StopSequence();
    }
}
