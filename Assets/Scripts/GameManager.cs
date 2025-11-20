using System;
using System.Collections;
using UnityEngine;
using static GameManager;

public class GameManager : MonoBehaviour
{
    public static int ROWS = 6;
    public static int COLUMNS = 7;

    public int[,] board = new int[COLUMNS, ROWS];
    public Transform panel;

    public enum AIType { MiniMax, NegamaxAB, NegaScout, MTDf, BuscaAsp }
    public enum GameMode { PlayerVSIA, IAvsIA }

    public AIType selectedAI;
    public AIType playerVsAIType;
    public AIType iaType1;
    public AIType iaType2;

    bool isIAvsIAMode = false;
    public GameMode currentMode = GameMode.PlayerVSIA;

    // IA existentes
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

        // Informacion del menu
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
    // PLAYER MOVE
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
                    return;
                }

                AIMoveSingle(playerVsAIType);

                return;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // IA (PLAYER VS IA)
    // ──────────────────────────────────────────────────────────────
    void AIMoveSingle(AIType tipoIA)
    {
        int aiPlayer = -1;

        int bestCol;
        long nodosVisitados;
        string nombreIA;

        bestCol = GetBestMoveForAI(tipoIA, aiPlayer, out nombreIA, out nodosVisitados);

        if (bestCol < 0)
        {
            Debug.Log($"IA ({nombreIA}) no encuentra movimientos válidos.");
            return;
        }

        for (int row = 0; row < ROWS; row++)
        {
            if (board[bestCol, row] == 0)
            {
                board[bestCol, row] = aiPlayer;
                UpdateVisual(bestCol, row, Color.yellow);

                Debug.Log($"{nombreIA} jugó en columna {bestCol + 1} (nodos: {nodosVisitados})");

                if (CheckWin(board, aiPlayer))
                    Debug.Log($"¡Gana la IA ({nombreIA})!");

                return;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // IA vs IA 
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
            long nodosVisitados;
            int bestCol = GetBestMoveForAI(tipoActual, currentPlayer, out nombreIA, out nodosVisitados);

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

                    Debug.Log($"[{nombreIA}] jugó columna {bestCol + 1} (nodos: {nodosVisitados})");

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
                Debug.Log("Empate entre IAs");
                break;
            }

            currentPlayer = -currentPlayer;
        }

        isIAvsIAMode = false;
    }

    // ──────────────────────────────────────────────────────────────
    // GENERALIZACION DE BUSQUEDA PARA TODAS LAS IAs
    // ──────────────────────────────────────────────────────────────
    int GetBestMoveForAI(AIType tipo, int player, out string nombreIA, out long nodos)
    {
        int bestCol = -1;
        nombreIA = "";
        nodos = 0;

        switch (tipo)
        {
            case AIType.NegamaxAB:
                nombreIA = "NEGAMAX AB";
                bestCol = NegamaxAB.GetBestMove(board, searchDepth, player);
                nodos = NegamaxAB.NodesVisited;
                break;

            case AIType.NegaScout:
                nombreIA = "NEGASCOUT";
                bestCol = NegaScoutAI.GetBestMove(board, searchDepth, player);
                nodos = NegaScoutAI.NodesVisited;
                break;

            case AIType.MiniMax:
                nombreIA = "MINIMAX";
                Debug.LogWarning("MiniMax no implementado.");
                break;

            case AIType.MTDf:
                nombreIA = "MTD(f)";
                int? m = MTDfAI.MTD(board);
                bestCol = m.HasValue ? m.Value : -1;
                nodos = 0;
                break;

            case AIType.BuscaAsp:
                nombreIA = "BUSCA ASPIRACIONAL";
                bestCol = BuscaAspAI.GetBestMove(board, searchDepth, player);
                nodos = BuscaAspAI.NodesVisited;
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