using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    private int countOfLeftKeyPressed = 0;
    private int countOfRightKeyPresssed = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            countOfLeftKeyPressed++;
            if (countOfLeftKeyPressed % 2 == 0)
            {
                transform.Translate(Vector2.right * (Time.deltaTime + 1.2f));
            }

            Debug.Log(countOfLeftKeyPressed);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            countOfRightKeyPresssed++;
            if (countOfRightKeyPresssed % 2 == 0)
            {
                transform.Translate(Vector2.left * (Time.deltaTime + 1.2f));
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("wybrano element");
        }
    }
}
