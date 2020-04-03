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
        SetAngularVel(allRotationalVelocity);
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
        Vector3 directionToTracked;
        Vector3 trackPosition = trackObject.transform.position;
        Vector3 normalToRefPlane;
        float distanceToTracked;
        float angleToRefPlane;

        // adjustment specific to this arm geometry due to the offset of the grab from the plane of the arm rotation
        float angularArmOffset = 2.249f;

        directionToTracked = trackPosition - transformDriver[0].transform.position;
        distanceToTracked = Vector3.Magnitude(directionToTracked);

        normalToRefPlane = Vector3.Normalize( transformDriver[0].transform.parent.right);
        angleToRefPlane = CalcSignedCentralAngle(normalToRefPlane, directionToTracked, transformDriver[0].transform.parent.up) + angularArmOffset;
        MoveNow(transformDriver[0], angleToRefPlane);

        normalToRefPlane = Vector3.Normalize(transformDriver[4].transform.parent.right);
        directionToTracked = trackPosition - transformDriver[4].transform.position;
        angleToRefPlane = CalcSignedCentralAngle(normalToRefPlane, directionToTracked, transformDriver[4].transform.parent.forward);
        MoveNow(transformDriver[4], angleToRefPlane);




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

    // move arm to a position with option to use timings for start of each tranforms motion
    private void MoveToPosition()
    {
        // define an orientation for a transform, to begin at a specific delay after this function is called
        // transformDriver[DriveID], angle (0-360 degrees), delay (seconds)
        StartCoroutine(MoveDriver(transformDriver[2], 135, 0));
        StartCoroutine(MoveDriver(transformDriver[4], 100, 0));
        StartCoroutine(MoveDriver(transformDriver[0], 90, 6));
        StartCoroutine(MoveDriver(transformDriver[1], 25, 3));
    }

    IEnumerator MoveDriver(TransformController transformController, float position, float startTime)
    {
        yield return new WaitForSeconds(startTime);
        transformController.AbsoluteDisplacement(position);
    }

}
