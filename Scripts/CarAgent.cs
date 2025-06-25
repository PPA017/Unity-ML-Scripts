using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;
public class CarAgent : Agent
{
    private float currentSteerAngle;
    private float currentBrakeForce;

    private LapTimer lapTimer;

    [Header("Surface settings")]
    [SerializeField] private float dirtSpeed = 0.7f;
    [SerializeField] private float dirtSteer = 0.9f;
    [SerializeField] private float iceBrake = 0.3f;
    [SerializeField] private float iceSteer = 1.3f;

    [Header("Car Settings")]
    [SerializeField] private float motorForce = 1000f;
    [SerializeField] private float brakeForce = 8000f;
    [SerializeField] private float maxSteerAngle = 35f;

    [Header("Wheel Colliders")]
    [SerializeField] public WheelCollider frontLeftWheelCollider;
    [SerializeField] public WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    [Header("Wheels")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    [Header("Slow timers")]
    [SerializeField] private int slowStepLimit = 10;
    [SerializeField] private float slowSpeedLimit = 0.5f;

    [Header("Speedometer")]
    public TextMeshProUGUI speed;

    public float speedSound;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;

    public float brakeInput;
    private bool isOnDirt = false;
    private bool isOnIce = false;
    public bool isDrifting;
    public float isBraking;
    private void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.rotation;

        lapTimer = FindAnyObjectByType<LapTimer>();
        lapTimer.StartLap();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("begin");
        transform.localPosition = startPos;
        transform.localRotation = startRot;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.linearVelocity.magnitude);
        sensor.AddObservation(currentSteerAngle);
        sensor.AddObservation(rb.angularVelocity.y);
        sensor.AddObservation(isOnDirt ? 1f : 0f);
        sensor.AddObservation(isOnIce ? 1f : 0f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float steerInput = actions.ContinuousActions[0];
        float accelerationInput = actions.ContinuousActions[1];
        brakeInput = actions.ContinuousActions[2];

        if (rb.linearVelocity.z > 0.1f)
        {
            AddReward(0.01f);
        }

        if (rb.linearVelocity.magnitude < 0.5f)
        {
            AddReward(-0.05f);
        }

        HandleMotor(accelerationInput, brakeInput);
        HandleSteering(steerInput);
        HandleDrifting();
        UpdateWheels();
        SimulateWeightTransfer();
    }

    private void HandleMotor(float acceleration, float braking)
    {
        float currentMotorForce = motorForce;

        float motorTorque = acceleration * motorForce;

        float forwardVelocity = Vector3.Dot(rb.linearVelocity, transform.forward);

        if (forwardVelocity < -0.1f && acceleration <= 0)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;
            ApplyBraking(0f);

            return;
        }

        frontLeftWheelCollider.motorTorque = motorTorque;
        frontRightWheelCollider.motorTorque = motorTorque;

