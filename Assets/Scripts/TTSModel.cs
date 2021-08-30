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
    private bool _playedTTS;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _mpb;
    private InfoUI _infoUI;

    private void Awake()
    {
        if(_ttsManager == null)
            _ttsManager = FindObjectOfType<RequestKakaoTTS>();

        _audioSource = GetComponent<AudioSource>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();
        _infoUI = GetComponentInChildren<InfoUI>();

        Debug.Assert(_ttsManager != null);
    }

    private void OnEnable()
    {
        _playDeployAnimeTime = 0.0f;
        _lastPlayRate = 0.0f;
        _playedTTS = false;
        _infoUI.gameObject.SetActive(false);

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
        // 기존 쉐이더 작성할걸 방향성만 변경하여 재생률에 문제가 있음
        // 현재 1.0 배치율은 실제로는 0.3임

        if (!_IsPlayDeployAnime && _lastPlayRate >= 1.0f)
            return;

        // 보정했음
        _playDeployAnimeTime += (Time.deltaTime / 3.0f);

        _lastPlayRate = _playDeployAnimeTime / _targetDeployAnimeTime;

        // 현재 쉐이더의 0.3이 == 1
        if (_lastPlayRate < 0.3f)
        {
            _mpb.SetFloat(Shader.PropertyToID("_DeployRate"), _lastPlayRate);
            _meshRenderer.SetPropertyBlock(_mpb);
        }
        else
        {
            _meshRenderer.SetPropertyBlock(null);

            if (!_playedTTS)
            {
                _audioSource.Play();
                _playedTTS = true;

                _infoUI.Name.text = _modelSO.TTSDatas[0].Text;
                _infoUI.Desc.text = _modelSO.TTSDatas[1].Text;
                _infoUI.gameObject.SetActive(true);
            }
        }
    }
}
