
using System.Collections;
using System.Collections.Generic;
using MemoryPack;
using Omni;
using Omni.Core;
using Omni.Threading.Tasks;
using UnityEngine;
using static MultiPlayerOmni;
using static Omni.Core.NetworkManager;

public class Spawn : ServerBehaviour
{
    [SerializeField]
    private List<NetworkIdentity> entityList = new();
    private readonly Dictionary<int, Car> entityListDic = new();
    NetworkGroup group;
    [SerializeField]
    private Vector3 inst1, inst2;
    protected override void OnStart()
    {
        group = Matchmaking.Server.AddGroup("newGroup");
    }
    protected override void OnServerPeerDisconnected(NetworkPeer peer, Phase phase)
    {
        if (phase == Phase.Begin)
        {
            DestroyMyCar(peer);
        }
    }
    [Server(1)]
    void RpcLog(DataBuffer buffer, NetworkPeer peer)
    {
        int car = buffer.Read<int>();
        peer.Data["typeCar"] = car;
        buffer.SeekToBegin();
        Remote.Invoke(1, peer, buffer, Target.Self);
    }
    [Server(2)]
    void RpcInstantiateMycar(DataBuffer buffer, NetworkPeer peer)
    {
        Matchmaking.Server.JoinGroup(group, peer);
        buffer.WriteAsBinary(entityListDic);
        Remote.Invoke(2, peer, buffer, Target.Self);
        InstantiateMyCar(peer);
    }

    [Server(3)]
    void RpcInLobby(DataBuffer buffer, NetworkPeer peer)
    {
        Matchmaking.Server.LeaveGroup(group, peer);
        DestroyMyCar(peer);
        Remote.Invoke(3, peer, target: Target.Self);
    }

    [Server(4)]
    void RpcReposition(DataBuffer buffer, NetworkPeer peer)
    {
        InstantiateMyCar(peer);
    }
    bool DestroyMyCar(NetworkPeer peer)
    {
        if (peer.Data.ContainsKey("myCar"))
        {
            var oldCar = peer.Data.Get<NetworkIdentity>("myCar");
            if (oldCar == null) return false;
            entityList.Remove(oldCar);
            entityListDic.Remove(oldCar.Owner.Id);
            oldCar.Destroy();
            return true;
        }
        return false;
    }

    void InstantiateMyCar(NetworkPeer peer)
    {
        int car = peer.Data.Get<int>("typeCar");
        DestroyMyCar(peer);
        float t = Random.Range(0f, 1f);
        Vector3 spawnPosition = Vector3.Lerp(inst1, inst2, t);
        GetPrefab(car).transform.position = spawnPosition;
        NetworkIdentity identitycar = GetPrefab(car).Spawn(peer, Target.GroupMembers);

        var myCar = identitycar.Get<MultiPlayerOmni>();
        myCar.SetPositionInitial(spawnPosition);
        peer.Data["myCar"] = identitycar;
        entityList.Add(identitycar);
        entityListDic.Add(identitycar.Owner.Id, new Car { IdentityId = identitycar.IdentityId, peerId = identitycar.Owner.Id, car = car });
    }

}

[MemoryPackable]
public partial struct Car
{
    public int IdentityId, peerId, car;
}