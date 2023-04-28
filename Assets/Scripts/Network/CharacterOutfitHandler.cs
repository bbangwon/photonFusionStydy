using Fusion;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterOutfitHandler : NetworkBehaviour
{
    [Header("Character parts")]
    public GameObject playerHead;
    public GameObject playerBody;
    public GameObject playerRightArm;
    public GameObject playerLeftArm;

    [Header("Ready UI")]
    public Image readyCheckboxImage;

    [Header("Animation")]
    public Animator characterAnimator;

    List<GameObject> headPrefabs = new List<GameObject>();
    List<GameObject> bodyPrefabs = new List<GameObject>();
    List<GameObject> leftArmPrefabs = new List<GameObject>();
    List<GameObject> rightArmPrefabs = new List<GameObject>();

    struct NetworkOutfit : INetworkStruct
    {
        public byte headPrefabID;
        public byte bodyPrefabID;
        public byte leftArmPrefabID;
        public byte rightArmPrefabID;
    }

    [Networked(OnChanged = nameof(OnOutfitChanged))]
    NetworkOutfit networkOutfit { get; set; }

    [Networked(OnChanged = nameof(OnIsDoneWithCharacterSelectionChanged))]
    public NetworkBool isDoneWithCharacterSelection { get; set; }

    private void Awake()
    {
        headPrefabs = Resources.LoadAll<GameObject>("Bodyparts/Heads/").ToList();
        headPrefabs = headPrefabs.OrderBy(n => n.name).ToList();

        bodyPrefabs = Resources.LoadAll<GameObject>("Bodyparts/Bodies/").ToList();
        bodyPrefabs = bodyPrefabs.OrderBy(n => n.name).ToList();

        leftArmPrefabs = Resources.LoadAll<GameObject>("Bodyparts/LeftArms/").ToList();
        leftArmPrefabs = leftArmPrefabs.OrderBy(n => n.name).ToList();

        rightArmPrefabs = Resources.LoadAll<GameObject>("Bodyparts/RightArms/").ToList();
        rightArmPrefabs = rightArmPrefabs.OrderBy(n => n.name).ToList();
    }

    void Start()
    {
        characterAnimator.SetLayerWeight(1, 0.0f);

        if (SceneManager.GetActiveScene().name != "Ready")
            return;

        NetworkOutfit newOutfit = networkOutfit;

        newOutfit.headPrefabID = (byte)Random.Range(0, headPrefabs.Count);
        newOutfit.bodyPrefabID = (byte)Random.Range(0, bodyPrefabs.Count);
        newOutfit.leftArmPrefabID = (byte)Random.Range(0, leftArmPrefabs.Count);
        newOutfit.rightArmPrefabID = (byte)Random.Range(0, rightArmPrefabs.Count);

        characterAnimator.SetLayerWeight(1, 1.0f);

        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    GameObject ReplaceBodyPart(GameObject currentBodyPart, GameObject prefabNweBodyPart)
    {
        GameObject newPart = Instantiate(prefabNweBodyPart, currentBodyPart.transform.position, currentBodyPart.transform.rotation);
        newPart.transform.parent = currentBodyPart.transform.parent;
        Utils.SetRenderLayerInChildren(newPart.transform, currentBodyPart.layer);
        Destroy(currentBodyPart);

        return newPart;
    }

    void ReplaceBodyParts()
    {
        playerHead = ReplaceBodyPart(playerHead, headPrefabs[networkOutfit.headPrefabID]);
        playerBody = ReplaceBodyPart(playerBody, bodyPrefabs[networkOutfit.bodyPrefabID]);
        playerLeftArm = ReplaceBodyPart(playerLeftArm, leftArmPrefabs[networkOutfit.leftArmPrefabID]);
        playerRightArm = ReplaceBodyPart(playerRightArm, rightArmPrefabs[networkOutfit.rightArmPrefabID]);

        GetComponent<HPHandler>().ResetMeshRenderers();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestOutfitChange(NetworkOutfit newNetworkOutfit, RpcInfo info = default)
    {
        Debug.Log($"Received RPC_RequestOutfitChange for player {transform.name}, HeadID {newNetworkOutfit.headPrefabID}");

        networkOutfit = newNetworkOutfit;
    }

    static void OnOutfitChanged(Changed<CharacterOutfitHandler> changed)
    {
        changed.Behaviour.OnOutfitChanged();
    }

    private void OnOutfitChanged()
    {
        ReplaceBodyParts();
    }

    public void OnCycleHead()
    {
        NetworkOutfit newOutfit = networkOutfit;

        newOutfit.headPrefabID++;

        if(newOutfit.headPrefabID > headPrefabs.Count -1 )
            newOutfit.headPrefabID = 0;

        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnCycleBody()
    {
        NetworkOutfit newOutfit = networkOutfit;

        newOutfit.bodyPrefabID++;

        if (newOutfit.bodyPrefabID > bodyPrefabs.Count - 1)
            newOutfit.bodyPrefabID = 0;

        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnCycleLeftArm()
    {
        NetworkOutfit newOutfit = networkOutfit;

        newOutfit.leftArmPrefabID++;

        if (newOutfit.leftArmPrefabID > leftArmPrefabs.Count - 1)
            newOutfit.leftArmPrefabID = 0;

        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnCycleRightArm()
    {
        NetworkOutfit newOutfit = networkOutfit;

        newOutfit.rightArmPrefabID++;

        if (newOutfit.rightArmPrefabID > rightArmPrefabs.Count - 1)
            newOutfit.rightArmPrefabID = 0;

        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnReady(bool isReady)
    {
        if(Object.HasInputAuthority)
        {
            RPC_SetReady(isReady);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default)
    {
        isDoneWithCharacterSelection = isReady;
    }

    static void OnIsDoneWithCharacterSelectionChanged(Changed<CharacterOutfitHandler> changed)
    {
        changed.Behaviour.IsDoneWithCharacterSelectionChanged();
    }

    private void IsDoneWithCharacterSelectionChanged()
    {
        if (SceneManager.GetActiveScene().name != "Ready")
        {
            readyCheckboxImage.gameObject.SetActive(false);
            return;
        }

        if (isDoneWithCharacterSelection)
        {
            characterAnimator.SetTrigger("Ready");
            readyCheckboxImage.gameObject.SetActive(true);
        }
        else readyCheckboxImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {       
        if (Object == null || !Object.HasStateAuthority)
            return;

        Debug.Log($"CharacterOutfitHandler Scene Name : {scene.name}");

        if (scene.name != "Ready")
        {            
            readyCheckboxImage.gameObject.SetActive(false);
        }
    }
}
