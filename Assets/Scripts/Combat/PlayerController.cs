using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScrapBench.Combat
{
    // Top-down 2D: WASD / arrows to move, mouse to aim, hold left click to shoot.
    [RequireComponent(typeof(Rigidbody2D), typeof(Weapon), typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private bool rotateToAim = true;
        [SerializeField] private float restartDelay = 2f;

        private Rigidbody2D rb;
        private Weapon weapon;
        private Health health;
        private Camera cam;
        private Vector2 move;
        private Vector2 aimDir = Vector2.right;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            weapon = GetComponent<Weapon>();
            health = GetComponent<Health>();
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= OnDied;
        }

        private void Update()
        {
            if (health.IsDead) return;

            move = GameInput.Move();

            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                Vector2 screen = GameInput.PointerScreenPosition();
                Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z)));
                Vector2 toMouse = (Vector2)world - (Vector2)transform.position;
                if (toMouse.sqrMagnitude > 0.01f) aimDir = toMouse.normalized;
            }

            if (rotateToAim)
            {
                float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (GameInput.FireHeld()) weapon.TryFire(aimDir);
        }

        private void FixedUpdate()
        {
            PhysicsCompat.SetVelocity(rb, health.IsDead ? Vector2.zero : move * moveSpeed);
        }

        private void OnDied()
        {
            Debug.Log("[Combat] Player died. Restarting scene...");
            Invoke(nameof(Restart), restartDelay);
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
