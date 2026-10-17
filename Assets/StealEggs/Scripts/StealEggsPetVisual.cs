using UnityEngine;

/// <summary>Runtime visual for a newly hatched original pet and its passive income.</summary>
public class StealEggsPetVisual : MonoBehaviour
{
    public string petName = "Sproutling";
    public float coinsPerSecond = 1f;
    public Color bodyColor = new Color(.3f, .8f, .45f);
    private float phase;
    private Vector3 initialPosition;

    public void Initialize(string name, float income, Color color)
    {
        petName = name; coinsPerSecond = income; bodyColor = color;
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = color;
        phase = Random.Range(0f, 6f);
        initialPosition = transform.position;
    }

    private void Start() { initialPosition = transform.position; }

    private void Update()
    {
        transform.Rotate(0f, 40f * Time.deltaTime, 0f, Space.World);
        Vector3 p = initialPosition;
        p.y += Mathf.Sin(Time.time * 2f + phase) * .12f;
        transform.position = p;
    }
}
