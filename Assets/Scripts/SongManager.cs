using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MidiPlayerTK;

public class SongManager : MonoBehaviour
{
  // Configs (set in inspector)
  [Header("Song and Audio")]
  [Tooltip("The MPTK MidiFilePlayer component that will play the song.")]
  public MidiFilePlayer midiFilePlayer;
  [Tooltip("The name of the MIDI file in the StreamingAssets folder")]
  public string midiFileName;

  [Header("Note Prefab")]
  [Tooltip("The prefab for the visual note blocks.")]
  public GameObject notePrefab;

  [Header("Note Visualization")]
  [Tooltip("The object that serves as the anchor for the note visualization. If null, world space is used.")]
  public Transform visualizationAnchor;

  [Header("Timing and Speed")]
  [Tooltip("The total time in seconds for a note to fall from spawn to the hit line.")]
  public float noteFallDuration = 2f;
  [Tooltip("The easing curve for the note's fall. X-axis is normalized time (0-1), Y-axis is normalized distance (0-1).")]
  public AnimationCurve noteFallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
  [Tooltip("How long (in seconds) the note should exist after its end time before being destroyed.")]
  public float noteDestroyOffset = 0.0f;


  [Header("Lane Configuration")]
  [Tooltip("The Y-position where notes will spawn.")]
  public float spawnY = 15;
  [Tooltip("The Z-position for spawning notes.")]
  public float spawnZOffset = 0f;
  [Tooltip("The Y-position of the 'hit line' where notes should be played.")]
  public float hitLineY = 0f;

  [Tooltip("The X-position of the center of the first lane.")]
  public float laneStartX = -4.5f;
  [Tooltip("The distance between the centers of adjacent lanes.")]
  public float laneWidth = 1f;
  [Tooltip("The MIDI pitch number that corresponds to the first lane (e.g., 48 for C3.)")]
  public int lowestPitch = 48;

  // Private variables
  private MidiFile midiFile;
  private List<NoteInfo> notes = new List<NoteInfo>();
  private double songStartTimeDsp;
  private float spawnTimeAhead;

  void Start()
  {
    // Ensure MidiFilePlayer is assigned
    if (midiFilePlayer == null) midiFilePlayer = GetComponent<MidiFilePlayer>();
    if (midiFilePlayer == null)
    {
        Debug.LogError("MidiFilePlayer not assigned or not found on the GameObject.");
        return;
    }

    // Set the MIDI file to play
    midiFilePlayer.MPTK_MidiName = Path.GetFileNameWithoutExtension(midiFileName);

    // Set a default ease-in curve if one isn't set in the inspector.
    // This curve starts slow and accelerates (y = x^2).
    if (noteFallCurve == null || noteFallCurve.keys.Length < 2)
    {
      noteFallCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 2, 0));
    }

    // Notes need to spawn 'noteFallDuration' seconds before they are to be hit.
    spawnTimeAhead = noteFallDuration;

    ReadMidiFile();
    StartSong();
  }

  private void ReadMidiFile()
  {
    string midiFilePath = Path.Combine(Application.streamingAssetsPath, midiFileName);

    if (!File.Exists(midiFilePath))
    {
      Debug.LogError($"MIDI file not found at path: {midiFilePath}");
      return;
    }

    midiFile = MidiFile.Read(midiFilePath);
    var tempoMap = midiFile.GetTempoMap();

    // Get all notes from the MIDI file
    foreach (var note in midiFile.GetNotes())
    {
      // Convert time from MIDI ticks to seconds
      var startTimeInSeconds = note.TimeAs<MetricTimeSpan>(tempoMap).TotalSeconds;
      var durationInSeconds = note.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds;

      notes.Add(new NoteInfo
      {
        Pitch = note.NoteNumber,
        StartTime = startTimeInSeconds,
        Duration = durationInSeconds,
        HasBeenSpawned = false
      });
    }
  }

  private void StartSong()
  {
    songStartTimeDsp = AudioSettings.dspTime;
    midiFilePlayer.MPTK_Play();
    Debug.Log("Song Started with MPTK");
  }

  void Update()
  {
    // Calculate the current time in the song
    double currentTime = AudioSettings.dspTime - songStartTimeDsp;

    // Loop through the notes to check for spawning
    for (int i = 0; i < notes.Count; i++)
    {
      NoteInfo note = notes[i];
      if (!note.HasBeenSpawned && currentTime >= note.StartTime - spawnTimeAhead)
      {
        // Spawn the note
        SpawnNote(note);
        note.HasBeenSpawned = true;
        notes[i] = note;
      }
    }
  }

  private void SpawnNote(NoteInfo noteInfo)
  {
    // Instantiate prefab at correct height
    Vector3 spawnPos = new Vector3(0, spawnY, spawnZOffset); // X will be set by the note itself
    GameObject newNoteObject;

    if (visualizationAnchor != null)
    {
      // instantiate at anchor, then set local position and rotation.
      newNoteObject = Instantiate(notePrefab, visualizationAnchor);
      newNoteObject.transform.localPosition = spawnPos;
      newNoteObject.transform.localRotation = Quaternion.identity;
    }
    else
    {
      // Instantiate at world position
      newNoteObject = Instantiate(notePrefab, spawnPos, Quaternion.identity);
    }

    // Get FallingNote component and initialize it
    FallingNote fallingNote = newNoteObject.GetComponent<FallingNote>();
    if (fallingNote != null)
    {
      // Calculate the precise time the note should be destroyed.
      double destructionTime = songStartTimeDsp + noteInfo.StartTime + noteInfo.Duration + noteDestroyOffset;

      fallingNote.Initialize(
          noteInfo.Pitch,
          (float)noteInfo.Duration,
          noteFallDuration,
          spawnY,
          hitLineY,
          laneStartX,
          laneWidth,
          lowestPitch,
          noteFallCurve,
          destructionTime
      );
    }
  }
}

public struct NoteInfo
{
  public int Pitch;
  public double StartTime;
  public double Duration;
  public bool HasBeenSpawned;
}

