using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NWH.VehiclePhysics2;
using Omni.Core;
using Unity.VisualScripting;
using UnityEngine;
using static NWH.VehiclePhysics2.VehicleController;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(VehicleController))]
public class MultiPlayerOmni : NetworkBehaviour
{

    [SerializeField]
    private List<MonoBehaviour> components;
    float steering;
    private Vector3 lastPosition;
    [SerializeField]
    MultiplayerState lastState;
    private NetworkIdentity _networkIdentity;
    private VehicleController _vehicleController;
    private bool _vehicleInitialized = false;
    [SerializeField]
    private float vel, velRot;
    protected override void OnStart()
    {
        var colliders = GetComponentsInChildren<Collider>().ToList();
        var listentes = GetComponentsInChildren<AudioListener>().ToList();
        Camera.main.GetComponent<AudioListener>().enabled = false;
        colliders.ForEach(c => c.enabled = false);
        listentes.ForEach(a => a.enabled = false);
        _networkIdentity = GetComponent<NetworkIdentity>();

        _vehicleController = GetComponent<VehicleController>();

        //if (!IsLocalPlayer) _vehicleController.enabled = false;

        _vehicleController.onVehicleInitialized.AddListener(() =>
        {
            _vehicleInitialized = true;
            _vehicleController.MultiplayerIsRemote = !IsLocalPlayer;
        });
    }
    protected override void OnStartRemotePlayer()
    {

        print("Bloquei no servidor");
    }


    private void Update()
    {
        if (!IsLocalPlayer) return;
        var state = _vehicleController.GetMultiplayerState();
        if (!lastState.Equals(state))
        {
            lastState = state;
            using var buffer = Rent();
            buffer.WriteAsBinary(state);
            Local.Invoke(1, buffer);
        }


    }


    [Server(1)]
    private void CmdMultiplayerState(DataBuffer buffer)
    {
        print("Recebi steering");
        Remote.Invoke(1, buffer, Target.AllExceptSelf);
    }
    [Server(2)]
    private void RpcPosition(DataBuffer buffer)
    {
        print("Recebi position");
        Remote.Invoke(2, buffer, Target.AllExceptSelf, DeliveryMode.Unreliable);
    }
    //Client
    [Client(2)]
    private void RpcClientPosition(DataBuffer buffer)
    {
        Vector3 position = buffer.Read<HalfVector3>();
        Quaternion quaternion = buffer.ReadPackedQuaternion();
        transform.DOMove(position, vel);
        transform.DORotateQuaternion(quaternion, velRot);
    }
    [Client(1)]
    private void RpcMultiplayerState(DataBuffer buffer)
    {
        lastState = buffer.ReadAsBinary<MultiplayerState>();
        
        _vehicleController.SetMultiplayerState(lastState);
    }
    public override void OnTick(ITickInfo data)
    {
        if (IsLocalPlayer)
        {
            if (Vector3.Distance(lastPosition,transform.position) > 0.2f)
            {
                lastPosition = transform.position;
                using var buffer = Rent();
                buffer.Write((HalfVector3)transform.position);
                buffer.WritePacked(transform.rotation);
                Local.Invoke(2, buffer, DeliveryMode.Unreliable);
            }

        }
    }
}

