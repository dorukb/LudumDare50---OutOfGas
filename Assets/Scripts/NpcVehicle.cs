using System.Collections;
using UnityEngine;

public class NpcVehicle : MonoBehaviour
{
    public float Velocity = 20f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;
    [SerializeField] SpriteRenderer sr;

    private bool isMoving = false;
    private Vector2 moveVelocity;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
    }

    public void ResetCar(int velocity = 0)
    {
        SetVelocity(velocity);
        anim.ResetTrigger("Disappear");
        anim.SetTrigger("Reset");
        sr.enabled = true;
        col.enabled = true;
    }

    private void SetVelocity(int velocity = 0)
    {
        sr.enabled = true;
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

        //Debug.LogFormat("Vehicle: {0} now has vel: {1} , awake: {2}", gameObject.name, rb.velocity.y, rb.IsAwake());
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            rb.velocity = moveVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.LogFormat("Vehicle: {0} collide w/ player. it will disappear.", gameObject.name);
            var playerControl = collision.gameObject.GetComponent<CarController>();
            playerControl.OnAnotherCarCrash();

            // Disable collider.
            col.enabled = false;

            // Stop movement
            moveVelocity = Vector2.zero;

            // Do disappear anim and hide.
            anim.ResetTrigger("Reset");
            anim.SetTrigger("Disappear");
        }   
    }
}
