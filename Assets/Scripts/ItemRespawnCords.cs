using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRespawnCords : MonoBehaviour
{
    private Vector3 rPos;
    private Quaternion rRotation;
    void Start()
    {
        rPos = transform.position;
        rRotation = transform.rotation;
    }

    public void Respawn(){
        transform.position = rPos;
        transform.rotation = rRotation;
        Debug.Log("Respawned item");
    }

}
