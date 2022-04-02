using UnityEngine;

public class NpcVehicle : MonoBehaviour
{
    public float Velocity = 20f;

    private Rigidbody2D rb;
    private bool isMoving = false;
    private Vector2 moveVelocity;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetVelocity(int velocity = 0)
    {
        rb.WakeUp();
        moveVelocity = new Vector2(0, 0);
        isMoving = true;
        if (velocity != 0)
        {
            // use specified velocity
            moveVelocity.y = velocity;
        }
        else
        {
            // use default velocity
            moveVelocity.y = Velocity;
        }

        Debug.LogFormat("Vehicle: {0} now has vel: {1} , awake: {2}", gameObject.name, rb.velocity.y, rb.IsAwake());
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            rb.velocity = moveVelocity;
        }
    }
}
