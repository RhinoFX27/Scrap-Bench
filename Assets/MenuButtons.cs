using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("CombatScene");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player Said I quit");
    }
}