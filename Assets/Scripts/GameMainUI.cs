using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMainUI : MonoBehaviour
{
    public Text txtLevel;
    public Text txtTime;
    public Text txtStep;

    public DateTimeOffset startTime;
    // Start is called before the first frame update
    void Start()
    {
        var scene = SceneManager.GetActiveScene();
        txtLevel.text = scene.name;
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime != DateTimeOffset.MinValue)
        {
            txtTime.text = $"ºÄÊ±: {DateTimeOffset.Now - startTime}";
        }
    }
}
