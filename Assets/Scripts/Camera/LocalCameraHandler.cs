using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    public Transform cameraAnchorPoint; 

    Vector2 viewInput;

    float cameraRotationX = 0;
    float cameraRotationY = 0;

    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;

    public Camera localCamera;

    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        networkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraRotationX = GameManager.instance.cameraViewRotation.x;
        cameraRotationY = GameManager.instance.cameraViewRotation.y;
    }

    private void LateUpdate()
    {
        if (cameraAnchorPoint == null)
            return;

        if(!localCamera.enabled) 
            return;

        localCamera.transform.position = cameraAnchorPoint.position;

        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterControllerPrototypeCustom.rotationSpeed;

        localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }

    private void OnDestroy()
    {
        if(cameraRotationX != 0 && cameraRotationY != 0)
        {
            GameManager.instance.cameraViewRotation.x = cameraRotationX;
            GameManager.instance.cameraViewRotation.y = cameraRotationY;
        }
    }
}
