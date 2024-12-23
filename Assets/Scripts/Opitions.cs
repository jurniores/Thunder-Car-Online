using System.Collections;
using System.Collections.Generic;
using Omni.Core;
using UnityEngine;

public class Opitions : ServiceBehaviour
{
    [SerializeField]
    private GameObject panel;
    private SpawnClient spawnClient;
    public MultiPlayerOmni car;
    protected override void OnStart()
    {
        spawnClient = NetworkService.Get<SpawnClient>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Open();
    }
    public void Open()
    {
        panel.SetActive(!panel.activeSelf);
    }

    public void Reposition()
    {
        car.sendTick = false;
        spawnClient.Reposition();
    }
    public void Lobby()
    {
        car.sendTick = false;
        spawnClient.BackLobby();
    }


}
