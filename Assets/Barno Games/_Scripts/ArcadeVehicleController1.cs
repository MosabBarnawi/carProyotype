using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeVehicleController1 : MonoBehaviour
{
    [Header("Driving Settings")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve brakingCurve;
    public AnimationCurve steeringCurve;
    public AnimationCurve antiDriftCurve;

    [Header("Per Tier Settings")]
    public float[] speedTiers = { 0.5f, 0.75f, 1.0f }; // Speed tiers as ratios of max speed
    public AnimationCurve[] accelerationPerTier;
    public AnimationCurve[] brakingPerTier;
    public AnimationCurve[] steeringPerTier;
    public AnimationCurve[] antiDriftPerTier;

    [Header("General Settings")]
    public float maxSpeed = 100f;
    public float maxSteeringAngle = 30f;
    public float tiltAmount = 10f;

    [Header("Drive Type")]
    public bool isFrontWheelDrive = true;

    [Header("Gizmos")]
    public bool showGizmos = true;

    [SerializeField] private Transform[] tireLocations;

    private Rigidbody rb;
    private float currentSpeedRatio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        currentSpeedRatio = rb.velocity.magnitude / maxSpeed;
        HandleSteering();
    }

    void FixedUpdate()
    {
        HandleAcceleration();
        ApplyAntiDrift();
    }

    void HandleAcceleration()
    {
        float moveInput = Input.GetAxis("Vertical");
        float appliedAcceleration = moveInput >= 0 ? GetCurveValue(accelerationCurve, accelerationPerTier) : GetCurveValue(brakingCurve, brakingPerTier);
        Vector3 force = transform.forward * moveInput * appliedAcceleration * maxSpeed;
        rb.AddForce(force, ForceMode.Acceleration);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        // Example: Apply tire forces (visualize with gizmos)
        ApplyTireForces(moveInput, force);
    }

    void HandleSteering()
    {
        float turnInput = Input.GetAxis("Horizontal");
        float steerAngle = turnInput * GetCurveValue(steeringCurve, steeringPerTier) * maxSteeringAngle * Mathf.Sign(Vector3.Dot(rb.velocity, transform.forward));
        transform.Rotate(0, steerAngle * Time.deltaTime, 0);

        // Tilting the car
        float tilt = turnInput * tiltAmount * -1;
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, tilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2);
    }

    void ApplyAntiDrift()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right);

        float antiDriftFactor = GetCurveValue(antiDriftCurve, antiDriftPerTier);
        rb.velocity = forwardVelocity + rightVelocity * antiDriftFactor * maxSpeed;
    }

    float GetCurveValue(AnimationCurve mainCurve, AnimationCurve[] perTierCurves)
    {
        int tierIndex = GetSpeedTierIndex();
        AnimationCurve selectedCurve = perTierCurves[tierIndex] != null ? perTierCurves[tierIndex] : mainCurve;
        return selectedCurve.Evaluate(currentSpeedRatio);
    }

    int GetSpeedTierIndex()
    {
        for (int i = 0; i < speedTiers.Length; i++)
        {
            if (currentSpeedRatio <= speedTiers[i])
                return i;
        }
        return speedTiers.Length - 1;
    }

    void ApplyTireForces(float moveInput, Vector3 appliedForce)
    {
        if (tireLocations == null || tireLocations.Length == 0)
            return;

        for (int i = 0; i < tireLocations.Length; i++)
        {
            Vector3 tirePosition = tireLocations[i].position;
            Vector3 forceDirection = (appliedForce / tireLocations.Length).normalized;
            Debug.DrawRay(tirePosition, forceDirection * 2f, Color.green); // Example: Draw gizmo to visualize force direction
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + rb.velocity);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5);
    }
}
