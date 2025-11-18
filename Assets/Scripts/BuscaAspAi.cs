using System.Collections.Generic;
using UnityEngine;

public class BuscaAspAi
{
    private const int INF = 999999;
    public long NodesVisited = 0;

    public int GetBestMove(int[,] board, int depth, int player)
    {
        NodesVisited = 0;

        int bestMove = -1;
        int bestScore = -INF;

        // Guess inicial: evaluación simple (0 funciona bien también)
        int guess = 0;

        List<int> validMoves = GetValidMoves(board);

        foreach (int move in validMoves)
        {
            int[,] next = SimulateMove(board, move, player);

            int score = -AspirationSearch(next, depth - 1, -player, guess);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int AspirationSearch(int[,] board, int depth, int player, int guess)
    {
        int window = 10; // ventana aspiracional
        int alpha = guess - window;
        int beta = guess + window;

        int score = Negamax(board, depth, alpha, beta, player);

        // Si la estimación falla → búsqueda completa
        if (score <= alpha || score >= beta)
        {
            score = Negamax(board, depth, -INF, INF, player);
        }

        return score;
    }

    private int Negamax(int[,] board, int depth, int alpha, int beta, int player)
    {
        NodesVisited++;

        if (depth == 0 || IsTerminal(board))
            return Evaluate(board, player);

        int maxEval = -INF;

        foreach (int move in GetValidMoves(board))
        {
            int[,] next = SimulateMove(board, move, player);
            int eval = -Negamax(next, depth - 1, -beta, -alpha, -player);

            if (eval > maxEval)
                maxEval = eval;

            alpha = Mathf.Max(alpha, eval);

            if (alpha >= beta)
                break; // poda
        }

        return maxEval;
    }

    private bool IsTerminal(int[,] board)
    {
        return CheckWin(board, 1) || CheckWin(board, -1) || GetValidMoves(board).Count == 0;
    }

    private int Evaluate(int[,] b, int player)
    {
        // Heurística simple (funciona bien con aspiracional)
        int score = 0;

        // Cuenta fichas del jugador - fichas del oponente
        for (int c = 0; c < GameManager.COLUMNS; c++)
            for (int r = 0; r < GameManager.ROWS; r++)
                score += b[c, r] * player;

        return score;
    }

    private List<int> GetValidMoves(int[,] board)
    {
        List<int> moves = new List<int>();
        for (int c = 0; c < GameManager.COLUMNS; c++)
            if (board[c, GameManager.ROWS - 1] == 0)
                moves.Add(c);

        return moves;
    }

    private int[,] SimulateMove(int[,] board, int column, int player)
    {
        int[,] newBoard = board.Clone() as int[,];

        for (int r = 0; r < GameManager.ROWS; r++)
        {
            if (newBoard[column, r] == 0)
            {
                newBoard[column, r] = player;
                break;
            }
        }

        return newBoard;
    }

    private bool CheckWin(int[,] b, int player)
    {
        int C = GameManager.COLUMNS;
        int R = GameManager.ROWS;

        // Horizontal
        for (int c = 0; c < C - 3; c++)
            for (int r = 0; r < R; r++)
                if (b[c, r] == player && b[c + 1, r] == player &&
                    b[c + 2, r] == player && b[c + 3, r] == player)
                    return true;

        // Vertical
        for (int c = 0; c < C; c++)
            for (int r = 0; r < R - 3; r++)
                if (b[c, r] == player && b[c, r + 1] == player &&
                    b[c, r + 2] == player && b[c, r + 3] == player)
                    return true;

        // Diagonal /
        for (int c = 0; c < C - 3; c++)
            for (int r = 0; r < R - 3; r++)
                if (b[c, r] == player && b[c + 1, r + 1] == player &&
                    b[c + 2, r + 2] == player && b[c + 3, r + 3] == player)
                    return true;

        // Diagonal \
        for (int c = 0; c < C - 3; c++)
            for (int r = 3; r < R; r++)
                if (b[c, r] == player && b[c + 1, r - 1] == player &&
                    b[c + 2, r - 2] == player && b[c + 3, r - 3] == player)
                    return true;

        return false;
    }
}
