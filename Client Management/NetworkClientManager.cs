using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;

public class NetworkClientManager : MonoBehaviour
{
    [SerializeField]
    private XmlUnityServer server;

    private void Awake()
    {
        Clients.clients = new List<IClient>();
        TeamIDs.teamIDs = new List<ushort>();
    }

    private void Start()
    {
        server.Server.ClientManager.ClientConnected += ClientConnected;
        server.Server.ClientManager.ClientDisconnected += ClientDisconnected;
    }

    private void ClientConnected(object sender, ClientConnectedEventArgs e)
    {
        Clients.clients.Add(e.Client);
        e.Client.MessageReceived += CommandReceiver.MessageReceived;
    }

    private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        Clients.clients.Remove(e.Client);
    }
}
