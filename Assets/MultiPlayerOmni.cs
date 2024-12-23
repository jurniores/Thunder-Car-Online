using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NWH.VehiclePhysics2;
using NWH.WheelController3D;
using Omni.Core;
using Unity.VisualScripting;
using UnityEngine;
using static NWH.VehiclePhysics2.VehicleController;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(VehicleController))]
public partial class MultiPlayerOmni : NetworkBehaviour
{
    public struct PosIntitial
    {
        public Vector3 pos;
        public Quaternion rot;

    }
    [NetworkVariable]
    private PosIntitial m_InititalPoisition;
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
    private Quaternion inititalRotation;
    public bool sendTick = false;
    protected override void OnStart()
    {

        //var colliders = GetComponentsInChildren<Collider>().ToList();
        var listentes = GetComponentsInChildren<AudioListener>().ToList();
        if (IsLocalPlayer) Camera.main.GetComponent<AudioListener>().enabled = false;
        //colliders.ForEach(c => c.enabled = false);
        listentes.ForEach(a => a.enabled = false);
        _networkIdentity = GetComponent<NetworkIdentity>();
        inititalRotation = transform.rotation;
        _vehicleController = GetComponent<VehicleController>();

        if (IsServer)
        {
            DefaultNetworkVariableOptions = new()
            {
                Target = Target.GroupMembers
            };
        }
        //if (!IsLocalPlayer) _vehicleController.enabled = false;
        if (IsLocalPlayer) NetworkService.Get<Opitions>().car = this;
        else if (IsClient)
        {
            _vehicleController.SetMultiplayerState(lastState);
        }

        _vehicleController.onVehicleInitialized.AddListener(() =>
        {
            _vehicleInitialized = true;
            _vehicleController.MultiplayerIsRemote = !IsLocalPlayer;
        });
        sendTick = true;
    }
    protected override void OnStartRemotePlayer()
    {

    }
    public void SetPositionInitial(Vector3 pos)
    {
        m_InititalPoisition = new PosIntitial { pos = pos, rot = transform.rotation };
    }
    private void Update()
    {
        if (!IsLocalPlayer) return;
        var state = _vehicleController.GetMultiplayerState();
        if (!lastState.Equals(state) && sendTick)
        {
            lastState = state;
            using var buffer = Rent();
            buffer.WriteAsBinary(state);
            Local.Invoke(1, buffer);
        }

        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     transform.rotation = inititalRotation;
        // }
    }

    protected override void OnDestroy()
    {
    }

    [Server(1)]
    private void CmdMultiplayerState(DataBuffer buffer)
    {
        Remote.Invoke(1, buffer, Target.GroupMembersExceptSelf);
    }
    [Server(2)]
    private void RpcPosition(DataBuffer buffer)
    {
        m_InititalPoisition.pos = buffer.Read<HalfVector3>();
        m_InititalPoisition.rot = buffer.ReadPackedQuaternion();
        buffer.SeekToBegin();
        Remote.Invoke(2, buffer, Target.GroupMembersExceptSelf, DeliveryMode.Unreliable);
    }
    //Client
    [Client(2)]
    private void RpcClientPosition(DataBuffer buffer)
    {
        Vector3 position = buffer.Read<HalfVector3>();
        Quaternion quaternion = buffer.ReadPackedQuaternion();
        position.y = transform.position.y;
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
        if (IsLocalPlayer && sendTick)
        {
            if (Vector3.Distance(lastPosition, transform.position) > 0.2f)
            {
                lastPosition = transform.position;
                using var buffer = Rent();
                buffer.Write((HalfVector3)transform.position);
                buffer.WritePacked(transform.rotation);
                Local.Invoke(2, buffer, DeliveryMode.Unreliable);
            }

        }
    }

    partial void OnInititalPoisitionChanged(PosIntitial prevInititalPoisition, PosIntitial nextInititalPoisition, bool isWriting)
    {
        if (!isWriting)
        {
            transform.DOMove(nextInititalPoisition.pos, vel);
            transform.DORotateQuaternion(nextInititalPoisition.rot, vel);
        }
    }

}

