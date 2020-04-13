using System.Collections;
using UnityEngine;

/*
 * Control the behaviour of the arm from here, some of this code is specific to the arm geometry, 
 * most will adapt easily to other installations
 * 
 * Select control methods here, some methods will overide others e.g. enableTracking will constantly interfere with other methods
 * 
 * To initially position the arm set the transform angle for the DriveP? transform in the editor, this will automatically sort out lock angles etc.
 * 
 * Do not directly change transforms from code without using the TransformController methods or you will be sad
 * 
*/


public class ArmController : MonoBehaviour
{
    [SerializeField] private TransformController[] clampDriver = new TransformController[] { };
    [SerializeField] private TransformController[] transformDriver = new TransformController[] { };
    [SerializeField] private GameObject trackObject = null;
    [SerializeField] private float allRotationalVelocity = 20;
    [SerializeField] private bool enableManualControl = true;
    [SerializeField] private bool enableMoveToPosition = false;
    [SerializeField] private bool enableTracking = false;

    private float rotationalStep = 0;
    private float translationalStep = 0.001f;

    void Start()
    {
        // set the angular velocity of the rotational transforms in one hit, comment this out to have them moving at different angular velocities and set them individually
        SetAngularVel(allRotationalVelocity);

        // make sure the angular displacement of a single step is completed within a single FixedUpdate to avoid unexpected behaviour
        rotationalStep = allRotationalVelocity * Time.deltaTime * 1.05f;

        // example of pre-recorded positioning, including time control
        if (enableMoveToPosition) MoveToPosition();
    }

    void Update()
    {
        // ManualController() allows keyboard control using the key pairs [rf] [tg] [yh] [uj] [ik] [ol] [p;]
        if (enableManualControl) ManualController();
    }

    void FixedUpdate()
    {
        // do not use this with MoveToPosition() or ManualController().
        // this will automatically track the target object as you move it around wasd + qe for y displacements
        if (enableTracking) TrackObject();
    }

    private void ManualController( )
    {
        // note: GetAxis instead of GetButton to save coding extra lines for +/- (due to keyboard buffering, can make the arm feel a little laggy)
        if (Mathf.Abs(Input.GetAxis("P1")) > 0.4) transformDriver[0].RelativeDisplacement(rotationalStep * Mathf.Sign(Input.GetAxis("P1")));
        if (Mathf.Abs(Input.GetAxis("P2")) > 0.4) transformDriver[1].RelativeDisplacement(rotationalStep * Mathf.Sign(Input.GetAxis("P2")));
        if (Mathf.Abs(Input.GetAxis("P3")) > 0.4) transformDriver[2].RelativeDisplacement(rotationalStep * Mathf.Sign(Input.GetAxis("P3")));
        if (Mathf.Abs(Input.GetAxis("P4")) > 0.4) transformDriver[3].RelativeDisplacement(rotationalStep * Mathf.Sign(Input.GetAxis("P4")));
        if (Mathf.Abs(Input.GetAxis("P5")) > 0.4) transformDriver[4].RelativeDisplacement(rotationalStep * Mathf.Sign(Input.GetAxis("P5")));
        if (Mathf.Abs(Input.GetAxis("P6")) > 0.4) transformDriver[5].RelativeDisplacement(rotationalStep * Mathf.Sign(Input.GetAxis("P6")));

        // clamps displace an equal and opposing amount
        if (Mathf.Abs(Input.GetAxis("Clamps")) > 0.4)
        {
            for (int i = 0; i < clampDriver.Length; i++)
            {
                clampDriver[i].RelativeDisplacement(translationalStep * Mathf.Sign(Input.GetAxis("Clamps"))) ;
            }
        }

        // reset all joints to their initial position
        if (Input.GetButton("Jump")) // (Jump.... pro-slacking)
        {
            for (int i = 0; i < clampDriver.Length; i++) clampDriver[i].ReturnToInitialCondition();
            for (int i = 0; i < transformDriver.Length; i++) transformDriver[i].ReturnToInitialCondition();
        }
    }

