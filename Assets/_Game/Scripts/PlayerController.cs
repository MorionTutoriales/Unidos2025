using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty moveAction;     // Vector2 (WASD o stick)
    public InputActionProperty mousePosAction; // Vector2 (posición del mouse en pantalla)

    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Referencias")]
    public Camera mainCamera;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        mousePosAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        mousePosAction.action.Disable();
    }

    void Update()
    {
        Mover();
        ApuntarConMouse();
    }

    private void Mover()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 dir = new Vector3(input.x, 0, input.y);

        if (dir.sqrMagnitude > 0.01f)
        {
            controller.SimpleMove(dir.normalized * moveSpeed);
        }
    }

    private void ApuntarConMouse()
    {

        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new Plane(Vector3.up, Vector3.zero); // plano en Y=0

        if (ground.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0;

            if (lookDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }
}
