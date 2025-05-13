using UnityEngine;

public class PianoKeyController : MonoBehaviour
{
    public AudioClip keySound;
    private AudioSource audioSource;
    private Vector3 originalPosition;

    void Start() 
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = keySound;
        originalPosition = transform.localPosition;
    }

    public void PressKey() 
    {
        audioSource.Play();
        // transform.localPosition = originalPosition - new Vector3(0, pressDepth, 0);
    }

    public void ReleaseKey()
    {
        // transform.localPosition = originalPosition;
    }

    // private void OnTriggerEnter
}
