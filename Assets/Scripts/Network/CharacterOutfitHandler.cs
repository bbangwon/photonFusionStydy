using Fusion;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOutfitHandler : NetworkBehaviour
{
    [Header("Character parts")]
    public GameObject playerHead;
    public GameObject playerBody;
    public GameObject playerRightArm;
    public GameObject playerLeftArm;

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
        NetworkOutfit newOutfit = networkOutfit;

        newOutfit.headPrefabID = (byte)Random.Range(0, headPrefabs.Count);
        newOutfit.bodyPrefabID = (byte)Random.Range(0, bodyPrefabs.Count);
        newOutfit.leftArmPrefabID = (byte)Random.Range(0, leftArmPrefabs.Count);
        newOutfit.rightArmPrefabID = (byte)Random.Range(0, rightArmPrefabs.Count);

        if(Object.HasInputAuthority)
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
}
