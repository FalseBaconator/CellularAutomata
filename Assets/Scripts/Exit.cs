using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public bool is2D;

    public void Spawn(float x, float z)
    {
        if(is2D)
        {
            transform.position = new Vector3(x, z, 0);
        }
        else
        {
            transform.position = new Vector3(x, -3.5f, z);
        }
    }
}
