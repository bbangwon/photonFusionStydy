using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public TextMeshProUGUI playerNickNameTM;
    public static NetworkPlayer Local { get; set; }
    public Transform playerModel;

    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName { get; set; }

    bool isPublicJoinMessageSent = false;

    public LocalCameraHandler localCameraHandler;
    public GameObject localUI;

    NetworkInGameMessages networkInGameMessages;

    [Networked]
    public int token { get; set; }

    private void Awake()
    {
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Spawned()
    {
        bool isReadyScene = SceneManager.GetActiveScene().name == "Ready";

        if (Object.HasInputAuthority)
        {
            Local = this;

            if(isReadyScene)
            {
                Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);

                localCameraHandler.gameObject.SetActive(false);

                localUI.SetActive(false);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                if (Camera.main != null)
                    Camera.main.gameObject.SetActive(false);

                localCameraHandler.localCamera.enabled = true;
                localCameraHandler.gameObject.SetActive(true);

                localCameraHandler.transform.parent = null;

                localUI.SetActive(true);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            RPC_SetNickName(GameManager.instance.playerNickName);

            Debug.Log("Spawned local player");
        }
        else
        {
            localCameraHandler.localCamera.enabled = false;
            localCameraHandler.gameObject.SetActive(false);

            localUI.SetActive(false);

            Debug.Log("Spawned remote player");

        }

        Runner.SetPlayerObject(Object.InputAuthority, Object);

        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if(Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
            {
                if(playerLeftNetworkObject == Object)
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(
                        playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), 
                        "left");
            }
        }

        if (player == Object.InputAuthority)
            Runner.Despawn(Object);
    }

    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.nickName}");

        changed.Behaviour.OnNickNameChanged();
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");

        playerNickNameTM.text = nickName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;

        if(!isPublicJoinMessageSent)
        {
            networkInGameMessages.SendInGameRPCMessage(nickName, "joined");
            isPublicJoinMessageSent = true;
        }
    }

    private void OnDestroy()
    {
        if (localCameraHandler != null)
            Destroy(localCameraHandler.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Object == null || !Object.HasStateAuthority)
            return;

        Debug.Log($"{Time.time} OnSceneLoaded: " + scene.name);

        if (scene.name != "Ready")
        {
            if (Object.HasInputAuthority)
                Spawned();

            GetComponent<CharacterMovementHandler>().RequestRespawn();
        }

    }
}
