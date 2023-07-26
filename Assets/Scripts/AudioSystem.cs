using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    [Header("AudioSource")]
    [SerializeField] AudioSource BGM;
    [SerializeField] AudioSource SFX;
    [Space(20)]

    [Header("AudioClip")]
    [SerializeField] AudioClip Sfx_Move;
    [SerializeField] AudioClip Sfx_Rotate;
    [SerializeField] AudioClip Sfx_Touch;
    [SerializeField] AudioClip Sfx_Untouch;
    [SerializeField] AudioClip Sfx_Drop;

    public static AudioSystem instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void playerMove()
    {
        SFX.clip = Sfx_Move;
        SFX.pitch = Random.Range(0.8f, 1.2f);
        SFX.Play();
    }

    public void playerRotate()
    {
        SFX.clip = Sfx_Rotate;
        SFX.pitch = 1;
        SFX.Play();
    }

    public void playerTouch()
    {
        SFX.clip = Sfx_Touch;
        SFX.pitch = 1;
        SFX.Play();
    }

    public void playerUntouch()
    {
        SFX.clip = Sfx_Untouch;
        SFX.pitch = 1;
        SFX.Play();
    }

    public void playerDrop()
    {
        SFX.clip = Sfx_Drop;
        SFX.pitch = 1;
        SFX.Play();
    }

    public void succeed()
    {

    }
}