        if (acceleration > 0)
        {
            ApplyBraking(0);
        }
        else
        {
            ApplyBraking(braking * brakeForce);
        }
    }

    private void HandleDrifting()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            RearFrictionChange(0.4f);

            rearLeftWheelCollider.brakeTorque = brakeForce * 0.3f;
            rearRightWheelCollider.brakeTorque = brakeForce * 0.3f;

            frontLeftWheelCollider.steerAngle *= 1.2f;
            frontRightWheelCollider.steerAngle *= 1.2f;
        }
        else
        {
            RearFrictionChange(1f);
        }

        isDrifting = Input.GetKey(KeyCode.Space) || Mathf.Abs(currentSteerAngle) > 35f;


    }

    private void RearFrictionChange(float grip)
    {
        WheelFrictionCurve sidewaysFriction = rearLeftWheelCollider.sidewaysFriction;
        sidewaysFriction.stiffness = grip;
        rearLeftWheelCollider.sidewaysFriction = sidewaysFriction;
        rearRightWheelCollider.sidewaysFriction = sidewaysFriction;
    }

    private void FrontFrictionChange(float grip)
    {
        WheelFrictionCurve sidewaysFriction = frontLeftWheelCollider.sidewaysFriction;
        sidewaysFriction.stiffness = grip;
        frontLeftWheelCollider.sidewaysFriction = sidewaysFriction;
        frontRightWheelCollider.sidewaysFriction = sidewaysFriction;
    }

    private void ApplyBraking(float brakeForce)
    {
        currentBrakeForce = brakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce * 1f;
        frontLeftWheelCollider.brakeTorque = currentBrakeForce * 1f;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce * 1.8f;
        rearRightWheelCollider.brakeTorque = currentBrakeForce * 1.8f;
    }

    private void HandleSteering(float steerInput)
    {
        float currentMaxSteerAngle = maxSteerAngle;

        if (isOnDirt)
        {
            currentMaxSteerAngle *= dirtSteer;
        }

        currentSteerAngle = maxSteerAngle * steerInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    public float GetSteeringAngle()
    {
        return currentSteerAngle;
    }


    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            if (lapTimer != null)
            {

                lapTimer.NullifyLap();
            }
            EndEpisode();
            Debug.Log("Wall collision detected!");

        }

        if (other.gameObject.CompareTag("RearWall"))
        {
            AddReward(-30f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Gate"))
        {
            AddReward(0.2f);
            Debug.Log("Checkpoint reached!");
        }

        if (other.gameObject.CompareTag("Sector"))
        {
            AddReward(0.7f);

            Debug.Log("Sector reached!");
        }

        if (other.gameObject.CompareTag("BigSector1"))
        {
            AddReward(1f);

            Debug.Log("Sector reached!");
        }

        if (other.gameObject.CompareTag("BigSector2"))
        {
            AddReward(2f);

            Debug.Log("Sector reached!");
        }

        if (other.gameObject.CompareTag("BigSector3"))
        {
            AddReward(3f);

            Debug.Log("Sector reached!");
        }
        if (other.gameObject.CompareTag("BigSector4"))
        {
            AddReward(4f);

            Debug.Log("Sector reached!");
        }

        if (other.gameObject.CompareTag("BigSector5"))
        {
            AddReward(5f);

            Debug.Log("Sector reached!");
        }

        if (other.gameObject.CompareTag("Finish"))
        {
            SetReward(20f);
            if (lapTimer != null)
            {
                lapTimer.EndLap();
            }


            Debug.Log("Finish line reached!");
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Dirt Road"))
        {
            isOnDirt = true;
            isOnIce = false;
            ApplyDirtRoad();
            Debug.Log("On dirt road");
        }

        if (other.gameObject.CompareTag("Ice Road"))
        {
            isOnDirt = false;
            isOnIce = true;
            ApplyIceRoad();
            Debug.Log("On ice road");
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall collision detected!");
            AddReward(-0.05f);

            if (lapTimer != null)
            {

                lapTimer.NullifyLap();
            }

            EndEpisode();
        }
    }

    private void OnTriggerExit(Collider stuff)
    {
        if (stuff.CompareTag("Dirt Road") || stuff.CompareTag("Ice Road"))
        {
            ResetSurface();
            Debug.Log("Resetting to normal surface");

        }
    }
    private void ApplyDirtRoad()
    {
        isOnDirt = true;
        isOnIce = false;

        motorForce *= dirtSpeed;
        maxSteerAngle = 30;
    }

    private void ApplyIceRoad()
    {
        isOnDirt = false;
        isOnIce = true;

        RearFrictionChange(0.4f);
        FrontFrictionChange(0.9f);

        brakeForce = 8000f;
        maxSteerAngle *= 1.2f;

        rb.angularVelocity *= 2f;
    }
    private void SimulateWeightTransfer()
    {
        if (!isOnIce) return;

        float brakeWeightTransfer = currentBrakeForce / brakeForce * 0.2f;
        rb.centerOfMass = new Vector3(0, -0.3f, brakeWeightTransfer);

        float turnWeightTransfer = -currentSteerAngle / maxSteerAngle * 0.1f;
        rb.centerOfMass += new Vector3(turnWeightTransfer, 0, 0);
    }

    private void ResetSurface()
    {
        isOnDirt = false;
        isOnIce = false;


        motorForce = 1000f;
        maxSteerAngle = 35f;
        brakeForce = 8000f;
        RearFrictionChange(1f);
        FrontFrictionChange(1f);

        rb.centerOfMass = Vector3.zero;
    }

    private void Update()
    {
        if (speed != null && rb != null)
        {
            int currentSpeed = ((int)(rb.linearVelocity.magnitude * 3.6f)); 
            speed.text = $"{currentSpeed} km/h";
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 5f; 
                Debug.Log("Fast forward ON");
            }
            else
            {
                Time.timeScale = 1f; 
                Debug.Log("Fast forward OFF");
            }
        }

    }
}