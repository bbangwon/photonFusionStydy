using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadyUIHandler : NetworkBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI buttonReadyText;
    public TextMeshProUGUI countDownText;

    bool isReady = false;

    Vector3 desiredCameraPosition = new Vector3 (0, 5, 20);

    TickTimer countDownTickTimer = TickTimer.None;   

    [Networked(OnChanged = nameof(OnCountdownChnaged))]
    byte countDown { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        countDownText.text = "";
    }

    private void Update()
    {
        if(NetworkPlayer.Local != null)
        {
            float lerpSpeed = 0.5f;

            if (!isReady)
            {
                desiredCameraPosition = new Vector3(NetworkPlayer.Local.transform.position.x, 0.95f, 5);
                lerpSpeed = 7;
            }
            else
            {
                desiredCameraPosition = new Vector3(14, 3, 30);
                lerpSpeed = 0.5f;
            }

            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, desiredCameraPosition, Time.deltaTime * lerpSpeed);
        }


        if (countDownTickTimer.Expired(Runner))
        {
            StartGame();
            countDownTickTimer = TickTimer.None;
        }
        else if(countDownTickTimer.IsRunning)
        {
            countDown = (byte)countDownTickTimer.RemainingTime(Runner);
        }
    }

    void StartGame()
    {
        Runner.SessionInfo.IsOpen = false;

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject gameObjectToTransfer in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObjectToTransfer);

            if(!gameObjectToTransfer.GetComponent<CharacterOutfitHandler>().isDoneWithCharacterSelection) 
                Runner.Disconnect(gameObjectToTransfer.GetComponent<NetworkObject>().InputAuthority);
        }

        Runner.SetActiveScene("World1");
    }

    public void OnChangeCharacterHead()
    {
        if (isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleHead();
    }

    public void OnChangeCharacterBody()
    {
        if(isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleBody();
    }

    public void OnChangeCharacterLeftArm()
    {
        if (isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleLeftArm();
    }

    public void OnChangeCharacterRightArm()
    {
        if(isReady) 
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleRightArm();
    }

    public void OnReady()
    {
        if (isReady)
            isReady = false;
        else
            isReady = true;

        if (isReady)
            buttonReadyText.text = "NOT READY";
        else
            buttonReadyText.text = "READY";

        if(Runner.IsServer)
        {
            if (isReady)
                countDownTickTimer = TickTimer.CreateFromSeconds(Runner, 10);
            else
            {
                countDownTickTimer = TickTimer.None;
                countDown = 0;
            }
        }

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnReady(isReady);
    }

    static void OnCountdownChnaged(Changed<ReadyUIHandler> changed)
    {
        changed.Behaviour.OnCountdownChanged();
    }

    private void OnCountdownChanged()
    {
        if (countDown == 0)
            countDownText.text = $"";
        else countDownText.text = $"Game starts in {countDown}";
    }
}
