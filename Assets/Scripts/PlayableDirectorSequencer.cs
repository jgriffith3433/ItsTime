using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class PlayableDirectorSequencer : MonoBehaviour
{
    [Header("Timeline Sequence")]
    [Tooltip("The first PlayableDirector to play")]
    public PlayableDirector firstDirector;
    
    [Tooltip("The second PlayableDirector to play after the first finishes")]
    public PlayableDirector secondDirector;
    
    [Tooltip("The death director to play on player death")]
    public PlayableDirector deathDirector;
    
    [Tooltip("The win director to play on player win")]
    public PlayableDirector winDirector;
    
    [Header("Settings")]
    [Tooltip("Auto-start the first director on Start")]
    public bool autoStart = true;

    private void Start()
    {
        if (firstDirector == null)
        {
            Debug.LogError("First PlayableDirector is not assigned!");
            return;
        }
        
        if (secondDirector == null)
        {
            Debug.LogError("Second PlayableDirector is not assigned!");
            return;
        }
        var skipIntro = PlayerPrefs.GetInt("SkipIntro", 0) == 1;

        if (skipIntro)
        {
            Debug.Log("Test Second Director mode enabled. Playing second director directly.");
            firstDirector.gameObject.SetActive(false);
            secondDirector.Play();
            return;
        }
        // Subscribe to the stopped event
        firstDirector.stopped += OnFirstDirectorStopped;
        
        if (autoStart)
        {
            firstDirector.Play();
        }
    }

    public void PlayDeathSequence()
    {
        if (secondDirector != null)
        {
            secondDirector.stopped -= OnSecondDirectorStopped;
            secondDirector.Stop();
            secondDirector.gameObject.SetActive(false);
        }
        if (deathDirector != null)
        {
            deathDirector.gameObject.SetActive(true);
            deathDirector.stopped += OnDeathDirectorStopped;
            deathDirector.Play();
        }
        else
        {
            Debug.LogWarning("Death Director is not assigned!");
        }
    }

    public void PlayWinSequence()
    {
        if (secondDirector != null)
        {
            secondDirector.stopped -= OnSecondDirectorStopped;
            secondDirector.Stop();
            secondDirector.gameObject.SetActive(false);
        }
        if (winDirector != null)
        {
            winDirector.gameObject.SetActive(true);
            winDirector.stopped += OnWinDirectorStopped;
            winDirector.Play();
        }
        else
        {
            Debug.LogWarning("Win Director is not assigned!");
        }
    }

    private void OnWinDirectorStopped(PlayableDirector director)
    {
        if (director == winDirector)
        {
            winDirector.stopped -= OnWinDirectorStopped;
            winDirector.gameObject.SetActive(false);
            StartCoroutine(WaitAndQuit(3f));
        }
    }

    private IEnumerator WaitAndQuit(float delay)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }

    private void OnDeathDirectorStopped(PlayableDirector director)
    {
        if (director == deathDirector)
        {
            deathDirector.stopped -= OnDeathDirectorStopped;
            deathDirector.gameObject.SetActive(false);
            // reload the current scene or handle post-death logic here
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    // context menu for resetting player prefs
    [ContextMenu("ResetPlayerPrefs")]
    public void ResetSequence()
    {
        PlayerPrefs.DeleteAll();
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (firstDirector != null)
        {
            firstDirector.stopped -= OnFirstDirectorStopped;
        }
    }

    private void OnFirstDirectorStopped(PlayableDirector director)
    {
        // Make sure it's the first director that stopped
        if (director == firstDirector)
        {
            PlayerPrefs.SetInt("SkipIntro", 1);
            Debug.Log("First timeline finished. Starting second timeline.");
            firstDirector.gameObject.SetActive(false);
            secondDirector.Play();
            secondDirector.stopped += OnSecondDirectorStopped;
        }
    }

    private void OnSecondDirectorStopped(PlayableDirector director)
    {
        if (director == secondDirector)
        {
            secondDirector.stopped -= OnSecondDirectorStopped;
            PlayDeathSequence();
        }
    }

    // Public method to manually start the sequence
    public void StartSequence()
    {
        if (firstDirector != null)
        {
            firstDirector.Play();
        }
    }
}
