using UnityEngine;
using System.Collections;

public class FallingNote : MonoBehaviour
{
  private float fallDuration;
  private float timeElapsed;
  private float spawnY;
  private float hitLineY;
  private AnimationCurve fallCurve;
  private double destructionTimeDsp;

  public void Initialize(int pitch, float durationInSeconds, float fallDuration, float spawnY, float hitLineY, float laneStartX, float laneWidth, int lowestPitch, AnimationCurve fallCurve, double destructionTimeDsp)
  {
    this.fallDuration = fallDuration;
    this.spawnY = spawnY;
    this.hitLineY = hitLineY;
    this.fallCurve = fallCurve;
    this.destructionTimeDsp = destructionTimeDsp;
    this.timeElapsed = 0f;

    // 1. Set the lane (X position) based on pitch
    int laneIndex = pitch - lowestPitch;
    float xPos = laneStartX + (laneIndex * laneWidth);
    transform.localPosition = new Vector3(xPos, transform.localPosition.y, transform.localPosition.z);

    // 2. Set the length (Y scale) based on duration
    float averageSpeed = (this.spawnY - this.hitLineY) / this.fallDuration;
    float length = durationInSeconds * averageSpeed;
    transform.localScale = new Vector3(transform.localScale.x, length, transform.localScale.z);

    // Adjust position so the note's front (bottom) edge starts at spawnY.
    // Since the pivot is at the center, we move the note up by half its length.
    transform.localPosition += new Vector3(0, length / 2f, 0);

    // Start the self-destruction coroutine
    StartCoroutine(DestroyAfterTime());
  }

  private IEnumerator DestroyAfterTime()
  {
    // Wait until the specified DSP time has been reached.
    // This will continue even if the GameObject is deactivated.
    yield return new WaitUntil(() => AudioSettings.dspTime >= destructionTimeDsp);

    // Now, destroy the GameObject.
    Destroy(gameObject);
  }

  void Update()
  {
    timeElapsed += Time.deltaTime;

    // Calculate progress (0 to 1)
    float progress = Mathf.Clamp01(timeElapsed / fallDuration);

    // Evaluate the curve to get the eased progress
    float easedProgress = fallCurve.Evaluate(progress);

    // The movement should be based on the top edge of the note, not the bottom.
    float noteLength = transform.localScale.y;
    float topEdgeStart = spawnY + noteLength;
    float topEdgeEnd = hitLineY;

    // Interpolate the position of the note's top edge.
    float topEdgeY = Mathf.Lerp(topEdgeStart, topEdgeEnd, easedProgress);

    // The object's position is its center.
    float newCenterY = topEdgeY - (noteLength / 2f);
    transform.localPosition = new Vector3(transform.localPosition.x, newCenterY, transform.localPosition.z);
  }
}
