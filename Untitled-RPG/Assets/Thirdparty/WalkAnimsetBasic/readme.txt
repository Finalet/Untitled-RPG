Walk Animset Basic

------------------

This is a set of 21 motion capture animations to build a TPP Walking movement.

------------------
Contents:

 - 21 mocap animations of directional starting, loops and stopping walking.
 - a model of a dummy
 - a MECANIM example graph of how to blend animations

------------------

The MECANIM graph:

The graph is controlled by 4 variables:

1) InputMagnitude
	This is a float that should be controlled by Input Axis Vector Magnitude (so for example, how far is the joystick bent). It controls the transitions between Idling and Walking.

2) InputDirection
	This is a float that should be controlled by the angle between character's Forward Axis and the Input Axis Vector. So for example, if the joystick is bent right, the angle will be 90 degrees, if back, the angle will be 180 degrees and so on.

3) IsRightLegUp
	This is a bool that should be controlled by which foot is currently higher in a walk cycle, so the controller knows which animation it should play while stopping. Easiest way to get this info is checking which foot bone has a bigger Global Transform Y at the moment. So, if right foot is higher - we change IsRightLegUp to true.

4) RotationDirection
	This is a float that should be controlled by the angle between character's Forward Axis and the desired character's rotation. So basically, it controls turning in place. So, when the character is idling - you can control his turning by animation. If the character is walking - you need to control his rotation by SmoothLookAt.