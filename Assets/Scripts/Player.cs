using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    Rigidbody rb;
    Vector3 velocity;
    public float speed;

    public MapGen mapGen;

    // Start is called before the first frame update
    void Start()
    {
        mapGen = FindObjectOfType<MapGen>();
        rb = GetComponent<Rigidbody>();
    }

    public void Spawn(float x, float z)
    {
        transform.position = new Vector3(x, -3.5f, z);
    }

    // Update is called once per frame
    void Update()
    {
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        mapGen.GenerateMap();
    }

}
