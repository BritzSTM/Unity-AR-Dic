using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public enum KakaoTTSVoiceType
{
    WOMAN_READ_CALM,
    MAN_READ_CALM,
    WOMAN_DIALOG_BRIGHT,
    MAN_DIALOG_BRIGHT
}

public class RequestKakaoTTS : MonoBehaviour
{
    private const string _requsetUrl = "https://kakaoi-newtone-openapi.kakao.com/v1/synthesize";
    private static UTF8Encoding _utf8Encoder = new UTF8Encoding();

    [SerializeField] private string _restKey = "";

    public event UnityAction<TTSRequestResult> OnDownloaded;
    public event UnityAction<TTSRequestResult> OnProgress;

    public void GetTTSAudioClip(IEnumerator<TTSDescData> enumerator)
    {
        var bodyDatas = GetRawBodyData(enumerator);

        if(bodyDatas == null || bodyDatas.Length == 0)
        {
            Debug.LogWarning("KakaoTTS bodyDatas is 0");
            return;
        }

        StartCoroutine(DownloadAudioClip(bodyDatas));
    }

    private static byte[] GetRawBodyData(IEnumerator<TTSDescData> enumerator)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("<speak>");
        
        while(enumerator.MoveNext())
        {
            builder.AppendFormat("<voice name=\"{0}\">{1}<break/></voice>", enumerator.Current.VoiceType, enumerator.Current.Text);
        }

        builder.Append("</speak>");

        return _utf8Encoder.GetBytes(builder.ToString());
    }

    private IEnumerator DownloadAudioClip(byte[] bodyDatas)
    {
        using (UnityWebRequest postReq = UnityWebRequestMultimedia.GetAudioClip(_requsetUrl, AudioType.MPEG))
        {
            postReq.method = "POST";
            postReq.SetRequestHeader("Content-Type", "application/xml");
            postReq.SetRequestHeader("Authorization", $"KakaoAK {_restKey}");
            postReq.uploadHandler = new UploadHandlerRaw(bodyDatas);

            yield return postReq.SendWebRequest();

            var res = new TTSRequestResult();

            // Failed...
            if (postReq.result == UnityWebRequest.Result.ConnectionError)
            {
                res.TTSAudioClip = null;
                res.Progress = -1.0f;
                res.Log = postReq.error;
                OnDownloaded.Invoke(res);

                yield break;
            }

            // Progress
            res.Log = "Download progress";
            while(!postReq.isDone)
            {
                res.Progress = postReq.downloadProgress;
                OnProgress.Invoke(res);
            }

            // Downloaded
            res.TTSAudioClip = DownloadHandlerAudioClip.GetContent(postReq);
            res.Progress = 1.0f;
            res.Log = "Downloaded";

            OnDownloaded.Invoke(res);
        }
    }
}
