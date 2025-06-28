using UnityEngine;

public class PianoKeyController : MonoBehaviour
{
    // params
    public int midiNoteNumber;
    // public KeyCode mappedPCKey;
    public string actionName;

    // states
    private Vector3 originalPosition;
    private bool isPressed = false;

    void Start() 
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {

    }

    public void PressKey() 
    {
        if (isPressed) { return;  }
        
        isPressed = true;
        
        // transform.localPosition = originalPosition - new Vector3(0, pressDepth, 0);
        // Optional: Add a subtle visual cue like a slight color change
    }

    public void ReleaseKey()
    {
        if (!isPressed) { return; }
        
        isPressed = false;
        
        // transform.localPosition = originalPosition;
    }
}
