using UnityEngine;

public class FallingNote : MonoBehaviour
{
    private float fallSpeed;
    private float killZoneY;

    public void Initialize(int pitch, float durationInSeconds, float speed, float killY, float laneStartX, float laneWidth, int lowestPitch)
    {
        this.fallSpeed = speed;
        this.killZoneY = killY;

        // 1. Set the lane (X position) based on pitch
        int laneIndex = pitch - lowestPitch;
        float xPos = laneStartX + (laneIndex * laneWidth);
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);

        // 2. Set the length (Y scale) based on duration
        // A common approach is to scale the note based on its duration and fall speed. 
        // A note that lasts 1 second should be `fallSpeed` units long. 
        float length = durationInSeconds * fallSpeed;
        transform.localScale = new Vector3(transform.localScale.x, length, transform.localScale.z);

        // Adjust position so the note's front edge is at the spawn Y
        transform.position += new Vector3(0, length / 2f, 0);
    }

    void Update()
    {
        // Move the note down
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Check if the note is below the kill zone and destroy it
        if (transform.position.y < killZoneY)
        {
            Destroy(gameObject);
        }
    }
}
