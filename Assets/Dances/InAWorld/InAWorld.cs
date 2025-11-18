using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

public class InAWorld : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private float waitTimeBeforeStart = 2f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(waitTimeBeforeStart);
        playableDirector.Play();
        yield return new WaitForSeconds((float)playableDirector.duration);
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }
}
