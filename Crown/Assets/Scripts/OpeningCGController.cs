using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class OpeningCGController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "PrologueScene";

    void Start()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        // 任意键跳过
        if (Input.anyKeyDown)
            LoadNextScene();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}