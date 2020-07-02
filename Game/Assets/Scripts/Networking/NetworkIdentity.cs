using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Networking
{
    public class NetworkIdentity : MonoBehaviour
    {
        [Header("HelpFul Values")]
        [SerializeField]
        [GreyOut]
        private string id;
        [SerializeField]
        [GreyOut]
        private bool isControlling;

        private SocketIOComponent socket;

        public void Awake()
        {
            isControlling = false;

        }

        public void SetCotrollerID(string id)
        {
            this.id = id;
            isControlling = (NetworkClient.ClientID) == id;
        }

        public void SetSocketReference(SocketIOComponent socket)
        {
            this.socket = socket;
        }

        public string GetId()
        {
            return id;
        }

        public SocketIOComponent GetSocket()
        {
            return socket;
        }

        public bool IsControlling() {
            return this.isControlling;
        }
    }
}


