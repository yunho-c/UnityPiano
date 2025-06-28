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

    private GameControls gameControls;
    private InputActionMap inputActionMap;

    // private Dictionary<KeyCode, PianoKeyController> keyMappings = new Dictionary<KeyCode, PianoKeyController>();
    private Dictionary<string, PianoKeyController> keyMappings = new Dictionary<string, PianoKeyController>();
    // private Dictionary<KeyCode, MPTKEvent> activeNotes = new Dictionary<KeyCode, MPTKEvent>();
    private Dictionary<string, MPTKEvent> activeNotes = new Dictionary<string, MPTKEvent>();

    void Start()
    {
        gameControls = new GameControls();
        inputActionMap = gameControls.PianoKeys.Get();

        // Find all PianoKeyController components
        PianoKeyController[] pianoKeys = FindObjectsByType<PianoKeyController>(FindObjectsSortMode.None);
        
        // Register keyboard <-> piano key mapping
        foreach (PianoKeyController pianoKey in pianoKeys)
        {
            // if (pianoKey.mappedPCKey != KeyCode.None && !keyMappings.ContainsKey(pianoKey.mappedPCKey))
            // {
            //     keyMappings.Add(pianoKey.mappedPCKey, pianoKey);
            // }
            
            // if (pianoKey.actionName != "" && !keyMappings.ContainsKey(pianoKey.mappedPCKey))
            if (!string.IsNullOrEmpty(pianoKey.actionName) && !keyMappings.ContainsKey(pianoKey.actionName))
                {
                InputAction action = gameControls.FindAction(pianoKey.actionName); // nullable
                if (action != null)
                {
                    keyMappings.Add(action.name, pianoKey);
                }
            }
            // else if (keyMappings.ContainsKey(pianoKey.mappedPCKey))
            else if (keyMappings.ContainsKey(pianoKey.actionName))
            {
                // Debug.LogWarning($"Duplicate PC Key mapping for {pianoKey.mappedPCKey} on {pianoKey.gameObject.name}.");
                Debug.LogWarning($"Duplicate action name mapping for {pianoKey.actionName} on {pianoKey.gameObject.name}.");
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

        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        // foreach (InputAction action in gameControls.PianoKeys.actions)
        foreach (InputAction action in inputActionMap.actions)
            {
            action.started += OnPianoKeyPressed;
            action.canceled += OnPianoKeyReleased;
        }
        // gameControls.PianoKeys.actions.Enable();
        gameControls.PianoKeys.Enable();
        Debug.Log("Piano input actions registered and PianoKeys action map enabled.");
    }

    private void OnPianoKeyPressed(InputAction.CallbackContext context)
    {
        // Debug.Log($"ran");
        if (keyMappings.TryGetValue(context.action.name, out PianoKeyController pianoKey))
        {
            if (!activeNotes.ContainsKey(context.action.name))
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
                activeNotes.Add(context.action.name, noteOnEvent); // store activation state for tracking
                Debug.Log($"PC Key: {context.action.name} -> Piano Key: {pianoKey.actionName} (Note {pianoKey.midiNoteNumber}) ON");
            }
        }
    }

    private void OnPianoKeyReleased(InputAction.CallbackContext context)
    {
        if (keyMappings.TryGetValue(context.action.name, out PianoKeyController pianoKey))
        {
            // if (activeNotes.ContainsKey(context.action.name)) // ERROR
            if (activeNotes.TryGetValue(context.action.name, out MPTKEvent midiNote))
            {
                pianoKey.PressKey();
                midiStreamPlayer.MPTK_StopEvent(midiNote);
                activeNotes.Remove(context.action.name);
            }
        }
    }

    void OnDisable()
    {
        // Disable action map and unregister event callbacks
        if (gameControls != null)
        {
            gameControls.PianoKeys.Disable();
            // foreach (InputAction action in gameControls.PianoKeys.actions)
            foreach (InputAction action in inputActionMap.actions)
            {
                action.started -= OnPianoKeyPressed;
                action.canceled -= OnPianoKeyReleased;
            }
        }

        // Stop any lingering notes
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
