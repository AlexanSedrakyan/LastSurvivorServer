using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private float health;
    private float maxHealth = 400f;

    private float explosionTime;
    private float explosionmaxTime = 100f;
    private bool startExplosion = false;

    bool low75PerDetected = false;
    bool low50PerDetected = false;
    bool low25PerDetected = false;

    private int playerCntOn = 0;

    Renderer myRenderer;

    public int id;
    GridGenerator gridGenerator;

    public void Initialize(int _id, GridGenerator _gridGenerator)
    {
        id = _id;
        gridGenerator = _gridGenerator;
    }

    // Start is called before the first frame update
    void Start()
    {
        myRenderer = this.GetComponent<Renderer>();
        myRenderer.material.SetColor("_Color", Color.gray);
        health = maxHealth;
        explosionTime = explosionmaxTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCntOn > 0)
        {
            //maxHealth -= 10 * Time.fixedDeltaTime;

            if (health > 0)
                health -= 250 * Time.deltaTime;

            if ((health < 300) && (health > 200))
            {
                if (!low75PerDetected)
                {
                    myRenderer.material.SetColor("_Color", Color.green);
                    low75PerDetected = true;
                    ServerSend.TileHealth(this, 75);
                }
            }
            else if ((health < 200) && (health > 100))
            {
                if (!low50PerDetected)
                {
                    myRenderer.material.SetColor("_Color", Color.blue);
                    low50PerDetected = true;
                    ServerSend.TileHealth(this, 50);
                }
            }
            else if (health < 100)
            {
                if (!low25PerDetected)
                {
                    startExplosion = true;
                    myRenderer.material.SetColor("_Color", Color.red);
                    low25PerDetected = true;
                    ServerSend.TileHealth(this, 25);
                }
            }
        }

        if (startExplosion)
        {
            if (explosionTime > 0)
                explosionTime -= 100 * Time.deltaTime;
            else
            {
                ServerSend.TileHealth(this, 0);

                this.gameObject.SetActive(false);
                playerCntOn = 0;
                //health = maxHealth;
                //myRenderer.material.SetColor("_Color", Color.gray);
                //Destroy(this.gameObject);
            }
        }
    }

    public void PlayerOn()
    {
        playerCntOn += 1;
    }

    public void PlayerOff()
    {
        if (playerCntOn > 0)
            playerCntOn -= 1;
    }

    public void RecoverTile()
    {
        health = maxHealth;
        startExplosion = false;
        low75PerDetected = false;
        low50PerDetected = false;
        low25PerDetected = false;
        //playerCntOn = 0;
        myRenderer.material.SetColor("_Color", Color.gray);
        this.gameObject.SetActive(true);
    }
}
