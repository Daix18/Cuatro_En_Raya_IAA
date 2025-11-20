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

    public enum AIType { MiniMax, NegamaxAB, NegaScout,MTDf } //CAMBIO AÑADIDO
    public enum GameMode { PlayerVSIA, IAvsIA}
    public AIType selectedAI;

    public AIType playerVsAIType;
    public AIType iaType1;
    public AIType iaType2;

    bool isIAvsIAMode = false;

    public GameMode currentMode = GameMode.PlayerVSIA;

    // AMBAS IAs
    public NegaScoutAI NegaScoutAI;
    public NegamaxAB NegamaxAB;
    public MiniMaxAI MiniMaxAI;
    public MTDAlgorithm MTDfAI;// CAMBIO AÑADIDO

    [Header("Configuración IA")]
    public bool usarNegamaxAB = true; // Cambia esto para elegir IA
    int searchDepth = 6;

    void Start()
    {
        InitializeBoard();
        NegaScoutAI = new NegaScoutAI();
        NegamaxAB = new NegamaxAB();
        MiniMaxAI = new MiniMaxAI();//CAMBIO AÑADIDO

        if (GameSettings.Instance != null)
        {
            currentMode = GameSettings.Instance.selectedMode;
            playerVsAIType = GameSettings.Instance.playerVsAIType;
            iaType1 = GameSettings.Instance.iaType1;
            iaType2 = GameSettings.Instance.iaType2;
        }

        Debug.Log($"[GameManager] Modo: {currentMode}, PlayerVsIA: {playerVsAIType}, IA1: {iaType1}, IA2: {iaType2}");

        // Si entras directo en modo IA vs IA, puedes lanzar la corrutina aquí:
        if (currentMode == GameMode.IAvsIA)
        {
            StartCoroutine(IAvsIACoroutine());
        }

    }

    void InitializeBoard()
    {
        for (int c = 0; c < COLUMNS; c++)
        {
            for (int r = 0; r < ROWS; r++)
            {
                board[c, r] = 0;
            }
        }
    }

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

                AIMoveSingle();

                return;
            }
        }
    }


    // ══════════════════════════════════════════════════════════════════
    // MÉTODO AIMove MODIFICADO
    // ══════════════════════════════════════════════════════════════════
    void AIMoveSingle()
    {
        int aiPlayer = -1;
        int bestCol;
        string nombreIA = "";
        long nodosVisitados = 0;

        if (usarNegamaxAB)
        {
            bestCol = NegamaxAB.GetBestMove(board, searchDepth, aiPlayer);
            nombreIA = "NEGAMAX AB";
            nodosVisitados = NegamaxAB.NodesVisited;
            Debug.Log(" NEGAMAX AB - Columna: {bestCol} - Nodos: {NegamaxAB.NodesVisited}");
        }
        else
        {
            bestCol = NegaScoutAI.GetBestMove(board, searchDepth, aiPlayer);
            nombreIA = "NEGASCOUT";
            nodosVisitados = NegaScoutAI.NodesVisited;
            Debug.Log(" NEGASCOUT - Columna: {bestCol} - Nodos: {NegaScoutAI.NodesVisited}");
        }

        if (bestCol < 0)
        {
            Debug.Log("La IA no encuentra movimientos válidos.");
            return;
        }

        for (int row = 0; row < ROWS; row++)
        {
            if (board[bestCol, row] == 0)
            {
                board[bestCol, row] = aiPlayer;
                UpdateVisual(bestCol, row, Color.yellow);

                // Mensaje final confirmando qué IA jugó
                Debug.Log(" {nombreIA} jugó en columna {bestCol + 1} (visitó {nodosVisitados} nodos)");

                if (CheckWin(board, aiPlayer))
                {
                    Debug.Log(" ¡Gana la IA ({nombreIA})!");
                }

                return;
            }
        }
    }
    // ══════════════════════════════════════════════════════════════════

    IEnumerator IAvsIACoroutine()
    {
        isIAvsIAMode = true;
        int currentPlayer = 1; // IA1 empieza (rojo)

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
                Debug.Log($"[{nombreIA}] no encuentra movimientos válidos.");
                break;
            }

            for (int row = 0; row < ROWS; row++)
            {
                if (board[bestCol, row] == 0)
                {
                    board[bestCol, row] = currentPlayer;
                    Color color = (currentPlayer == 1) ? Color.red : Color.yellow;
                    UpdateVisual(bestCol, row, color);

                    Debug.Log($"[{nombreIA}] ({(currentPlayer == 1 ? "IA1" : "IA2")}) jugó columna {bestCol + 1} (nodos: {nodosVisitados})");

                    if (CheckWin(board, currentPlayer))
                    {
                        Debug.Log($"¡Gana {nombreIA} como {(currentPlayer == 1 ? "IA1" : "IA2")}!");
                        isIAvsIAMode = false;
                        yield break;
                    }

                    break;
                }
            }

            if (IsBoardFull(board))
            {
                Debug.Log("Empate en IA vs IA.");
                isIAvsIAMode = false;
                yield break;
            }

            currentPlayer = -currentPlayer;
        }

        isIAvsIAMode = false;
    }

    int GetBestMoveForAI(AIType tipo, int player, out string nombreIA, out long nodosVisitados)
    {
        int bestCol = -1;
        nombreIA = "";
        nodosVisitados = 0;

        switch (tipo)
        {
            case AIType.NegamaxAB:
                bestCol = NegamaxAB.GetBestMove(board, searchDepth, player);
                nombreIA = "NEGAMAX AB";
                nodosVisitados = NegamaxAB.NodesVisited;
                break;

            case AIType.NegaScout:
                bestCol = NegaScoutAI.GetBestMove(board, searchDepth, player);
                nombreIA = "NEGASCOUT";
                nodosVisitados = NegaScoutAI.NodesVisited;
                break;

            case AIType.MiniMax:
                bestCol = MiniMaxAI.GetBestMove(board, searchDepth, player);
                nombreIA = "MINIMAX";
                nodosVisitados = MiniMaxAI.NodesVisited;
                // bestCol = MiniMaxAI.GetBestMove(board, searchDepth, player);
                break;

            case AIType.MTDf:
                bestCol = MTDfAI.GetBestMove(board, searchDepth);
                nombreIA = "MTDf";
                nodosVisitados = MTDfAI.NodesVisited;
                // bestCol = MiniMaxAI.GetBestMove(board, searchDepth, player);
                break;
        }

        return bestCol;
    }

    bool IsBoardFull(int[,] b)
    {
        for (int c = 0; c < COLUMNS; c++)
        {
            if (b[c, ROWS - 1] == 0)
                return false;
        }
        return true;
    }

    void UpdateVisual(int column, int row, Color color)
    {
        Transform circle = panel.GetChild(column).GetChild(row);
        var image = circle.GetComponent<UnityEngine.UI.Image>();
        image.color = color;
        Debug.Log("Column " + column + ", Row " + row + " -> Color: " + color);
    }

    bool CheckWin(int[,] b, int player)
    {
        // Horizontal
        for (int c = 0; c < COLUMNS - 3; c++)
        {
            for (int r = 0; r < ROWS; r++)
            {
                if (b[c, r] == player &&
                    b[c + 1, r] == player &&
                    b[c + 2, r] == player &&
                    b[c + 3, r] == player)
                    return true;
            }
        }

        // Vertical
        for (int c = 0; c < COLUMNS; c++)
        {
            for (int r = 0; r < ROWS - 3; r++)
            {
                if (b[c, r] == player &&
                    b[c, r + 1] == player &&
                    b[c, r + 2] == player &&
                    b[c, r + 3] == player)
                    return true;
            }
        }

        // Diagonal /
        for (int c = 0; c < COLUMNS - 3; c++)
        {
            for (int r = 0; r < ROWS - 3; r++)
            {
                if (b[c, r] == player &&
                    b[c + 1, r + 1] == player &&
                    b[c + 2, r + 2] == player &&
                    b[c + 3, r + 3] == player)
                    return true;
            }
        }

        // Diagonal \
        for (int c = 0; c < COLUMNS - 3; c++)
        {
            for (int r = 3; r < ROWS; r++)
            {
                if (b[c, r] == player &&
                    b[c + 1, r - 1] == player &&
                    b[c + 2, r - 2] == player &&
                    b[c + 3, r - 3] == player)
                    return true;
            }
        }

        return false;
    }

    public void SetGameMode(int modeIndex)
    {
        currentMode = (GameMode)modeIndex;
        Debug.Log("Modo de juego cambiado a: " + currentMode);

        StopAllCoroutines();  
        isIAvsIAMode = false;

        if (currentMode == GameMode.IAvsIA)
        {
            StartCoroutine(IAvsIACoroutine());
        }
    }
}