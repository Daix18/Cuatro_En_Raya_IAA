using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int ROWS = 6;
    public static int COLUMNS = 7;

    public int[,] board = new int[COLUMNS, ROWS];
    public Transform panel;

    public enum AIType { MiniMax, NegamaxAB, NegaScout, MTDf }
    public AIType selectedAI = AIType.NegamaxAB;

    bool isIAvsIAMode = false;

    // IAs disponibles
    public NegaScoutAI NegaScoutAI;
    public NegamaxAB NegamaxAB;
    public MTDAlgorithm MTDfAI;

    [Header("Configuración IA")]
    public int searchDepth = 6;

    void Start()
    {
        InitializeBoard();
        NegaScoutAI = new NegaScoutAI();
        NegamaxAB = new NegamaxAB();
        MTDfAI = new MTDAlgorithm();

        Debug.Log("IA activa: " + selectedAI);
    }

    void InitializeBoard()
    {
        for (int c = 0; c < COLUMNS; c++)
            for (int r = 0; r < ROWS; r++)
                board[c, r] = 0;
    }

    // ────────────────────────────────────────────────────────────────
    //                     MOVIMIENTO DEL JUGADOR
    // ────────────────────────────────────────────────────────────────
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
                    Debug.Log("✓ Gana el PLAYER");
                    return;
                }

                AIMoveSingle();
                return;
            }
        }
    }

    // ────────────────────────────────────────────────────────────────
    //                         IA SINGLEPLAYER
    // ────────────────────────────────────────────────────────────────
    void AIMoveSingle()
    {
        int aiPlayer = -1;

        int bestCol = -1;
        long nodosVisitados = 0;
        string nombreIA = selectedAI.ToString();

        switch (selectedAI)
        {
            case AIType.NegamaxAB:
                Debug.LogError("AIType NegamaxAB");
                bestCol = NegamaxAB.GetBestMove(board, searchDepth, aiPlayer);
                nodosVisitados = NegamaxAB.NodesVisited;
                break;

            case AIType.NegaScout:
                Debug.LogError("AIType NegaScout");
                bestCol = NegaScoutAI.GetBestMove(board, searchDepth, aiPlayer);
                nodosVisitados = NegaScoutAI.NodesVisited;
                break;

            case AIType.MTDf:
                Debug.LogError("AIType MTDf");
                bestCol = MTDfAI.GetBestMove(board, searchDepth, aiPlayer);
                nodosVisitados = MTDfAI.NodesVisited;
                break;

            default:
                Debug.LogError("AIType MiniMax aún no implementado.");
                return;
        }

        if (bestCol < 0)
        {
            Debug.Log("✗ La IA no encuentra movimientos válidos.");
            return;
        }

        // Colocar pieza IA
        for (int row = 0; row < ROWS; row++)
        {
            if (board[bestCol, row] == 0)
            {
                board[bestCol, row] = aiPlayer;
                UpdateVisual(bestCol, row, Color.yellow);

                Debug.Log($"✓ {nombreIA} jugó en columna {bestCol + 1} (nodos: {nodosVisitados})");

                if (CheckWin(board, aiPlayer))
                {
                    Debug.Log($"🏆 ¡Gana la IA ({nombreIA})!");
                }
                return;
            }
        }
    }

    // ────────────────────────────────────────────────────────────────
    //                      ACTUALIZAR VISUAL
    // ────────────────────────────────────────────────────────────────
    void UpdateVisual(int column, int row, Color color)
    {
        Transform circle = panel.GetChild(column).GetChild(row);
        var image = circle.GetComponent<UnityEngine.UI.Image>();
        image.color = color;

        Debug.Log($"Column {column}, Row {row} -> {color}");
    }

    // ────────────────────────────────────────────────────────────────
    //                          CHECK WIN
    // ────────────────────────────────────────────────────────────────
    bool CheckWin(int[,] b, int player)
    {
        int C = COLUMNS;
        int R = ROWS;

        // Horizontal
        for (int c = 0; c < C - 3; c++)
            for (int r = 0; r < R; r++)
                if (b[c, r] == player && b[c + 1, r] == player && b[c + 2, r] == player && b[c + 3, r] == player)
                    return true;

        // Vertical
        for (int c = 0; c < C; c++)
            for (int r = 0; r < R - 3; r++)
                if (b[c, r] == player && b[c, r + 1] == player && b[c, r + 2] == player && b[c, r + 3] == player)
                    return true;

        // Diagonal /
        for (int c = 0; c < C - 3; c++)
            for (int r = 0; r < R - 3; r++)
                if (b[c, r] == player && b[c + 1, r + 1] == player && b[c + 2, r + 2] == player && b[c + 3, r + 3] == player)
                    return true;

        // Diagonal \
        for (int c = 0; c < C - 3; c++)
            for (int r = 3; r < R; r++)
                if (b[c, r] == player && b[c + 1, r - 1] == player && b[c + 2, r - 2] == player && b[c + 3, r - 3] == player)
                    return true;

        return false;
    }

    // ────────────────────────────────────────────────────────────────
    //                         CAMBIAR IA
    // ────────────────────────────────────────────────────────────────
    public void SetAI(int type)
    {
        selectedAI = (AIType)type;
        Debug.Log("✔ IA cambiada a: " + selectedAI);
    }
}