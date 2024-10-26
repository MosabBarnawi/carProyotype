using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeVehicleController : MonoBehaviour
{
    [Header("Driving Settings")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve brakingCurve;
    public AnimationCurve steeringCurve;
    public AnimationCurve antiDriftCurve;
    public float maxSpeed = 100f;
    public float maxSteeringAngle = 30f;
    public float tiltAmount = 10f;

    [Header("Drive Type")]
    public bool isFrontWheelDrive = true;

    [Header("Gizmos")]
    public bool showGizmos = true;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
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
        float speedFactor = rb.velocity.magnitude / maxSpeed;
        float appliedAcceleration = moveInput >= 0 ? accelerationCurve.Evaluate(speedFactor) : brakingCurve.Evaluate(speedFactor);
        Vector3 force = transform.forward * moveInput * appliedAcceleration;
        rb.AddForce(force, ForceMode.Acceleration);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    void HandleSteering()
    {
        float turnInput = Input.GetAxis("Horizontal");
        float speedFactor = rb.velocity.magnitude / maxSpeed;
        float steerAngle = turnInput * steeringCurve.Evaluate(speedFactor) * maxSteeringAngle;
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

        float speedFactor = rb.velocity.magnitude / maxSpeed;
        float antiDriftFactor = antiDriftCurve.Evaluate(speedFactor);

        rb.velocity = forwardVelocity + rightVelocity * antiDriftFactor;
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
