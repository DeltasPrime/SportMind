using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    public void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

        public void LoadRelaxScene()
    {
        SceneManager.LoadScene("RelaxScene");
    }

    public void LoadShottingScene()
    {
        SceneManager.LoadScene("ShootingScene");
    }
        public void LoadWallScene()
    {
        SceneManager.LoadScene("WallScene");
    }

    public void LoadGymScene()
    {
        SceneManager.LoadScene("GymScene");
    }
}
