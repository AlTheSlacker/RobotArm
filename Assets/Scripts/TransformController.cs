using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * this script is attached to each of the DriveP? gameobjects and is used to control their motion
 * 
 * do not attempt to directly access the transforms without using the methods here
 * 
 * FreeDOF is the free degree of freedom, where 1-6 are the 3 translations, followed by the 3 rotations, eg DOF 5 is y rotation
 * only one DOF can be free per DriveP
 * 
 * base velocity is the speed at which the transform will move to the desired location in degrees / second
 * this velocity can be globally overwritten from the ArmController
 * 
 * LockMin/Max can be typed in or set visually with the handles. 0 degrees is the direction of the parent transform axis
 * if you position the transform in the editor by changing the rotation angle, the child body will rotate within the static lock angles
 * i.e. if you set a mobility range for an arm, you can then set the initial rotation of the arm using the editor, without messing up the lock angles
 * 
*/

public class TransformController : MonoBehaviour
{

    [SerializeField] private int freeDOF = 0;
    [SerializeField] private float baseVelocity = 20;
    public int FreeDOF { get => freeDOF; }
    public float BaseVelocity { get => baseVelocity; set => baseVelocity = value; }

    [SerializeField] private float lockMin;
    [SerializeField] private float lockMax;
    public float LockMin { get => lockMin; set => lockMin = value; }
    public float LockMax { get => lockMax; set => lockMax = value; }

    [HideInInspector] public float initialOffset;

    private float targetPosition = 0;
    private Transform tDriven;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float[] drivenPosRot = new float[6] { 0, 0, 0, 0, 0, 0 };

    void Start()
    {
        tDriven = GetComponent<Transform>();
        drivenPosRot[FreeDOF - 1] = ConvertTo0360(initialOffset);
        targetPosition = drivenPosRot[FreeDOF - 1];
        initialPosition = tDriven.localPosition;
        initialRotation = tDriven.localRotation;

        if (FreeDOF > 3)
        {
            lockMin = ConvertTo0360(lockMin);
            lockMax = ConvertTo0360(lockMax);
        }
    }

    void FixedUpdate()
    {
        float displacementPerDeltaTime = baseVelocity * Time.deltaTime;
        float displacementToTarget = ShortestRoute(drivenPosRot[FreeDOF - 1], targetPosition);
        if (displacementToTarget != 0)
        {
            if (Mathf.Abs(displacementToTarget) >= Mathf.Abs(displacementPerDeltaTime))
            {
                displacementPerDeltaTime = displacementPerDeltaTime * Mathf.Sign(displacementToTarget);
                ApplyDisplacement(displacementPerDeltaTime);
            }
            else
            {
                ApplyDisplacement(displacementToTarget);
            }
        }
    }

    private void ApplyDisplacement(float disp)
    {
        drivenPosRot[FreeDOF - 1] = LockCheck(drivenPosRot[FreeDOF - 1] + disp);
        tDriven.localPosition = initialPosition + new Vector3(drivenPosRot[0], drivenPosRot[1], drivenPosRot[2]);
        tDriven.localRotation = Quaternion.identity * Quaternion.Euler(new Vector3(drivenPosRot[3], drivenPosRot[4], drivenPosRot[5]));
    }

    public void RelativeDisplacement(float delta)
    {
        if (freeDOF > 3)
        {
            targetPosition = ConvertTo0360(drivenPosRot[FreeDOF - 1] + delta);
        }
        else
        {
            targetPosition = drivenPosRot[FreeDOF - 1] + delta;
        }
    }

    public void AbsoluteDisplacement(float delta)
    {
        if (freeDOF > 3)
        {
            targetPosition = ConvertTo0360(delta);
        }
        else
        {
            targetPosition = delta;
        }
    }


    public void ReturnToInitialCondition()
    {
        drivenPosRot = new float[6] { 0, 0, 0, 0, 0, 0 };
        drivenPosRot[FreeDOF - 1] = ConvertTo0360(initialOffset);
        targetPosition = drivenPosRot[FreeDOF - 1];
        tDriven.localPosition = initialPosition;
        tDriven.localRotation = initialRotation;
    }

    private float ConvertTo0360(float inputAngle)
    {
        if (inputAngle >= 0 && inputAngle <= 360) return inputAngle;
        if (inputAngle < 0) return (360 + inputAngle);
        if (inputAngle > 360) return (inputAngle - 360);
        return inputAngle;
    }

    private float ShortestRoute(float a, float b)
    {
        float signedDisplacement = b - a;
        if (freeDOF > 3)
        {
            if (b - a > 180) signedDisplacement = b - a - 360;
            if (b - a < -180) signedDisplacement = b - a + 360;
        }
        else
        {

        }
        return signedDisplacement;
    }

    private float LockCheck(float testValue)
    {
        if (freeDOF > 3) testValue = ConvertTo0360(testValue);

        if (lockMin > lockMax)
        {
            if(testValue < lockMax || testValue > lockMin)
            {
                return testValue;
            }
            else
            {
                targetPosition = drivenPosRot[FreeDOF - 1];
                return drivenPosRot[FreeDOF - 1];
            }
        }
        else if (testValue < lockMin || testValue > lockMax)
        {
            targetPosition = drivenPosRot[FreeDOF - 1];
            return drivenPosRot[FreeDOF - 1];
        }
        return testValue;
    }

}
