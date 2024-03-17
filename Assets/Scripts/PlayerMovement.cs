using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float deathKickForce = 10f;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;

    bool isAlive = true;
    Vector2 moveInput;
    Vector2 deathKick;

    Rigidbody2D myRigidbody2D;
    Animator myAnimator;
    CapsuleCollider2D bodyCollider2D;
    BoxCollider2D feetCollider2D;
    float startingGravityScale;


    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        bodyCollider2D = GetComponent<CapsuleCollider2D>();
        feetCollider2D = GetComponent<BoxCollider2D>();
        startingGravityScale = myRigidbody2D.gravityScale;
    }

    void Update()
    {
        if (!isAlive)
            return;

        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnMove(InputValue value)
    {
        if (!isAlive)
            return;

        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (!isAlive)
            return;

        if (value.isPressed && feetCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody2D.velocity += new Vector2(0f, jumpForce);
        }
    }

    void OnFire(InputValue value)
    {
        if (!isAlive)
            return;

        if(value.isPressed)
        {
            Instantiate(bullet, gun.position, transform.rotation);
        }
    }
    void Run()
    {
        if (!isAlive)
            return;

        Vector2 playerVelocity = new Vector2(moveSpeed * moveInput.x, myRigidbody2D.velocity.y);
        myRigidbody2D.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void ClimbLadder()
    {
        if (!isAlive)
            return;

        if (!feetCollider2D.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myRigidbody2D.gravityScale = startingGravityScale;
            myAnimator.SetBool("isClimbing", false);
            return;
        }

        myRigidbody2D.gravityScale = 0f;
        Vector2 climbVelocity = new Vector2(myRigidbody2D.velocity.x, moveInput.y * climbSpeed);
        myRigidbody2D.velocity = climbVelocity;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody2D.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("isClimbing", playerHasVerticalSpeed);
    }

    void FlipSprite()
    {
        if (!isAlive)
            return;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody2D.velocity.x), 1f);
        }
    }

    void Die()
    {
        if (bodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");

            //fling player in the sky
            deathKick = new Vector2(Mathf.Sign(myRigidbody2D.velocity.x) * deathKickForce, deathKickForce);
            myRigidbody2D.velocity = deathKick;

            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }
}
