using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishPannel : MonoBehaviour
{
    public Button btnRestart;
    public Button btnNext;
    public Button btnSelect;
    public Text txtTitle;
    public Text txtTime;
    public Text txtStep;
    public Text txtLevel;
    public GameObject GameMainUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        btnRestart.onClick.AddListener(onRestart);
        btnNext.onClick.AddListener(onNext);
        btnSelect.onClick.AddListener(onSelect);
    }

    private void OnDisable()
    {
        btnRestart.onClick.RemoveListener(onRestart);
        btnNext.onClick.RemoveListener(onNext);
        btnSelect.onClick.RemoveListener(onSelect);
    }

    private void onRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    private void onNext()
    {
        var index = SceneManager.GetActiveScene().buildIndex + 1;
        if (index < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(index);
        }
        else
        {
            Application.Quit();
        }
    }

    private void onSelect()
    {
        SceneManager.LoadScene("Select",LoadSceneMode.Additive);
    }

    public static readonly string[] SucceedTitles =
    {
        "惊！乱打的吧？",
        "夸夸夸，太棒了",
        "恭喜你通关了！",
    };

    public static readonly string[] FailTitles =
    {
        "悲",
        "再试试？",
        "要不要换个思路？",
    };
    public void Init(bool isSucceed, TimeSpan time, int step)
    {
        var scene = SceneManager.GetActiveScene();
        txtTitle.text = isSucceed 
            ? SucceedTitles[UnityEngine.Random.Range(0, SucceedTitles.Length)]
            : FailTitles[UnityEngine.Random.Range(0, FailTitles.Length)];
        txtTime.text = $"耗时: {time}";
        txtStep.text = $"步数: {step}";
        txtLevel.text = scene.name;
        gameObject.SetActive(true);
        GameMainUI.SetActive(false);
        btnNext.gameObject.SetActive(isSucceed);

        if (scene.buildIndex + 1 == SceneManager.sceneCountInBuildSettings)
        {
            btnNext.GetComponentInChildren<Text>().text = "结束游戏";
        }
    }
}
