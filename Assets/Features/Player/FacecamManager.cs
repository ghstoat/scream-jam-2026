using UnityEngine;

public class FacecamManager : MonoBehaviour
{
    public static FacecamManager Instance { get; private set; }

    [SerializeField] Animator mouthAnimator;
    private bool talking;
    private const string TALKING = "TALKING";
    [SerializeField] AudioSource playerAudioSource;
    [Range(0.001f, 0.1f)]
    [SerializeField] float threshold = 0.015f;

    [SerializeField] float sensitivity = 100f;

    private float[] samples = new float[256];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }


    }

    private void Update()
    {
        bool speaking = false;

        if (playerAudioSource.isPlaying)
        {
            playerAudioSource.GetOutputData(samples, 0);

            float sum = 0f;

            foreach (float sample in samples)
                sum += sample * sample;

            float rms = Mathf.Sqrt(sum / samples.Length);

            speaking = rms * sensitivity > threshold * sensitivity;
        }

        talking = speaking;

        mouthAnimator.SetBool(TALKING, talking);
    }




}
