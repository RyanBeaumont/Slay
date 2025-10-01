using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    public float bpm;
    [HideInInspector] public float elapsedBeats;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Intervals[] _intervals;
    List<Projectile> activeProjectiles = new List<Projectile>();
    public float keyTolerance = 0.25f;
    public float perfectTolerance = 0.125f;
    void Update()
    {
        elapsedBeats = _audioSource.timeSamples / (_audioSource.clip.frequency * (60f / bpm));
        foreach (Intervals interval in _intervals)
        {
            float sampledTime = _audioSource.timeSamples / (_audioSource.clip.frequency * interval.GetIntervalLength(bpm));
            interval.CheckForNewInterval(sampledTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            activeProjectiles.Sort((a, b) =>
                Mathf.Abs(a.destinationBeat - elapsedBeats)
                .CompareTo(Mathf.Abs(b.destinationBeat - elapsedBeats))
            );

            foreach (var proj in activeProjectiles)
            {
                if (!proj.resolved && proj.TryHit())
                {
                    break; // only one projectile consumes this key press
                }
            }
        }
    }
}

[System.Serializable]
public class Intervals
{
    [SerializeField] private float _steps;
    [SerializeField] private UnityEvent _trigger;
    private int _lastInterval;

    public float GetIntervalLength(float bpm)
    {
        return 60f / (bpm * _steps);
    }

    public void CheckForNewInterval(float interval)
    {
        if (Mathf.FloorToInt(interval) != _lastInterval)
        {
            _lastInterval = Mathf.FloorToInt(interval);
            _trigger.Invoke();
        }
    }
}
