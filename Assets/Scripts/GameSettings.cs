using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("Modo de juego")]
    public GameMode selectedMode = GameMode.PlayerVSIA;

    [Header("Player vs IA")]
    public AIType playerVsAIType = AIType.NegaScout;

    [Header("IA vs IA")]
    public AIType iaType1 = AIType.NegamaxAB;
    public AIType iaType2 = AIType.NegaScout;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // NO se destruye al cambiar de escena
    }

    // Estos los llamas desde el menú en vez de los del GameManager
    public void SetGameMode(int modeIndex)
    {
        selectedMode = (GameMode)modeIndex;
        Debug.Log($"[Settings] Modo seleccionado: {selectedMode}");
    }

    public void SetPlayerVsAI(int aiIndex)
    {
        playerVsAIType = (AIType)aiIndex;
        Debug.Log($"[Settings] Player vs IA contra: {playerVsAIType}");
    }

    public void SetIA1Type(int aiIndex)
    {
        iaType1 = (AIType)aiIndex;
        Debug.Log($"[Settings] IA1: {iaType1}");
    }

    public void SetIA2Type(int aiIndex)
    {
        iaType2 = (AIType)aiIndex;
        Debug.Log($"[Settings] IA2: {iaType2}");
    }

    public void LoadMainGameScene (int level)
    {
        SceneManager.LoadScene(level);
    }
}
