using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2D : MonoBehaviour
{

    Rigidbody2D rb;
    Vector2 velocity;
    public float speed;
    MapGen mapGen;

    // Start is called before the first frame update
    void Start()
    {
        mapGen = FindObjectOfType<MapGen>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Spawn(float x, float y)
    {
        transform.position = new Vector3(x, y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * speed;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        mapGen.GenerateMap();
    }

}
