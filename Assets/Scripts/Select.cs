using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Select : MonoBehaviour
{
    public ScrollRect rect;
    public string[] sceneNames;
    // Start is called before the first frame update
    void Start()
    {
        var count = sceneNames.Length;
        for (var i = 0; i < count; i++)
        {
            Transform child;
            if (i >= rect.content.childCount)
            {
                child = Instantiate(rect.content.GetChild(i - 1), rect.content);
            }
            else
            {
                child = rect.content.GetChild(i);
            }

            var name = sceneNames[i];
            child.GetComponentInChildren<Text>().text = name;
            child.GetComponent<Button>().onClick.AddListener(() =>
            {
                SceneManager.LoadScene(name);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
