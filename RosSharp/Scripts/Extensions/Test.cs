using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour
{

    private float OpenSpeed = 30;
    private string option = "";
    private bool isOpen = false;
	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{
	    if (!isOpen)
	    {
	        option = "打开";
	    }
	    else
	    {
	        option = "关闭";
	    }

	}

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 30), option))
        {
            JointMotor motor = new JointMotor();
            motor.targetVelocity = OpenSpeed;
            motor.force = 3;

            GetComponent<HingeJoint>().motor = motor;
            OpenSpeed = -OpenSpeed;
            isOpen = true;
        }
    }
}
