using UnityEngine;

public class WeldingSparks : MonoBehaviour
{
    [Header("Spark Settings")]
    [SerializeField] private int sparkCount = 30;
    [SerializeField] private float sparkSpeed = 3f;
    [SerializeField] private float sparkLifetime = 0.6f;
    [SerializeField] private float sparkSize = 0.05f;
    [SerializeField] private Color sparkColorStart = new Color(1f, 0.8f, 0.2f); // orange
    [SerializeField] private Color sparkColorEnd = new Color(1f, 0.2f, 0f, 0f); // fade to red transparent

    private ParticleSystem sparks;

    void Awake()
    {
        BuildParticleSystem();
    }

    void BuildParticleSystem()
    {
        GameObject go = new GameObject("WeldSparks");
        sparks = go.AddComponent<ParticleSystem>();

        // Main
        var main = sparks.main;
        main.startLifetime = sparkLifetime;
        main.startSpeed = sparkSpeed;
        main.startSize = sparkSize;
        main.startColor = new ParticleSystem.MinMaxGradient(sparkColorStart, sparkColorEnd);
        main.gravityModifier = 0.4f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = 200;

        // Emission — burst only
        var emission = sparks.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, sparkCount)
        });
        emission.rateOverTime = 0;

        // Shape — cone spray
        var shape = sparks.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.01f;

        // Color over lifetime
        var colorOverLifetime = sparks.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(sparkColorStart, 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // Size over lifetime — shrink
        var sizeOverLifetime = sparks.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Trail for streaky sparks
        var trails = sparks.trails;
        trails.enabled = true;
        trails.lifetime = 0.15f;
        trails.minVertexDistance = 0.02f;
        trails.widthOverTrail = 0.5f;
        trails.dieWithParticles = true;
    }

    public void EmitAt(Vector3 worldPosition, Vector3 normal)
    {
        sparks.transform.position = worldPosition;
        // Aim sparks along surface normal
        sparks.transform.rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(-90f, 0f, 0f);
        sparks.Play();
    }
}