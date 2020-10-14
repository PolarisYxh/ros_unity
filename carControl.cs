using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class carControl : MonoBehaviour
{
    //炮塔炮管轮子履带
    public List<HingeJoint> leftWheels;
    public List<HingeJoint> rightWheels;

    //马力/最大马力
    private float motor = 0;
    public float maxMotorTorque;
    //制动/最大制动
    private float brakeTorque = 0;
    public float maxBrakeTorque = 100;
    //转向角/最大转向角
    private float steering = 0;
    public float maxSteeringAngle;

    //玩家控制
    public void PlayerCtrl()
    {
        if(Input.GetKey(KeyCode.W))
        {
            for(int i=0;i<leftWheels.Count;i++)
            {
                JointMotor m=leftWheels[i].motor;
                m.targetVelocity=-90;
                m.force=float.PositiveInfinity;
                leftWheels[i].motor=m;
                leftWheels[i].useMotor=true;
                m=rightWheels[i].motor;
                m.targetVelocity=-90;
                m.force=float.PositiveInfinity;
                rightWheels[i].motor=m;
                rightWheels[i].useMotor=true;
            }
        }
        else if(Input.GetKey(KeyCode.S))
        {
            for(int i=0;i<leftWheels.Count;i++)
            {
                JointMotor m = leftWheels[i].motor;
                m.targetVelocity=0;
                m.force=float.PositiveInfinity;
                leftWheels[i].motor=m;
                leftWheels[i].useMotor=true;
                m=rightWheels[i].motor;
                m.targetVelocity=0;
                m.force=float.PositiveInfinity;
                rightWheels[i].motor=m;
                rightWheels[i].useMotor=true;
            }
        }
    }


    //开始时执行
    void Start()
    {

    }

    //每帧执行一次
    void Update()
    {
        //玩家控制操控
        PlayerCtrl();

    }

    //轮子旋转
    public void WheelsRotation(WheelCollider collider)
    {

    }

}