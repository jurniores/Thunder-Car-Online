using System.Collections;
using System.Collections.Generic;
using Omni.Core;
using UnityEngine;

public class Spawn : ServerBehaviour
{
    
    // Update is called once per frame
    void Update()
    {

    }

    [Server(1)]
    void RpcLog(DataBuffer buffer, NetworkPeer peer)
    {
        int car = buffer.Read<int>();
        print("Recebi o servidor o carro" + car);
        peer.Data["typeCar"] = car;
        buffer.SeekToBegin();
        Remote.Invoke(1, peer, buffer, Target.Self);
    }
    [Server(2)]
    void RpcInstantiate(DataBuffer buffer, NetworkPeer peer)
    {
        NetworkManager.GetPrefab(0).Spawn(peer);
    }
}
