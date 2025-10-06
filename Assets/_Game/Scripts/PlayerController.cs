using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerRBController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty moveAction;     // Vector2
    public InputActionProperty mousePosAction; // Vector2

    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Refs")]
    public Camera mainCamera;

    Rigidbody rb;
    Vector3 desiredVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (!mainCamera) mainCamera = Camera.main;
    }

    void OnEnable() { moveAction.action.Enable(); mousePosAction.action.Enable(); }
    void OnDisable() { moveAction.action.Disable(); mousePosAction.action.Disable(); }

    void Update()
    {
        // calcular velocidad deseada en Update (input)
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 dir = new Vector3(input.x, 0, input.y).normalized;
        desiredVel = dir * moveSpeed;

        ApuntarConMouse(); // solo rota (no mueve)
    }

    void FixedUpdate()
    {
        // aplicar movimiento suave en físicas
        Vector3 targetPos = rb.position + desiredVel * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }

    void ApuntarConMouse()
    {
        if (!mainCamera) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        // En vez de un plano fijo Y=0, mejor raycast al ground real:
        if (Physics.Raycast(ray, out var hit, 200f, ~0, QueryTriggerInteraction.Ignore))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                rb.MoveRotation(Quaternion.LookRotation(lookDir));
        }
        // Si quieres mantener el plano Y=0:
        // Plane ground = new Plane(Vector3.up, Vector3.zero);
        // if (ground.Raycast(ray, out float enter)) { ... }
    }
}
