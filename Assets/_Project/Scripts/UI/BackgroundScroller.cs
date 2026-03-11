using UnityEngine;
using UnityEngine.UI;

public class MusicReactiveScroller : MonoBehaviour
{
    public RawImage rawImage;
    public AudioSource audioSource;
    public float baseSpeed = 0.01f;
    public float boost = 0.08f;
    public float smooth = 5f;

    float[] samples = new float[64];
    float currentSpeed;
    Rect uv;

    void Awake()
    {
        if (rawImage == null) rawImage = GetComponent<RawImage>();
        uv = rawImage.uvRect;
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        float energy = 0f;
        for (int i = 0; i < 8; i++) energy += samples[i];

        float targetSpeed = baseSpeed + energy * boost;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * smooth);

        uv.y += currentSpeed * Time.deltaTime;
        rawImage.uvRect = uv;
    }
}