using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector3 aimForwardVector;
    public NetworkBool isJumpPressed;
    public NetworkBool isFireButtonPressed;
    public NetworkBool isGrenadeFireButtonPressed;
    public NetworkBool isRocketLauncherFireButtonPressed;
}
