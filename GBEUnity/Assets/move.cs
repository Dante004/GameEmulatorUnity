using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class move : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        int border = 150;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            
            if (transform.localPosition.x > -300)
            {
                transform.Translate(Vector2.left * (Time.deltaTime + 0.9f));
            }
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            
            if (transform.localPosition.x < 320)
            {
                transform.Translate(Vector2.right * (Time.deltaTime + 0.9f));
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("wybrano element");
        }
    }
    public void click()
    {
        Debug.Log("wybrano element");
    }
}
