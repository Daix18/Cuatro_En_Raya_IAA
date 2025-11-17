using System;
using System.Collections.Generic;
using UnityEngine;

public class NegamaxAB
{
    public long NodesVisited { get; private set; }

    const int ROWS = 6;
    const int COLUMNS = 7;

    public int GetBestMove(int[,] board, int depth, int player)
    {
        NodesVisited = 0;
        int bestMove = -1;
        int bestScore = int.MinValue + 1;

        // Ordenar movimientos: centro primero para mejor poda
        List<int> moves = GetOrderedMoves(board);

        foreach (int move in moves)
        {
            if (!IsColumnPlayable(board, move))
                continue;

            ApplyMove(board, move, player);
            int score = -Negamax(board, depth - 1, int.MinValue + 1, int.MaxValue - 1, -player);
            UndoMove(board, move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        Debug.Log($"NegamaxAB visitó {NodesVisited} nodos. Mejor movimiento: {bestMove}");
        return bestMove;
    }

    private int Negamax(int[,] board, int depth, int alpha, int beta, int player)
    {
        NodesVisited++;

        // Caso terminal
        if (depth == 0 || IsTerminal(board))
        {
            return Evaluate(board, player);
        }

        int bestScore = int.MinValue + 1;
        List<int> moves = GetOrderedMoves(board);

        foreach (int move in moves)
        {
            if (!IsColumnPlayable(board, move))
                continue;

            ApplyMove(board, move, player);
            int score = -Negamax(board, depth - 1, -beta, -alpha, -player);
            UndoMove(board, move);

            if (score > bestScore)
            {
                bestScore = score;
            }

            alpha = Math.Max(alpha, bestScore);

            // Poda Alfa-Beta
            if (alpha >= beta)
            {
                break;
            }
        }

        return bestScore;
    }

    // ------------------- UTILIDADES DE TABLERO  -------------------

    bool IsColumnPlayable(int[,] board, int col)
    {
        return board[col, ROWS - 1] == 0;
    }

    void ApplyMove(int[,] board, int col, int player)
    {
        for (int row = 0; row < ROWS; row++)
        {
            if (board[col, row] == 0)
            {
                board[col, row] = player;
                return;
            }
        }
    }

    void UndoMove(int[,] board, int col)
    {
        for (int row = ROWS - 1; row >= 0; row--)
        {
            if (board[col, row] != 0)
            {
                board[col, row] = 0;
                return;
            }
        }
    }

    List<int> GetOrderedMoves(int[,] board)
    {
        // Ordenar: centro primero para mejor poda
        List<int> moves = new List<int>();
        int[] columnOrder = new int[] { 3, 2, 4, 1, 5, 0, 6 };

        foreach (int col in columnOrder)
        {
            if (IsColumnPlayable(board, col))
            {
                moves.Add(col);
            }
        }
        return moves;
    }

    bool IsTerminal(int[,] board)
    {
        return CheckWin(board, 1) || CheckWin(board, -1) || IsBoardFull(board);
    }

    bool IsBoardFull(int[,] board)
    {
        for (int c = 0; c < COLUMNS; c++)
        {
            if (board[c, ROWS - 1] == 0)
                return false;
        }
        return true;
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

    const int WIN_SCORE = 1000;
    const int THREE_OPEN = 100;
    const int TWO_OPEN = 10;
    const int CENTER_WEIGHT = 6;

    int ScoreWindow(int[] window, int player)
    {
        int opp = -player;
        int aiCount = 0, oppCount = 0, emptyCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (window[i] == player) aiCount++;
            else if (window[i] == opp) oppCount++;
            else emptyCount++;
        }

        if (aiCount > 0 && oppCount > 0) return 0;

        // 4 en raya
        if (aiCount == 4) return WIN_SCORE;
        if (oppCount == 4) return -WIN_SCORE;

        // 3 en raya con hueco
        if (aiCount == 3 && emptyCount == 1) return THREE_OPEN;
        if (oppCount == 3 && emptyCount == 1) return -THREE_OPEN;

        // 2 en raya con 2 huecos
        if (aiCount == 2 && emptyCount == 2) return TWO_OPEN;
        if (oppCount == 2 && emptyCount == 2) return -TWO_OPEN;

        return 0;
    }

    int Evaluate(int[,] board, int player)
    {
        // Si alguien ha ganado
        if (CheckWin(board, player))
            return WIN_SCORE * 10;
        if (CheckWin(board, -player))
            return -WIN_SCORE * 10;

        int score = 0;

        // Controlar el centro
        int centerCol = COLUMNS / 2;
        for (int r = 0; r < ROWS; r++)
        {
            if (board[centerCol, r] == player) score += CENTER_WEIGHT;
            else if (board[centerCol, r] == -player) score -= CENTER_WEIGHT;
        }

        // Horizontal
        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLUMNS - 3; c++)
            {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++)
                    window[i] = board[c + i, r];
                score += ScoreWindow(window, player);
            }
        }

        // Vertical
        for (int c = 0; c < COLUMNS; c++)
        {
            for (int r = 0; r < ROWS - 3; r++)
            {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++)
                    window[i] = board[c, r + i];
                score += ScoreWindow(window, player);
            }
        }

        // Diagonal /
        for (int c = 0; c < COLUMNS - 3; c++)
        {
            for (int r = 0; r < ROWS - 3; r++)
            {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++)
                    window[i] = board[c + i, r + i];
                score += ScoreWindow(window, player);
            }
        }

        // Diagonal \
        for (int c = 0; c < COLUMNS - 3; c++)
        {
            for (int r = 3; r < ROWS; r++)
            {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++)
                    window[i] = board[c + i, r - i];
                score += ScoreWindow(window, player);
            }
        }

        return score;
    }
}
