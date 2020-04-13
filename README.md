An experiment to create a multi axis (R6 + clamps) industrial robot arm. The arm is contructed with chained transforms, not rigid body joints, which I found unreliable and wish to address separately in the future. However, considerable code has been added to support the control of the transform behaviour.

The project demonstrates GUI lock angle adjustments within the Unity editor for the transforms.

Individual manual control by the user via the keyboard.

Programmatic positioning of the arm system, by simple method calls.

Automated tracking of a target point using 4 axes of the arm, whilst avoiding any costly conventional inverse kinematics (the problem is resolved with some simple cosine rule solutions).
 
