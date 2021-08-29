using UnityEngine;

[CreateAssetMenu(fileName = "new tts model so", menuName = "TTSMdoel")]
public class TTSModelSO : ScriptableObject
{
    public GameObject Prefab;
    public Texture Img;
    public TTSDescData[] TTSDatas;
    public AudioClip TTSClip;
}
