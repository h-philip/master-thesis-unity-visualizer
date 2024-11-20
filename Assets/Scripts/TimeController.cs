using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    public Button PlayPauseButton;
    public Slider Slider;
    public TextMeshProUGUI Text;
    public GameObject Drone;

    private string _textMask = "{0:F1} / {1:F0} s";
    private bool _running = false;
    private float _playbackRate = 0;
    List<Vector3> _path = new();
    private List<float> _times = new();
    private Vector3 _droneTargetPosition;

    // Start is called before the first frame update
    void Start()
    {
        PlayPauseButton.onClick.AddListener(OnPlayPause);
        Slider.onValueChanged.AddListener(OnSlideChanged);
    }

    // Update is called once per frame
    void Update()
    {
        if (_running)
        {
            Slider.value += _playbackRate * Time.deltaTime;
            if (Slider.value >= Slider.maxValue)
            {
                OnPlayPause();
            }
        }
    }

    void LateUpdate()
    {
        Drone.transform.localPosition = _droneTargetPosition;
    }

    public void OnPlayPause()
    {
        _running = !_running;
        if (_running)
        {
            PlayPauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause";
            _playbackRate = 1;
            if (Slider.value >= Slider.maxValue)
            {
                Slider.value = Slider.minValue;
            }
        }
        else
        {
            PlayPauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
            _playbackRate = 0;
        }
    }

    public void OnSlideChanged(float value)
    {
        Text.text = string.Format(_textMask, value, Slider.maxValue);
        int prevIndex = -1;
        for (int i = 0; i < _times.Count() - 1; i++)
        {
            if (_times[i] < value && _times[i + 1] > value)
            {
                prevIndex = i;
            }
        }
        if (prevIndex == -1)
            return;
        float t = (value - _times[prevIndex]) / (_times[prevIndex + 1] - _times[prevIndex]);
        _droneTargetPosition = Vector3.Lerp(_path[prevIndex], _path[prevIndex + 1], t);
    }

    public void ResetPath(List<Vector3> path)
    {
        _path = path;
        Drone.transform.localPosition = path.First();
    }

    public void ResetTimes(List<float> times)
    {
        _times = times;
        if (_running)
        {
            OnPlayPause();
        }
        Slider.minValue = times[0];
        Slider.maxValue = times.Last();
        Slider.value = times[0];
    }
}
