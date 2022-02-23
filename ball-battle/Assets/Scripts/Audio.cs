using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public Globals Globals;
    public AudioSource BGM;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.bgm == true)
        {
            BGM.volume = 0.5f;
        }
        else
        {
            BGM.volume = 0f;
        }
    }
}
