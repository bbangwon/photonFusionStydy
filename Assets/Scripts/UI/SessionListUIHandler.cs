using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SessionListUIHandler : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public GameObject sessionItemListPrefab;
    public VerticalLayoutGroup verticalLayoutGroup;

    private void Awake()
    {
        ClearList();
    }

    public void ClearList()
    {
        foreach(Transform child in verticalLayoutGroup.transform)
            Destroy(child.gameObject);

        statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        SessionInfoListUIItem addedSessionInfoUIItem = Instantiate(sessionItemListPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();

        addedSessionInfoUIItem.SetInformation(sessionInfo);

        addedSessionInfoUIItem.OnJoinSession += AddedSessionInfoUIItem_OnJoinSession;
    }

    private void AddedSessionInfoUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(sessionInfo);

        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        mainMenuUIHandler.OnJoiningServer();
    }

    public void OnNoSessionsFound()
    {
        ClearList();

        statusText.text = "No game session found";
        statusText.gameObject.SetActive(true);
    }

    public void OnLookingForGameSessions()
    {
        ClearList();

        statusText.text = "Looking for game sessions";
        statusText.gameObject.SetActive(true);
    }
}
