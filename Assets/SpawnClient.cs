using System.Collections;
using System.Collections.Generic;
using Omni.Core;
using Omni.Threading.Tasks;
using UnityEngine;

public class SpawnClient : ClientBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I)){
            InstantiateMyCar();
        }
    }

    public void SetCar()
    {
        Local.Invoke(1, NetworkService.Get<Carrossel>().currentIndex);
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
    public void InstantiateMyCar(){
        Local.Invoke(2);
    }

}