    private void TrackObject()
    {
        // fun with kinematics
        // this approach allows the solving of a 3R problem without resorting to an inverse kinematic solution
        // the strategy is to reduce the problem to a cosine rule calculation for the two main arms
        // unfortunately this appears more complex than it is as some corrections have to be made for the arm geometry

        // target angles for the 4 active transforms
        float p1DriverAngle = 0, p2DriverAngle = 0, p3DriverAngle = 0, p5DriverAngle = 0;

        const float armLength1 = 0.4584155f;
        const float armLength2 = 0.5481802f;
        const float armLength3 = 0.1586066f;

        // adjustment specific to this arm geometry due to the offset of the grab from the plane of the arm rotation
        const float linearArmOffset = 0.04075f;

        // calculate angular correction for the P1 driver transform due to the linearArmOffset
        Vector3 trackPosition = trackObject.transform.position;
        float projectedDistance = Vector3.ProjectOnPlane(trackPosition, transformDriver[0].transform.parent.up).magnitude;
        if (projectedDistance < linearArmOffset) projectedDistance = linearArmOffset;
        float angularCorrectionP1 = Mathf.Asin(linearArmOffset / projectedDistance) * 57.2958f;
        
        // orient arm system to face object
        Vector3 directionToTracked = trackPosition - transformDriver[0].transform.position;
        float distanceToTracked = Vector3.Magnitude(trackPosition - transformDriver[0].transform.position);
        p1DriverAngle = CalcSignedCentralAngle(transformDriver[0].transform.parent.right, directionToTracked, transformDriver[0].transform.parent.up) + angularCorrectionP1;

        // angular correction for the P4 driver transform (due to arm geometry - arm3 introduces a translational offset)
        float angularCorrectionP4 = 3;

        // work out P2 and P4 (P3 is an unused rotation axis)
        directionToTracked = trackPosition - transformDriver[1].transform.position;
        distanceToTracked = Vector3.Magnitude(directionToTracked);
        Vector3 direction1 = transformDriver[1].transform.parent.right;

        if (distanceToTracked > armLength1 + armLength2 + armLength3)   
        // can't reach, just point at it
        {
            p2DriverAngle = CalcSignedCentralAngle(direction1, directionToTracked, transformDriver[1].transform.parent.forward);
            p3DriverAngle = 0;
        }
        else
        // offset towards centre by armLength3, then target the offset with with first two arms
        {
            Vector3 offsetTrackPosition = trackPosition - Vector3.Normalize(directionToTracked) * armLength3;
            Vector3 offsetDirectionToTracked = offsetTrackPosition - transformDriver[1].transform.position;
            float reducedDistance =  Vector3.Magnitude(offsetDirectionToTracked);
            float theta = CalcSignedCentralAngle(direction1, offsetDirectionToTracked, transformDriver[1].transform.parent.forward);
            float beta = Mathf.Acos( (armLength1 * armLength1 + reducedDistance * reducedDistance - armLength2 * armLength2) / (2 * armLength1 * reducedDistance) ) * 57.2958f;
            p2DriverAngle = theta + beta;
            float gamma = -(180 - Mathf.Acos((armLength1 * armLength1 + armLength2 * armLength2 - reducedDistance * reducedDistance) / (2 * armLength1 * armLength2)) * 57.2958f);
            p3DriverAngle = gamma + angularCorrectionP4;
        }

        // orient wrist to face tracked object
        direction1 = transformDriver[4].transform.parent.right;
        directionToTracked = trackPosition - transformDriver[4].transform.position;
        p5DriverAngle = CalcSignedCentralAngle(direction1, directionToTracked, transformDriver[4].transform.parent.forward);

        // move the arm
        MoveNow(transformDriver[0], p1DriverAngle);
        MoveNow(transformDriver[1], p2DriverAngle);
        MoveNow(transformDriver[2], p3DriverAngle);
        MoveNow(transformDriver[4], p5DriverAngle);
    }

    // used to get projected angle on a plane
    private float CalcSignedCentralAngle(Vector3 dir1Vector, Vector3 dir2Vector, Vector3 normalVector)
    {
        return Mathf.Atan2(Vector3.Dot(Vector3.Cross(dir1Vector, dir2Vector), normalVector), Vector3.Dot(dir1Vector, dir2Vector)) * Mathf.Rad2Deg;
    }

    // lazy way to set all joint rotational velocities
    private void SetAngularVel(float vel)
    {
        for (int i = 0; i < transformDriver.Length; i++)
        {
            transformDriver[i].BaseVelocity = vel;
        }
    }

    // move without a coroutine, motion starts immediately
    private void MoveNow(TransformController transformController, float position)
    {
        transformController.AbsoluteDisplacement(position);
    }

    // (example) move arm to a position with option to use timings for start of each tranforms motion
    private void MoveToPosition()
    {
        // define an orientation for a transform, to begin at a specific delay after this function is called
        // transformDriver[DriveID], angle (0-360 degrees), delay (seconds)
        StartCoroutine(MoveDriver(transformDriver[1], 125, 0));
        StartCoroutine(MoveDriver(transformDriver[2], 30, 0));
        StartCoroutine(MoveDriver(transformDriver[4], 56, 0));
    }

    IEnumerator MoveDriver(TransformController transformController, float position, float startTime)
    {
        yield return new WaitForSeconds(startTime);
        transformController.AbsoluteDisplacement(position);
    }

}
