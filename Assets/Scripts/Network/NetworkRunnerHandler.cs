using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;

    NetworkRunner networkRunner;

    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();

        if (networkRunnerInScene != null)
            networkRunner = networkRunnerInScene;
    }

    void Start()
    {
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network runner";

            if(SceneManager.GetActiveScene().name != "MainMenu")
            {
                var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient,
                    "TestSession",
                    GameManager.instance.GetConnectionToken(),
                    NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null);
            }

            Debug.Log($"Server NetworkRunner started.");
        }
    }

    public void StartHostMigration(HostMigrationToken hostMigrationToken)
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network runner - Migrated";

        var clientTask = InitializeNetworkRunnerHostMigration(networkRunner, hostMigrationToken);

        Debug.Log($"Host migration started");
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        sceneManager ??= runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        return sceneManager;
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, 
        GameMode gameMode, 
        string sessionName,
        byte[] connectionToken,
        NetAddress address, 
        SceneRef scene, 
        Action<NetworkRunner> initialized)
    {
        var sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            Initialized = initialized,
            SceneManager = sceneManager,
            ConnectionToken = connectionToken
        });
    }

    protected virtual Task InitializeNetworkRunnerHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        var sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs
        {
            SceneManager = sceneManager,
            HostMigrationToken = hostMigrationToken,
            HostMigrationResume = HostMigrationResume,
            ConnectionToken = GameManager.instance.GetConnectionToken()
        });
    }

    void HostMigrationResume(NetworkRunner runner)
    {
        Debug.Log($"HostMigrationResume started");

        foreach (var resumeNetworkObject in runner.GetResumeSnapshotNetworkObjects())
        {
            if(resumeNetworkObject.TryGetBehaviour<NetworkCharacterControllerPrototypeCustom>(out var characterController))
            {
                runner.Spawn(resumeNetworkObject, position: characterController.ReadPosition(),
                    characterController.ReadRotation(),
                    onBeforeSpawned: (runner, newNetworkObject) =>
                    {
                        newNetworkObject.CopyStateFrom(resumeNetworkObject);

                        if(resumeNetworkObject.TryGetBehaviour<HPHandler>(out HPHandler oldHPHandler))
                        {
                            HPHandler newHPHandler = newNetworkObject.GetComponent<HPHandler>();
                            newHPHandler.CopyStateFrom(oldHPHandler);

                            newHPHandler.skipSettingStartValues = true;
                        }

                        if(resumeNetworkObject.TryGetBehaviour<NetworkPlayer>(out var oldNetworkPlayer))
                        {
                            FindObjectOfType<Spawner>().SetConnectionTokenMapping(oldNetworkPlayer.token, newNetworkObject.GetComponent<NetworkPlayer>());
                        }
                    });
            }
        }

        StartCoroutine(CleanUpHostMigrationCO());

        Debug.Log($"HostMigrationResume completed");
    }

    IEnumerator CleanUpHostMigrationCO()
    {
        yield return new WaitForSeconds(5.0f);

        FindObjectOfType<Spawner>().OnHostMigrationCleanUp();
    }

    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }

    private async Task JoinLobby()
    {
        Debug.Log("JoinLobby started");
        string lobbyID = "OurLobbyID";

        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if(!result.Ok)
        {
            Debug.LogError($"Unable to join lobby {lobbyID}");
        }
        else
        {
            Debug.Log("JoinLobby ok");
        }
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log($"Create session {sessionName} scene {sceneName} build Index {SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")}");

        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Host,
            sessionName, 
            GameManager.instance.GetConnectionToken(),
            NetAddress.Any(), 
            SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}"), 
            null);
    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        Debug.Log($"Join session {sessionInfo.Name}");

        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Client,
            sessionInfo.Name,
            GameManager.instance.GetConnectionToken(),
            NetAddress.Any(),
            SceneManager.GetActiveScene().buildIndex,
            null);
    }
}
