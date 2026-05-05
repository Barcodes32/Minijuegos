using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void LoadMemory() => SceneManager.LoadScene("MemoryGame");
    public void LoadRuleta() => SceneManager.LoadScene("Ruleta");
    public void LoadScratchAndWin() => SceneManager.LoadScene("ScratchAndWin");
    public void LoadSimonGame() => SceneManager.LoadScene("SimonGame");
    public void LoadFlappyGame() => SceneManager.LoadScene("FlappyGame");
}