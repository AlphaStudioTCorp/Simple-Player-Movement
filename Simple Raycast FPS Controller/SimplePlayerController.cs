using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Камера")]
    [SerializeField] private Transform playerCamera;

    [Header("Настройки движения")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Проверка земли через Райкаст")]
    [Tooltip("Длина луча вниз. Должна быть чуть больше, чем половина высоты твоего персонажа")]
    [SerializeField] private float rayDistance = 1.1f;
    [Tooltip("Укажи здесь слой земли (например, Ground), чтобы луч не реагировал на самого игрока")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Обзор мышью")]
    [SerializeField] private float mouseSensitivity = 0.1f;

    private CharacterController controller;
    private Vector3 velocity;
    private float rotationY = 0f;
    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Тот самый Raycast: пускаем луч из центра игрока (transform.position) строго вниз (Vector3.down)
        // Он возвращает true, если наткнулся на объект со слоем groundLayer на расстоянии rayDistance
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);

        // Рисуем зеленый луч в редакторе Unity, чтобы ты глазами видел, как он работает
        Debug.DrawRay(transform.position, Vector3.down * rayDistance, isGrounded ? Color.green : Color.red);

        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMovement()
    {
        float inputX = 0f;
        float inputY = 0f;
        bool jumpPressed = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) inputY = 1f;
            if (Keyboard.current.sKey.isPressed) inputY = -1f;
            if (Keyboard.current.aKey.isPressed) inputX = -1f;
            if (Keyboard.current.dKey.isPressed) inputX = 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
        }

        Vector3 move = (transform.forward * inputY + transform.right * inputX).normalized;
        controller.Move(move * walkSpeed * Time.deltaTime);

        // Гравитация и прыжок с учетом нашего луча
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Слегка прижимаем к земле, чтобы не скользить на склонах
        }

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (playerCamera == null || Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(rotationY, 0f, 0f);
    }
}
