using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Begin : MonoBehaviour
{
    public GameObject BeginUI;
    public GameObject HelpUI;
    // Start is called before the first frame update
    void Start()
    {

        //InputSystem.onAnyButtonPress
        //    .CallOnce(ctrl =>
        //    {
        //        Debug.Log($"{ctrl} pressed");
        //        SceneManager.LoadScene("Select");
        //    });
    }
    public void LoadSelectScene()
    {
        SceneManager.LoadScene("Select",LoadSceneMode.Additive);
    }

    public void ClickStartBtn()
    {
        BeginUI.SetActive(false);
        HelpUI.SetActive(true);
    }



}
