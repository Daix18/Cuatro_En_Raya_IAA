using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static int ROWS = 6;
    public static int COLUMNS = 7;

    public int[,] board = new int[COLUMNS, ROWS];
    public Transform panel;

    public enum AIType { MiniMax, NegamaxAB, NegaScout, MTDf, BuscaAsp }
    public enum GameMode { PlayerVSIA, IAvsIA }

    public AIType playerVsAIType;
    public AIType iaType1;
    public AIType iaType2;

    bool isIAvsIAMode = false;
    public GameMode currentMode = GameMode.PlayerVSIA;

    // Todas las IA
    public NegaScoutAI NegaScoutAI;
    public NegamaxAB NegamaxAB;
    public MiniMaxAI MiniMaxAI;
    public MTDAlgorithm MTDfAI;
    public BuscaAspAI BuscaAspAI;

    [Header("Configuración IA")]
    public int searchDepth = 6;

    void Start()
    {
        InitializeBoard();

        // Instanciar IA
        NegaScoutAI = new NegaScoutAI();
        NegamaxAB = new NegamaxAB();
        MiniMaxAI = new MiniMaxAI();
        MTDfAI = new MTDAlgorithm();
        BuscaAspAI = new BuscaAspAI();

        // Recibir configuración del menú
        if (GameSettings.Instance != null)
        {
            currentMode = GameSettings.Instance.selectedMode;
            playerVsAIType = GameSettings.Instance.playerVsAIType;
            iaType1 = GameSettings.Instance.iaType1;
            iaType2 = GameSettings.Instance.iaType2;
        }

        Debug.Log($"[GameManager] Modo: {currentMode}, PlayerVsIA: {playerVsAIType}, IA1: {iaType1}, IA2: {iaType2}");

        if (currentMode == GameMode.IAvsIA)
            StartCoroutine(IAvsIACoroutine());
    }

    void InitializeBoard()
    {
        for (int c = 0; c < COLUMNS; c++)
            for (int r = 0; r < ROWS; r++)
                board[c, r] = 0;
    }

    // ──────────────────────────────────────────────────────────────
    // JUGADOR HUMANO
    // ──────────────────────────────────────────────────────────────
    public void PlayerMove(int column)
    {
        if (currentMode != GameMode.PlayerVSIA)
            return;

        for (int row = 0; row < ROWS; row++)
        {
            if (board[column, row] == 0)
            {
                board[column, row] = 1;
                UpdateVisual(column, row, Color.red);

                if (CheckWin(board, 1))
                {
                    Debug.Log("Gana el jugador");
                    SceneManager.LoadScene(0);
                    return;
                }

                AIMoveSingle(playerVsAIType);
                return;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // JUGADA DE LA IA (PLAYER VS IA)
    // ──────────────────────────────────────────────────────────────
    void AIMoveSingle(AIType tipoIA)
    {
        int aiPlayer = -1;

        string nombreIA;
        long nodos;
        double tiempoMs;
        int bestCol = GetBestMoveForAI(tipoIA, aiPlayer, out nombreIA, out nodos, out tiempoMs);

        if (bestCol < 0)
        {
            Debug.Log($"IA {nombreIA} no encuentra movimientos válidos.");
            return;
        }

        for (int row = 0; row < ROWS; row++)
        {
            if (board[bestCol, row] == 0)
            {
                board[bestCol, row] = aiPlayer;
                UpdateVisual(bestCol, row, Color.yellow);

                Debug.Log($"[{nombreIA}] jugó columna {bestCol + 1} | nodos: {nodos} | tiempo: {tiempoMs:F2}ms");

                if (CheckWin(board, aiPlayer))
                {
                    Debug.Log($"¡Gana la IA {nombreIA}!");
                    SceneManager.LoadScene(0);
                    return;
                }

                return;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // IA VS IA
    // ──────────────────────────────────────────────────────────────
    IEnumerator IAvsIACoroutine()
    {
        isIAvsIAMode = true;
        int currentPlayer = 1;

        while (true)
        {
            if (CheckWin(board, 1) || CheckWin(board, -1) || IsBoardFull(board))
                break;

            yield return new WaitForSeconds(0.3f);

            AIType tipoActual = (currentPlayer == 1) ? iaType1 : iaType2;

            string nombreIA;
            long nodos;
            double tiempoMs;
            int bestCol = GetBestMoveForAI(tipoActual, currentPlayer, out nombreIA, out nodos, out tiempoMs);

            if (bestCol < 0)
            {
                Debug.Log($"{nombreIA} no encuentra movimientos válidos.");
                break;
            }

            for (int r = 0; r < ROWS; r++)
            {
                if (board[bestCol, r] == 0)
                {
                    board[bestCol, r] = currentPlayer;
                    UpdateVisual(bestCol, r, currentPlayer == 1 ? Color.red : Color.yellow);

                    Debug.Log($"[{nombreIA}] jugó columna {bestCol + 1} | nodos: {nodos} | tiempo: {tiempoMs:F2}ms");

                    if (CheckWin(board, currentPlayer))
                    {
                        Debug.Log($"¡Gana {nombreIA}!");
                        isIAvsIAMode = false;
                        yield break;
                    }

                    break;
                }
            }

            if (IsBoardFull(board))
            {
                Debug.Log("Empate entre IAs.");
                break;
            }

            currentPlayer = -currentPlayer;
        }

        isIAvsIAMode = false;
    }

    // ──────────────────────────────────────────────────────────────
    // SELECCIÓN DE IA
    // ──────────────────────────────────────────────────────────────
    int GetBestMoveForAI(AIType tipo, int player, out string nombreIA, out long nodos, out double tiempoMs)
    {
        int bestCol = -1;
        nombreIA = "";
        nodos = 0;
        tiempoMs = 0;

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        switch (tipo)
        {
            case AIType.NegamaxAB:
                nombreIA = "NEGAMAX AB";
                stopwatch.Start();
                bestCol = NegamaxAB.GetBestMove(board, searchDepth, player);
                stopwatch.Stop();
                nodos = NegamaxAB.NodesVisited;
                tiempoMs = stopwatch.Elapsed.TotalMilliseconds;
                break;

            case AIType.NegaScout:
                nombreIA = "NEGASCOUT";
                stopwatch.Start();
                bestCol = NegaScoutAI.GetBestMove(board, searchDepth, player);
                stopwatch.Stop();
                nodos = NegaScoutAI.NodesVisited;
                tiempoMs = stopwatch.Elapsed.TotalMilliseconds;
                break;

            case AIType.MiniMax:
                nombreIA = "MINIMAX";
                stopwatch.Start();
                bestCol = MiniMaxAI.GetBestMove(board, searchDepth, player);
                stopwatch.Stop();
                nodos = MiniMaxAI.NodesVisited;
                tiempoMs = stopwatch.Elapsed.TotalMilliseconds;
                break;

            case AIType.MTDf:
                nombreIA = "MTD(f)";
                stopwatch.Start();
                bestCol = MTDfAI.GetBestMove(board, searchDepth, player);
                stopwatch.Stop();
                nodos = MTDfAI.NodesVisited;
                tiempoMs = stopwatch.Elapsed.TotalMilliseconds;
                break;

            case AIType.BuscaAsp:
                nombreIA = "BUSCA ASPIRACIONAL";
                stopwatch.Start();
                bestCol = BuscaAspAI.GetBestMove(board, searchDepth, player);
                stopwatch.Stop();
                nodos = BuscaAspAI.NodesVisited;
                tiempoMs = stopwatch.Elapsed.TotalMilliseconds;
                break;
        }

        return bestCol;
    }

    // ──────────────────────────────────────────────────────────────
    // UTILIDADES
    // ──────────────────────────────────────────────────────────────
    bool IsBoardFull(int[,] b)
    {
        for (int c = 0; c < COLUMNS; c++)
            if (b[c, ROWS - 1] == 0) return false;
        return true;
    }

    void UpdateVisual(int c, int r, Color color)
    {
        var circle = panel.GetChild(c).GetChild(r).GetComponent<UnityEngine.UI.Image>();
        circle.color = color;
    }

    bool CheckWin(int[,] b, int p)
    {
        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 0; r < ROWS; r++)
                if (b[c, r] == p && b[c + 1, r] == p && b[c + 2, r] == p && b[c + 3, r] == p)
                    return true;

        for (int c = 0; c < COLUMNS; c++)
            for (int r = 0; r < ROWS - 3; r++)
                if (b[c, r] == p && b[c, r + 1] == p && b[c, r + 2] == p && b[c, r + 3] == p)
                    return true;

        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 0; r < ROWS - 3; r++)
                if (b[c, r] == p && b[c + 1, r + 1] == p && b[c + 2, r + 2] == p && b[c + 3, r + 3] == p)
                    return true;

        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 3; r < ROWS; r++)
                if (b[c, r] == p && b[c + 1, r - 1] == p && b[c + 2, r - 2] == p && b[c + 3, r - 3] == p)
                    return true;

        return false;
    }
}