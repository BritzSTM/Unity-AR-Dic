using System;
using UnityEngine;

[Serializable]
public struct TTSDescData
{
    public KakaoTTSVoiceType VoiceType; // Too hard type...
    [TextArea(3, 15)] public string Text;
}

[Serializable]
public struct TTSRequestResult
{
    public AudioClip TTSAudioClip;
    public float Progress; // -1 이면 오류로 인한 실패를 나타냄
    public string Log;
}