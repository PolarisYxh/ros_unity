using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(HingeJoint))]
    public class hingeMotor : MonoBehaviour
    {
        public float MaxVelocity;

        private HingeJoint _hingeJoint;
        private JointMotor jointMotor;
        private float targetVelocity;

        private void Start()
        {
            _hingeJoint = GetComponent<HingeJoint>();
            _hingeJoint.useMotor = true;
        }

        private void Update()
        {
            jointMotor = _hingeJoint.motor;
            jointMotor.targetVelocity = 50;
            jointMotor.force = 400;
            _hingeJoint.motor = jointMotor;
        }
    }
}