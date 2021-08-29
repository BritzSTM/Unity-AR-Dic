using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TTSModel : MonoBehaviour
{
    [SerializeField] private TTSModelSO _modelSO;

    private static RequestKakaoTTS _ttsManager;
    private AudioSource _audioSource;

    [Header("Matrial")]
    [SerializeField] private bool _IsPlayDeployAnime = true;
    [SerializeField] private float _targetDeployAnimeTime = 1.0f;

    private float _playDeployAnimeTime;
    private float _lastPlayRate;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        if(_ttsManager == null)
            _ttsManager = FindObjectOfType<RequestKakaoTTS>();

        _audioSource = GetComponent<AudioSource>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();

        Debug.Assert(_ttsManager != null);
    }

    private void OnEnable()
    {
        _playDeployAnimeTime = 0.0f;
        _lastPlayRate = 0.0f;

        if (_modelSO.TTSClip == null)
        {
            _ttsManager.OnProgress += OnProgress;
            _ttsManager.OnDownloaded += OnDownloaded;
            _ttsManager.GetTTSAudioClip(_modelSO.TTSDatas.Cast<TTSDescData>().GetEnumerator());
        }
    }

    private void LateUpdate()
    {
        AnimeDepoly();
    }

    private void OnProgress(TTSRequestResult res)
    {
        Debug.Log(res.Progress);
    }

    private void OnDownloaded(TTSRequestResult res)
    {
        _ttsManager.OnProgress -= OnProgress;
        _ttsManager.OnDownloaded -= OnDownloaded;
        _modelSO.TTSClip = res.TTSAudioClip;
        _audioSource.clip = res.TTSAudioClip;
    }

    private void AnimeDepoly()
    {
        if (!_IsPlayDeployAnime && _lastPlayRate >= 1.0f)
            return;

        _playDeployAnimeTime += Time.deltaTime;

        _lastPlayRate = _playDeployAnimeTime / _targetDeployAnimeTime;
        if (_lastPlayRate < 1.0f)
        {
            _mpb.SetFloat(Shader.PropertyToID("_DeployRate"), _lastPlayRate);
            _meshRenderer.SetPropertyBlock(_mpb);
        }
        else
        {
            _meshRenderer.SetPropertyBlock(null);
            _audioSource.Play();
        }
    }
}
