using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
    public class WhoActivateMe : MonoBehaviour
    {   
        [SerializeField]
        [GreyOut]
        private string whoActivatedMe;

        public void SetActivator(string id)
        {
            whoActivatedMe = id;
        }

        public string GetActivator()
        {
            return whoActivatedMe;
        }
    }
}

