using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isFireButtonPressed = false;
    bool isGrenadeButtonPressed = false;
    bool isRocketLauncherFireButtonPressed = false;

    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;

    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!characterMovementHandler.Object.HasInputAuthority)
            return;

        if (SceneManager.GetActiveScene().name == "Ready")
            return;

        //Veiw Input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1;

        //Move Input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if(Input.GetButtonDown("Jump"))
            isJumpButtonPressed = true;

        if(Input.GetButtonDown("Fire1"))
            isFireButtonPressed = true;

        if (Input.GetButtonDown("Fire2"))
            isRocketLauncherFireButtonPressed = true;

        if (Input.GetKeyDown(KeyCode.G))
            isGrenadeButtonPressed = true;

        localCameraHandler.SetViewInputVector(viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        networkInputData.aimForwardVector = localCameraHandler.transform.forward;
        networkInputData.movementInput = moveInputVector;
        networkInputData.isJumpPressed = isJumpButtonPressed;
        networkInputData.isFireButtonPressed = isFireButtonPressed;
        networkInputData.isRocketLauncherFireButtonPressed = isRocketLauncherFireButtonPressed;        
        networkInputData.isGrenadeFireButtonPressed = isGrenadeButtonPressed;

        isJumpButtonPressed = false;
        isFireButtonPressed = false;
        isGrenadeButtonPressed = false;
        isRocketLauncherFireButtonPressed = false;

        return networkInputData;
    }


}
