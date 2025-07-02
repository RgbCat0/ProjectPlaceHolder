using Unity.Netcode;
using UnityEngine;
 
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
 
    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;
 
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
 
    [Rpc(SendTo.Server)]
    public void PlaySound3DRpc(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }
 
    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3DRpc(sfxLibrary.GetClipFromName(soundName), pos);
    }
 
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));
    }
}