using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(TransformController))]
public class TransformGUI : Editor
{
    JointAngularLimitHandle angularLimitHandle = new JointAngularLimitHandle();

    protected virtual void OnSceneGUI()
    {
        TransformController transformController = (TransformController)target;
        Vector3 orientationCorrection = new Vector3(0, 0, 0);

        switch (transformController.FreeDOF)
        {
            case 4:
                angularLimitHandle.xMin = transformController.LockMin;
                angularLimitHandle.xMax = transformController.LockMax;
                angularLimitHandle.xRange = new Vector2(-360, 360);
                angularLimitHandle.yMotion = 0;
                angularLimitHandle.zMotion = 0;
                transformController.initialOffset = TransformUtils.GetInspectorRotation(transformController.transform).x;
                break;
            case 5:
                angularLimitHandle.yMin = transformController.LockMin;
                angularLimitHandle.yMax = transformController.LockMax;
                angularLimitHandle.yRange = new Vector2(-360, 360);
                angularLimitHandle.xMotion = 0;
                angularLimitHandle.zMotion = 0;
                orientationCorrection = new Vector3(0, 90, 0);
                transformController.initialOffset = TransformUtils.GetInspectorRotation(transformController.transform).y;
                break;
            case 6:
                angularLimitHandle.zMin = transformController.LockMin;
                angularLimitHandle.zMax = transformController.LockMax;
                angularLimitHandle.zRange = new Vector2(-360, 360);
                angularLimitHandle.xMotion = 0;
                angularLimitHandle.yMotion = 0;
                orientationCorrection = new Vector3(0, 180, 90);
                transformController.initialOffset = TransformUtils.GetInspectorRotation(transformController.transform).z;
                break;
            default:
                // this is just for rotation (hinge) joints, if not one of these then return
                return;
        }

        // set the handle matrix to match the object's position/rotation with a uniform scale
        
        Matrix4x4 handleMatrix = Matrix4x4.TRS(
            transformController.transform.position,
            transformController.transform.parent.gameObject.transform.rotation * Quaternion.Euler(orientationCorrection),
            Vector3.one
        );

        EditorGUI.BeginChangeCheck();

        using (new Handles.DrawingScope(handleMatrix))
        {
            // maintain a constant screen-space size for the handle's radius based on the origin of the handle matrix
            // add 30% to avoid the handle getting lost behind the transform
            angularLimitHandle.radius = HandleUtility.GetHandleSize(Vector3.zero) * 1.3f;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            angularLimitHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // undone/redo
                Undo.RecordObject(transformController, "Change transform lock angles");

                // update JointController script
                float[] validated = new float[2] { 0, 0 };
                switch (transformController.FreeDOF)
                {
                    case 4:
                        validated = LockCheckAngle(angularLimitHandle.xMin, angularLimitHandle.xMax);
                        break;
                    case 5:
                        validated = LockCheckAngle(angularLimitHandle.yMin, angularLimitHandle.yMax);
                        break;
                    case 6:
                        validated = LockCheckAngle(angularLimitHandle.zMin, angularLimitHandle.zMax);
                        break;
                    default:
                        // this is just for rotation (hinge) joints, if not one of these then do nothing
                        break;
                }
                transformController.LockMin = validated[0];
                transformController.LockMax = validated[1];
            }
        }
    }

    private float[] LockCheckAngle(float min, float max)
    {
        if (Mathf.Abs(max - min) > 360) return new float[2] { 0, 360 };
        return new float[2] { min, max };
    }

}
