using System;
using UnityEngine;
using static GameManager;

public class GameManager : MonoBehaviour
{
    public static int ROWS = 6;
    public static int COLUMNS = 7;

    public int[,] board = new int[COLUMNS, ROWS];

    public Transform panel;

    public enum AIType { MiniMax, NegamaxAB, NegaScout}
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

    [Header("Configuración IA")]
    public bool usarNegamaxAB = true; // Cambia esto para elegir IA
    int searchDepth = 6;

    void Start()
    {
        InitializeBoard();
        NegaScoutAI = new NegaScoutAI();
        NegamaxAB = new NegamaxAB();

        Debug.Log("IA activa: " + (usarNegamaxAB ? "NegamaxAB" : "NegaScout"));
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
        if (isIAvsIAMode) return;

        for (int row = 0; row < ROWS; row++)
        {
            if (board[column, row] == 0)
            {
                board[column, row] = 1;
                UpdateVisual(column, row, Color.red);

                if (CheckWin(board, 1))
                {
                    Debug.Log("Gana el player");
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

    //Funciones para seleccionar ia y player

    public void SetPlayerVsAI(int aiIndex)
    {
        playerVsAIType = (AIType)aiIndex;
        Debug.Log($"Player vs IA: ahora usas {playerVsAIType}");
    }

    public void SetIA1Type(int aiIndex)
    {
        iaType1 = (AIType)aiIndex;
        Debug.Log($"IA1 ahora es {iaType1}");
    }

    public void SetIA2Type(int aiIndex)
    {
        iaType2 = (AIType)aiIndex;
        Debug.Log($"IA2 ahora es {iaType2}");
    }



}