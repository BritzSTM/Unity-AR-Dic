using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTIDeployManager : MonoBehaviour
{
    [SerializeField] private TTSModelSO[] _ttsModels;

    private ARTrackedImageManager _tiManager;

    private Dictionary<TrackableId, GameObject> _instanceMap = new Dictionary<TrackableId, GameObject>();

    private void Awake()
    {
        _tiManager = FindObjectOfType<ARTrackedImageManager>();

        Debug.Assert(_tiManager != null);
    }

    private void OnEnable()
    {
        _tiManager.trackedImagesChanged += OnChangedTrackingImg;
    }

    private void OnDisable()
    {
        _tiManager.trackedImagesChanged -= OnChangedTrackingImg;
    }

    private void OnChangedTrackingImg(ARTrackedImagesChangedEventArgs args)
    {
        if(args.added.Count > 0)
        {
            foreach(var plane in args.added)
            {
                var pickedModel = _ttsModels.Where(x => x.Img.name == plane.referenceImage.name).Single();

                if(pickedModel == null)
                {
                    Debug.LogWarning($"Not found correct prefab in TTS models.");
                    break;
                }

                Debug.Log($"inst{pickedModel.Prefab.name}");
                var inst = Instantiate(pickedModel.Prefab, plane.transform.position, plane.transform.rotation);
                _instanceMap[plane.trackableId] = inst;
            }
        }

        if (args.updated.Count > 0)
        {
            foreach (var plane in args.updated)
            {
                GameObject outObj;
                bool found = _instanceMap.TryGetValue(plane.trackableId, out outObj);

                if (!found)
                {
                    Debug.LogWarning("Not found correct inst in inst map for update");
                    break;
                }

                Debug.Log($"update {outObj.name}");
                outObj.transform.position = plane.transform.position;

                if (plane.trackingState == TrackingState.Tracking)
                    outObj.SetActive(true);
                else
                    outObj.SetActive(false);
            }
        }

        if (args.removed.Count > 0)
        {
            foreach (var plane in args.removed)
            {
                GameObject outObj;  
                if(_instanceMap.TryGetValue(plane.trackableId, out outObj))
                {
                    Debug.Log($"remove {outObj.name}");
                    Destroy(outObj);
                }
                else
                    Debug.LogWarning("Not found correct inst in inst map for remove");
            }
        }
    }
}
