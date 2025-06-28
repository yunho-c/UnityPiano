using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    // Configs (set in inspector)
    [Header("Song and Audio")]
    [Tooltip("The AudioSource component that will play the song.")]
    public AudioSource audioSource;
    [Tooltip("The name of the MIDI file in the StreamingAssets folder")]
    public string midiFileName;

    [Header("Note Prefab")]
    [Tooltip("The prefab for the visual note blocks.")]
    public GameObject notePrefab;

    [Header("Note Visualization")]
    [Tooltip("The object that serves as the anchor for the note visualization. If null, world space is used.")]
    public Transform visualizationAnchor;

    [Header("Timing and Speed")]
    [Tooltip("How fast the notes fall in units per second.")]
    public float fallSpeed = 10f;

    [Header("Lane Configuration")]
    [Tooltip("The Y-position where notes will spawn.")]
    public float spawnY = 15;
    [Tooltip("The Y-position of the 'hit line' where notes should be played.")]
    public float hitLineY = 0f;
    [Tooltip("The Y-position where notes are destroyed if missed.")]
    public float killZoneY = -5;

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
        // Ensure AudioSource is assigned
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Calculate how far ahead of time notes need to spawn
        spawnTimeAhead = (spawnY - hitLineY) / fallSpeed;

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
        audioSource.Play();
        Debug.Log("Song Started");
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
        Vector3 spawnPos = new Vector3(0, spawnY, 0); // X will be set by the note itself
        GameObject newNoteObject;

        if (visualizationAnchor != null)
        {
            // Instantiate under the anchor, then set local position and rotation.
            newNoteObject = Instantiate(notePrefab, visualizationAnchor);
            newNoteObject.transform.localPosition = spawnPos;
            newNoteObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            // Original behavior: instantiate at world position.
            newNoteObject = Instantiate(notePrefab, spawnPos, Quaternion.identity);
        }

        // Get FallingNote component and initialize it
        FallingNote fallingNote = newNoteObject.GetComponent<FallingNote>();
        if (fallingNote != null)
        {
            fallingNote.Initialize(
                noteInfo.Pitch,
                (float)noteInfo.Duration,
                fallSpeed,
                killZoneY,
                laneStartX,
                laneWidth,
                lowestPitch
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
