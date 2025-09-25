using UnityEngine;

public class ShipRock : MonoBehaviour
{
    [Header("Amplitudes")]
    public float pitchDeg = 3f;   // forward/back tip (degrees)
    public float rollDeg  = 6f;   // left/right tilt (degrees)
    public float bobMeters = 0.15f; // up/down (meters)

    [Header("Frequencies (Hz)")]
    public float pitchHz = 0.20f;
    public float rollHz  = 0.15f;
    public float bobHz   = 0.25f;

    [Header("Natural Variation")]
    [Range(0,1)] public float noiseAmount = 0.4f;
    public float noiseSpeed = 0.35f;

    [Header("Master Control")]
    [Range(0,2)] public float intensity = 1f;

    Transform _t;
    Vector3 _baseLocalPos;
    Quaternion _baseLocalRot;
    Vector2 _seed;

    void Awake()
    {
        _t = transform;
        _baseLocalPos = _t.localPosition;
        _baseLocalRot = _t.localRotation;
        _seed = new Vector2(Random.value * 10f, Random.value * 10f);
    }

    void Update()
    {
        float t = Time.time;

        float n1 = (Mathf.PerlinNoise(_seed.x, t * noiseSpeed) - 0.5f) * 2f;
        float n2 = (Mathf.PerlinNoise(_seed.y, (t + 11.3f) * noiseSpeed) - 0.5f) * 2f;

        float sPitch = Mathf.Sin(Mathf.PI * 2f * pitchHz * t + _seed.x);
        float sRoll  = Mathf.Sin(Mathf.PI * 2f * rollHz  * t + _seed.y);
        float sBob   = Mathf.Sin(Mathf.PI * 2f * bobHz   * t + (_seed.x + _seed.y) * 0.5f);

        float pitch = intensity * pitchDeg * Mathf.Lerp(sPitch, n1, noiseAmount);
        float roll  = intensity * rollDeg  * Mathf.Lerp(sRoll,  n2, noiseAmount);
        float bob   = intensity * bobMeters * sBob;

        _t.localRotation = _baseLocalRot * Quaternion.Euler(pitch, 0f, -roll);
        _t.localPosition = _baseLocalPos + Vector3.up * bob;
    }
}
