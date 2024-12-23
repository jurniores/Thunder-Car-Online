using System.Collections;
using System.Collections.Generic;
using Omni.Core;
using Omni.Threading.Tasks;
using UnityEngine;
using static Spawn;

public class SpawnClient : ClientBehaviour
{

    [SerializeField]
    private GameObject options;

    public void SetCar(int car)
    {
        Local.Invoke(1, car);
    }
    public void InstantiateMyCar()
    {
        Local.Invoke(2);
    }
    public void Reposition()
    {
        Local.Invoke(4);
    }
    public void BackLobby()
    {
        Local.Invoke(3);
    }

    [Client(1)]
    async void RpcRecieveCar(DataBuffer buffer)
    {
        int car = buffer.Read<int>();
        print("Recebi o client o carro" + car);
        NetworkManager.LocalPeer.Data["typeCar"] = car;
        await NetworkManager.LoadSceneAsync(1).ToUniTask();
        InstantiateMyCar();
    }

    [Client(2)]
    void RpcInitGame(DataBuffer buffer)
    {
        var entityList = buffer.ReadAsBinary<Dictionary<int, Car>>();

        foreach (var entity in entityList.Values)
        {
            NetworkManager.GetPrefab(entity.car).SpawnOnClient(entity.peerId, entity.IdentityId);
        }
    }

    [Client(3)]
    async void RpcInLobby(DataBuffer buffer)
    {
        foreach (var key in new List<int>(NetworkManager.Client.Identities.Keys))
        {
            int identityId = NetworkManager.Client.Identities[key].IdentityId;
            NetworkHelper.Destroy(identityId, false);
        }
        await NetworkManager.LoadSceneAsync(0).ToUniTask();
    }

}
