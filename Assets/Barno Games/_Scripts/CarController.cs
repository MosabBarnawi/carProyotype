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

    [Header("Debuggin Options"), Space(10)]
    [SerializeField] private float suspentionCorrectionMultiplier = 0.01f;

    #region Unity Methods

    private void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Suspension();
    }

    private void OnCollisionEnter(Collision collision)
    {
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
        Gizmos.color = Color.cyan;
        Handles.color = Color.cyan;
        foreach (var item in rayPoints)
        {
            Vector3 pos = item.position;
            //pos.y -= springTravel + (restLength / 2);
            //Handles.DrawSolidDisc(pos, Vector3.right, wheelRadius);
            //Gizmos.DrawSphere(item.position, wheelRadius);
        }

        if (carRB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position - carRB.centerOfMass, 0.5f);
        }
    }

    #endregion //Unity Methods

    private void Suspension()
    {
        foreach (Transform rayPoint in rayPoints)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            float maxRayDistance = maxLength + wheelRadius;

            // -rayPoint.up down
            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxDistance: maxRayDistance, drivableMask))
            {
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
                // DEBUG
                //Vector3 endPosition = rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up;
                Vector3 endPosition = rayPoint.position + maxRayDistance * -rayPoint.up;
                Debug.DrawLine(rayPoint.position, end: endPosition, Color.green);
            }
        }

    }


}
