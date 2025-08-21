using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public float speed = 5f;
    public Projectile laserPrefab;
    private Projectile laser;
    public float fireRate = 0.5f; // Time in seconds between each shot
    private float nextFireTime = 0f; // Tracks the next allowed fire time

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        Vector3 position = transform.position;

        // Update the position of the player based on the input
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            //MoveLeft();
            //position.x -= speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            //MoveRight();
            //position.x += speed * Time.deltaTime;
        }

        // Clamp the position of the character so they do not go out of bounds
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        position.x = Mathf.Clamp(position.x, leftEdge.x, rightEdge.x);

        // Set the new position
        transform.position = position;

        // Only one laser can be active at a given time so first check that
        // there is not already an active laser
        //if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        if (Time.time >= nextFireTime)
        {
            FireLaser();
            nextFireTime = Time.time + fireRate; // Set the next allowed fire time
        }
        //if (laser == null && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))) {
        //    laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        //}
    }



    public void MoveLeft()
    {
        Vector2 movement = Vector2.left * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    public void MoveRight()
    {
        Vector2 movement = Vector2.right * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }
    void FireLaser()
    {
        // Instantiate the laser at the current position
        Instantiate(laserPrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Missile") ||
            other.gameObject.layer == LayerMask.NameToLayer("Invader"))
        {
            GameManager.Instance.OnPlayerKilled(this);
        }
    }

}
