using System;
using System.Collections.Generic;

public class NegaScoutAI
{
    public long NodesVisited { get; private set; }

    const int ROWS = 6;
    const int COLUMNS = 7;

    public int GetBestMove(int[,] board, int depth, int player)
    {
        NodesVisited = 0;
        int bestMove = -1;
        int alpha = int.MinValue + 1;
        int beta = int.MaxValue - 1;

        // Orden simple de movimientos: centro primero, luego alrededores
        List<int> moves = GetOrderedMoves(board);

        foreach (int move in moves)
        {
            if (!IsColumnPlayable(board, move))
                continue;

            ApplyMove(board, move, player);
            int score = -NegaScout(board, depth - 1, -beta, -alpha, -player);
            UndoMove(board, move);

            if (score > alpha)
            {
                alpha = score;
                bestMove = move;
            }
        }

        //Debug.Log($"NegaScout visitó {NodesVisited} nodos.");
        return bestMove;
    }

    private int NegaScout(int[,] board, int depth, int alpha, int beta, int player)
    {
        NodesVisited++;

        if (depth == 0 || IsTerminal(board))
        {
            return Evaluate(board, player);
        }

        int a = alpha;
        int b = beta;
        bool firstChild = true;

        List<int> moves = GetOrderedMoves(board);

        foreach (int move in moves)
        {
            if (!IsColumnPlayable(board, move))
                continue;

            ApplyMove(board, move, player);

            int score;
            if (firstChild)
            {
                // Búsqueda completa al primer hijo
                score = -NegaScout(board, depth - 1, -b, -a, -player);
                firstChild = false;
            }
            else
            {
                // Null-window search
                score = -NegaScout(board, depth - 1, -a - 1, -a, -player);

                // Si entra en la ventana real, re-search
                if (a < score && score < beta)
                {
                    score = -NegaScout(board, depth - 1, -beta, -score, -player);
                }
            }

            UndoMove(board, move);

            if (score > a)
            {
                a = score;
            }

            if (a >= beta)
            {
                return a; // poda
            }

            b = a + 1; // actualiza null-window
        }

        return a;
    }

    // ------------------- Utilidades de tablero -------------------

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
        List<int> moves = new List<int>();
        for (int c = 0; c < COLUMNS; c++)
        {
            moves.Add(c);
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

    int ScoreWindow(int[] window, int player)
    {
        int opp = -player;
        int playerCount = 0;
        int oppCount = 0;
        int emptyCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (window[i] == player) playerCount++;
            else if (window[i] == opp) oppCount++;
            else emptyCount++;
        }

        int s = 0;

        // Configuraciones buenas para el jugador
        if (playerCount == 3 && emptyCount == 1) s += 50;      // 3 en línea con hueco
        else if (playerCount == 2 && emptyCount == 2) s += 10; // 2 en línea

        // Configuraciones peligrosas del rival (las penalizamos más)
        if (oppCount == 3 && emptyCount == 1) s -= 80;         // bloquear 3 del rival
        else if (oppCount == 2 && emptyCount == 2) s -= 10;

        return s;
    }

    // --- Heurística sencilla para empezar ---
    int Evaluate(int[,] board, int player)
    {
        // Si alguien ha ganado, valor grande / pequeño
        if (CheckWin(board, player))
            return 100000;
        if (CheckWin(board, -player))
            return -100000;

        // Heurística muy básica: controlar el centro
        int score = 0;

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
