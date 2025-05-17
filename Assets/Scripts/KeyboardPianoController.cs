using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System
using System.Collections.Generic;
using MidiPlayerTK;


public class KeyboardPianoController : MonoBehaviour
{
    [Header("MPTK Setup")]
    public MidiStreamPlayer midiStreamPlayer;

    [Header("MIDI Settings")]
    public int defaultVelocity = 100;
    public int midiChannel = 0; // QUE?

    private KeyboardInput keyboardInput;

    // private Dictionary<KeyCode, PianoKeyController> keyMappings = new Dictionary<KeyCode, PianoKeyController>();
    private Dictionary<string, PianoKeyController> keyMappings = new Dictionary<KeyCode, PianoKeyController>();
    // private Dictionary<KeyCode, MPTKEvent> activeNotes = new Dictionary<KeyCode, MPTKEvent>();
    private Dictionary<string, MPTKEvent> activeNotes = new Dictionary<KeyCode, MPTKEvent>();

    void Start()
    {
        // Find and register all PianoKeyController components
        PianoKeyController[] pianoKeys = FindObjectsByType<PianoKeyController>(FindObjectsSortMode.None);
        
        // Register keyboard <-> piano key mappings
        foreach (PianoKeyController pianoKey in pianoKeys)
        {
            if (pianoKey.mappedPCKey != KeyCode.None && ! keyMappings.ContainsKey(pianoKey.mappedPCKey))
            {
                keyMappings.Add(pianoKey.mappedPCKey, pianoKey);
            }
            else if (keyMappings.ContainsKey(pianoKey.mappedPCKey))
            {
                Debug.LogWarning($"Duplicate PC Key mapping for {pianoKey.mappedPCKey} on {pianoKey.gameObject.name}.");
            }
        }

        if (midiStreamPlayer == null)
        {
            Debug.LogError("MidiStreamPlayer not assigned.");
            return;
        }

        // Ensure MPTK is ready
        // if (!midiStreamPlayer.MPTK_IsSynthInitialized)
        // {
        //     midiStreamPlayer.
        // }
    }

    void Update()
    {
        // Iterate through all mapped keys to check their state
        foreach (KeyValuePair<KeyCode, PianoKeyController> entry in keyMappings)
        {
            KeyCode pcKey = entry.Key;
            PianoKeyController pianoKey = entry.Value;

            if (Input.GetKeyDown(pcKey))
            {
                if (!activeNotes.ContainsKey(pcKey)) // prevent re-triggering
                {
                    pianoKey.PressKey();

                    MPTKEvent noteOnEvent = new MPTKEvent()
                    {
                        Command = MPTKCommand.NoteOn,
                        Channel = midiChannel,
                        Duration = -1,
                        Value = pianoKey.midiNoteNumber,
                        Velocity = defaultVelocity,
                    };
                    midiStreamPlayer.MPTK_PlayEvent(noteOnEvent);
                    activeNotes.Add(pcKey, noteOnEvent); // store activation state for tracking
                                                         // Debug.Log($"PC Key: {pcKey} -> Piano Key: {pianoKey.midiNoteNumber} ON")
                }
            }
            else if (Input.GetKeyUp(pcKey))
            {
                if (activeNotes.TryGetValue(pcKey, out MPTKEvent noteToStop))
                {
                    pianoKey.ReleaseKey();
                    midiStreamPlayer.MPTK_StopEvent(noteToStop); // stops specific note event
                    activeNotes.Remove(pcKey);
                    // Debug.Log($"PC Key: {pcKey} -> Piano Key: {pianoKey.midiNoteNumber} OFF")
                }
            }
        }
    }

    void OnDisable()
    {
        // Ensure all notes are stopped if this controller is disabled or destroyed
        if (midiStreamPlayer != null && activeNotes != null)
        {
            foreach (MPTKEvent noteEvent in activeNotes.Values)
            {
                midiStreamPlayer.MPTK_StopEvent(noteEvent);
            }
            activeNotes.Clear();
        }
    }
}
