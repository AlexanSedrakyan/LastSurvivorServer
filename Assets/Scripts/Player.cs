using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f; //-9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float throwForce = 600f;
    public float health;
    public float maxHealth = 100f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;

    private bool[] inputs;
    private float yVelocity = 0;

    private bool movementDisabled = false;
    private Vector3 respawnPosition;

    private void Start()
    {
        controller = this.GetComponent<CharacterController>();
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[5];

        respawnPosition = transform.position;
    }

    public void Update()
    {
        CheckCollisions();
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        if (!movementDisabled)
        {
            //Quaternion toRotation = Quaternion.LookRotation(_moveDirection);
            
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 15f);
            //Debug.Log(transform.rotation);

            controller.Move(_moveDirection);
            
        }

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if (health <= 0f)
        {
            return;
        }

        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
            else if (_hit.collider.CompareTag("Enemy"))
            {
                _hit.collider.GetComponent<Enemy>().TakeDamage(50f);
            }
        }
    }

    public void ThrowItem(Vector3 _viewDirection)
    {
        if (health <= 0f)
        {
            return;
        }

        if (itemAmount > 0)
        {
            itemAmount--;
            NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(_viewDirection, throwForce, id);
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0f)
        {
            return;
        }

        health -= _damage;
        if (health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }

    private void CheckCollisions()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit _hit, 1f))
        {
            movementDisabled = false;

            if (_hit.collider.CompareTag("DeadZone"))
            {
                //Debug.Log("DeadZone");
                movementDisabled = true;
                yVelocity = 0f;
                transform.position = respawnPosition;
            }
            else if (_hit.collider.CompareTag("Tile"))
            {
                //PlayerOn();
                //Debug.Log("Tile");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.transform.tag);
        if (collision.transform.tag == "DeadZone")
        {
            //Debug.Log("zzzzzzzzz DeadZone");
            movementDisabled = true;
            yVelocity = 0f;
            transform.position = respawnPosition;
        }
        else if (collision.transform.tag == "Tile")
        {
            collision.gameObject.GetComponent<Tile>().PlayerOn();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log(collision.transform.tag);
        if (collision.transform.tag == "DeadZone")
        {
            movementDisabled = false;
        }
        else if (collision.transform.tag == "Tile")
        {
            collision.gameObject.GetComponent<Tile>().PlayerOff();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit detected");
    }
}
