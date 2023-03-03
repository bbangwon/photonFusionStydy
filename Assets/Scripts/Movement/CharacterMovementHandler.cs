using Fusion;
using UnityEngine;

public class CharacterMovementHandler : NetworkBehaviour
{
    Vector2 viewInput;

    float cameraRotationX = 0;

    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    Camera localCamera;

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        localCamera = GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        localCamera.gameObject.SetActive(Object.HasInputAuthority);
    }

    // Update is called once per frame
    void Update()
    {
        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        localCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData NetworkInputData))
        {
            networkCharacterControllerPrototypeCustom.Rotate(NetworkInputData.rotationInput);

            Vector3 moveDirection = transform.forward * NetworkInputData.movementInput.y + transform.right * NetworkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerPrototypeCustom.Move(moveDirection);

            //Jump
            if(NetworkInputData.isJumpPressed)
            {
                networkCharacterControllerPrototypeCustom.Jump();
            }
        }
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput; 
    }
}
