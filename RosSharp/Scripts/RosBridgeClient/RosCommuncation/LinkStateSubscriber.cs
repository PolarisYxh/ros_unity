using System.Collections.Generic;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using UnityEngine;
namespace RosSharp.RosBridgeClient
{
    public class LinkStateSubscriber : UnitySubscriber<MessageTypes.Gazebo.LinkStates>
    {
        public List<string> LinkNames;
        public List<LinkStateWriter> LinkStateWriters;

        protected override void ReceiveMessage(MessageTypes.Gazebo.LinkStates message)
        {
            int index;
            for (int i = 0; i < message.name.Length; i++)
            {
                index = LinkNames.IndexOf(message.name[i]);
                if (index != -1)
                {
                    UnityEngine.Vector3 pos=new UnityEngine.Vector3(-(float)message.pose[i].position.y,(float)message.pose[i].position.z,(float)message.pose[i].position.x);
                    UnityEngine.Quaternion rot=new UnityEngine.Quaternion(-(float)message.pose[i].orientation.y,(float)message.pose[i].orientation.z,(float)message.pose[i].orientation.z,(float)message.pose[i].orientation.x);
                    UnityEngine.Vector3 lin=new UnityEngine.Vector3(-(float)message.twist[i].linear.y,(float)message.twist[i].linear.z,(float)message.twist[i].linear.x);
                    UnityEngine.Vector3 ang=new UnityEngine.Vector3(-(float)message.twist[i].angular.y,(float)message.twist[i].angular.z,(float)message.twist[i].angular.x);
                    LinkStateWriters[index].Write(pos,rot,lin,ang);
                }
            }
        }
    }
}