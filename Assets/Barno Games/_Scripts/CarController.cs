using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivableMask;
    [SerializeField] private Transform accelerationPoint;

    [Header("Suspension Settings"), Space(10)]
    [Tooltip("The max force the sprint can exert, when its fully compressed")]
    [SerializeField] private float springStiffness;
    [Tooltip("The Damer Fluid in spring suspension, The lower the value the bouncher it will be")]
    [SerializeField] private float damperStiffness;

    [Tooltip("The standard Length when not being compressed or Streadhed")]
    [SerializeField, Min(0)] private float restLength;

    [Tooltip("The Maximum distance the sprint can either compress or exted from its notmal (rest) position")]
    [SerializeField] private float springTravel;
    [SerializeField, Min(0.1f)] private float wheelRadius;
    public float collisionForce;

    [Header("Custom Gravity"), Space(10)]
    [SerializeField] private bool enableCustomGravity = false;
    [SerializeField] private float downwardForce = -9.81f;
    [SerializeField] private bool useAtPoint = false;
    [SerializeField] private bool customForceType = false;
    [SerializeField] private ForceMode forceType = ForceMode.Acceleration;

    [Header("Debuggin Options"), Space(10)]
    [SerializeField] private float suspentionCorrectionMultiplier = 0.01f;


    private int[] wheelsIsGrounded = new int[4];
    //private bool[] wheelsIsGrounded = new bool[4];
    private bool isGrounded = false;

    //[Header("Input"), Space(10)]
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("Car SAettings"), Space(10)]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;

    [Tooltip("The higher the value the sharper it turns")]
    [SerializeField] private float steerStrength = 15f;

    [Tooltip("To Adjust power steer ratio dynamically depending on speed ration")]
    [SerializeField] private AnimationCurve turningCurve;

    [SerializeField] private float sideDragCoefficient = 1f;

    Vector3 currentCarLocalVelocity = Vector3.zero;

    /// <summary>
    /// <list type="bullet">
    /// <item>Velocity Relavent to the max Speed.</item>
    /// <item>will return a value bwtwen 0-1  where 0 is not moving and 1 is moving at maxSpeed.</item>
    /// </list>
    /// </summary>
    float carVelocityRatio = 0;


    Vector3 dragForce_DEBUG;

    #region Unity Methods

    private void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateDownwardGravity();
        CalculateCarVelocity();
        Movement();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void OnCollisionEnter(Collision collision)
    {
        return;
        // Check if the colliding object is the one you want to apply the force
        if (collision.gameObject.CompareTag("Player")) // Replace with your object's tag
        {
            // Apply a force to the car in the direction opposite to the collision
            Vector3 collisionForceDirection = (transform.position - collision.transform.position).normalized;
            carRB.AddForce(collisionForceDirection * collisionForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.cyan;
        //Handles.color = Color.cyan;
        foreach (var item in rayPoints)
        {
            Vector3 pos = item.position;
            //pos.y -= springTravel + (restLength / 2);
            //Handles.DrawSolidDisc(pos, Vector3.right, wheelRadius);
            //Gizmos.DrawSphere(item.position, wheelRadius);

            //Handles.DrawLine(item.position, carRB.velocity);

            //Gizmos.color = Color.yellow;
            //Gizmos.DrawLine(item.position, item.position + dragForce_DEBUG);
        }

        if (carRB != null)
        {
            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(transform.position - carRB.centerOfMass, 0.5f);

            Handles.color = Color.red;
            //Vector3 center = transform.position - carRB.centerOfMass;
            Vector3 center = carRB.worldCenterOfMass;
            float radius = 0.5f;
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
            Handles.DrawSolidDisc(center, Vector3.right, radius);

            //Gizmos.color = Color.red;
            Vector3 worldCenterOfMass = carRB.worldCenterOfMass;
            Gizmos.DrawLine(worldCenterOfMass, worldCenterOfMass + dragForce_DEBUG);
        }
    }

    #endregion //Unity Methods

    ///////////////////////////////////////////////////////
    ////////////////////// TETSTING ///////////////////////
    ///////////////////////////////////////////////////////
    /// <summary>
    /// JUST TESTING CURRENTLY NOT IMPLEMENTED FULLLY
    /// </summary>
    private void CalculateDownwardGravity()
    {
        /*TODO:: IF NOT GROUNDE DRAW INFINATE RAY
         * Increase drop speed, cap at maximum velcity
         * when close to ground decrease so that it dose not clip through
        */


        if (!enableCustomGravity)
            return;

        if (isGrounded)
            return;

        ForceMode forceMode = ForceMode.Acceleration;
        if (customForceType)
            forceMode = forceType;

        if (useAtPoint)
        {
            //float accelerationDirection = acceleration * moveInput;

            // ForceMode.Acceleration so that the acceleration is not dependant on the mass
            carRB.AddForceAtPosition(downwardForce * (-transform.up), accelerationPoint.position, forceMode);
        }
        else
        {
            Vector3 forceDirection = Vector3.down;
            forceDirection.y += downwardForce;
            carRB.AddForce(forceDirection, forceMode);
        }

    }

    #region Movement

    private void Movement()
    {
        if (isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();

            //foreach (var item in rayPoints)
            //{
            //    TireSteering(item, tireMass:1, tireGripFactor: 1);
            //}
            SidewaysDrag();
        }
    }

    private void Acceleration()
    {
        float accelerationDirection = acceleration * moveInput;

        // ForceMode.Acceleration so that the acceleration is not dependant on the mass
        carRB.AddForceAtPosition(accelerationDirection * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    /// <summary>
    /// <list type="bullet">
    /// <item>Custom Deceleration otherwiaw the movement will be moving for a longer time.</item>
    /// <item>Can Increase Drag on RB [BUT] that means its applied in all direction at all times, and will fall slow, as well as not move in side direction.</item>
    /// </list>
    /// </summary>
    private void Deceleration()
    {
        float decelerationDirection = deceleration * Mathf.Abs(carVelocityRatio);

        // ForceMode.Acceleration so that the acceleration is not dependant on the mass
        carRB.AddForceAtPosition(decelerationDirection * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        //rb.AddRelativeTorque(steerStrength * steerInput * turning curve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * rb.transform.up, ForceMode.Acceleration);

        float desiredSteerDirection = steerStrength * steerInput;
        // Mathf.sign destermines if car is going forward or backawards.
        float curveData = turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio);
        //carRB.AddTorque(desiredSteerDirection * curveData * transform.up, ForceMode.Acceleration);
        carRB.AddRelativeTorque(desiredSteerDirection * curveData * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        float currentSidewaySpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaySpeed * sideDragCoefficient;

        Vector3 dragForce = dragForce_DEBUG = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void TireSteering(Transform tireTransform, float tireMass, float tireGripFactor)
    {
        // world-space direction of the spring force.
        Vector3 steeringDir = tireTransform.right;
        // world - space velocity of the suspension
        Vector3 tireWorldVel = carRB.GetPointVelocity(tireTransform.position);
        // what it's the tire's velocity in the steering direction ?
        // note that steeringDir is a unit vector, so this returns the magnitude of tireWorldVel
        // as projected onto steeringDir
        float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);

        // the change in velocity that we're looking for is - steeringvel * gripFactor
        // gripFactor is in range 0-1, 0 means no grip, 1 means full grip.
        float desiredVelChange = -steeringVel * tireGripFactor;

        // turn change in velocity into an acceleration (acceleration = change in vel / time)
        // this will produce the acceleration necessary to change the velocity by desiredVelChange in 1 physics step
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
        // Force = Mass * Acceleration, so multiply by the mass of the tire and apply as a force!
        carRB.AddForceAtPosition(steeringDir * tireMass * desiredAccel, tireTransform.position);
    }

    #endregion //Movement

    #region Car Status Check

    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i < wheelsIsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsIsGrounded[i];
        }

        if (tempGroundedWheels > 1)
            isGrounded = true;
        else
            isGrounded = false;
    }

    private void CalculateCarVelocity()
    {
        // Returns the car velocity in its local axis.
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.velocity);

        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    #endregion //

    #region Input Handling

    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    #endregion // Input Handling

    #region Suspension Functions

    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            Transform rayPoint = rayPoints[i];
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            float maxRayDistance = maxLength + wheelRadius;

            // -rayPoint.up down
            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxDistance: maxRayDistance, drivableMask))
            {
                wheelsIsGrounded[i] = 1;
                //wheelsIsGrounded[i] = true;

                // calculate contraction
                float currentSprintLength = hit.distance - wheelRadius;

                // restLength - currentSprintLength =  how far has the spring has moved from its rest position.
                // current sprint length / spring travel to normalized it.
                float springCompression = (restLength - currentSprintLength) / springTravel;

                // caluclate spring speed so that we can calcualte damppening speed required
                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoint.position), rayPoint.up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoint.up, rayPoint.position);
                // DEBUG
                Debug.DrawLine(rayPoint.position, hit.point, Color.red);

                // DEBUG: Draw the corrected suspension line in blue
                Vector3 correctionForceDirection = netForce * rayPoint.up;
                Debug.DrawLine(rayPoint.position, rayPoint.position + correctionForceDirection * suspentionCorrectionMultiplier, Color.blue);

                /*
                // world-space direction of the sopring force
                Vector3 sprintDir = rayPoint.up;
                // world-space velocity of this tire
                Vector3 tireWorldVel = carRB.GetPointVelocity(rayPoint.position);

                // calculate offset from raycast
                float offset = restLength - hit.distance;

                // calculate velocity along the spring direction
                // note that springDir is a unit vector, so this returns the magnitude of tireWorldBel
                // as projected onto springDir
                float vel = Vector3.Dot(sprintDir, tireWorldVel);

                // calculate the magnitude of the damppend spring force
                //float force = (offset * springStregth) - (vel * springDamer);
                float force = (offset * springStiffness) - (vel * damperStiffness);

                carRB.AddForceAtPosition(sprintDir * force, rayPoint.position);
                */
            }
            else
            {
                wheelsIsGrounded[i] = 0;
                //wheelsIsGrounded[i] = false;

                // DEBUG
                //Vector3 endPosition = rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up;
                Vector3 endPosition = rayPoint.position + maxRayDistance * -rayPoint.up;
                Debug.DrawLine(rayPoint.position, end: endPosition, Color.green);
            }
        }

    }

    #endregion // Suspension Functions


}
