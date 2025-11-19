using System;
using System.Collections.Generic;
using UnityEngine;
/* using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Linq;
using System.Text;
using System.Threading.Tasks; */

public class MTDAlgorithm
{
    private const int INF = 999999;
    public long NodesVisited = 0;

    private int maxDepth;

    public int GetBestMove(int[,] board, int depth, int player)
    {
        NodesVisited = 0;
        maxDepth = depth;

        int guess = 0;
        int bestMove;

        int result = MTD(board, player, guess, depth, out bestMove);

        Debug.Log($"[MTD(f)] BestMove={bestMove}, Score={result}, Nodes={NodesVisited}");
        return bestMove;
    }

    // ============================================================
    //                   MTD(f) ALGORITHM
    // ============================================================
    private int MTD(int[,] board, int player, int firstGuess, int depth, out int bestMove)
    {
        int lowerBound = -INF;
        int upperBound = INF;
        int guess = firstGuess;

        bestMove = -1;

        while (lowerBound < upperBound)
        {
            int beta = (guess == lowerBound) ? guess + 1 : guess;
            int score = NegamaxRoot(board, player, depth, beta - 1, beta, out bestMove);

            if (score < beta) upperBound = score;
            else lowerBound = score;

            guess = score;
        }

        return guess;
    }

    // ============================================================
    //          ROOT NEGAMAX — RETURNS BEST MOVE
    // ============================================================
    private int NegamaxRoot(int[,] board, int player, int depth, int alpha, int beta, out int bestMove)
    {
        bestMove = -1;
        int bestScore = -INF;

        List<int> moves = GetValidMoves(board);

        // Order moves: try central columns first (improves pruning)
        moves.Sort((a, b) =>
            Math.Abs(b - (GameManager.COLUMNS / 2)).CompareTo(
            Math.Abs(a - (GameManager.COLUMNS / 2)))
        );

        foreach (int col in moves)
        {
            int[,] next = SimulateMove(board, col, player);
            int score;

            if (CheckWin(next, player))
            {
                score = 1000000; // winning move
            }
            else
            {
                score = -Negamax(next, -player, depth - 1, -beta, -alpha);
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }

            alpha = Math.Max(alpha, score);
            if (alpha >= beta)
                break; // pruning
        }

        return bestScore;
    }

    // ============================================================
    //         NEGAMAX + ALFA-BETA
    // ============================================================
    private int Negamax(int[,] board, int player, int depth, int alpha, int beta)
    {
        NodesVisited++;

        if (depth == 0 || IsTerminal(board))
            return Evaluate(board, player);

        List<int> moves = GetValidMoves(board);
        if (moves.Count == 0) return 0;

        moves.Sort((a, b) =>
            Math.Abs(b - (GameManager.COLUMNS / 2)).CompareTo(
            Math.Abs(a - (GameManager.COLUMNS / 2))));

        int best = -INF;

        foreach (int col in moves)
        {
            int[,] next = SimulateMove(board, col, player);
            int score;

            if (CheckWin(next, player))
            {
                score = 1000000 / ((maxDepth - depth) + 1);
            }
            else
            {
                score = -Negamax(next, -player, depth - 1, -beta, -alpha);
            }

            if (score > best)
                best = score;

            alpha = Math.Max(alpha, score);
            if (alpha >= beta)
                break;
        }

        return best;
    }

    // ============================================================
    //               HELPER FUNCTIONS FOR YOUR BOARD
    // ============================================================
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

    private bool IsTerminal(int[,] board)
    {
        return CheckWin(board, 1) || CheckWin(board, -1) || GetValidMoves(board).Count == 0;
    }

    // ============================================================
    //          WIN CHECK (copied from your GameManager)
    // ============================================================
    private bool CheckWin(int[,] b, int player)
    {
        int C = GameManager.COLUMNS;
        int R = GameManager.ROWS;

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

    // ============================================================
    //                   EVALUATION FUNCTION
    // ============================================================
    private int Evaluate(int[,] board, int player)
    {
        int score = 0;

        // Bonus por controlar el centro
        int center = GameManager.COLUMNS / 2;
        for (int r = 0; r < GameManager.ROWS; r++)
            if (board[center, r] == player) score += 3;

        // Ventanas de 4
        for (int c = 0; c < GameManager.COLUMNS; c++)
        {
            for (int r = 0; r < GameManager.ROWS; r++)
            {
                if (c + 3 < GameManager.COLUMNS)
                    score += EvaluateWindow(board, c, r, 1, 0, player);
                if (r + 3 < GameManager.ROWS)
                    score += EvaluateWindow(board, c, r, 0, 1, player);
                if (c + 3 < GameManager.COLUMNS && r + 3 < GameManager.ROWS)
                    score += EvaluateWindow(board, c, r, 1, 1, player);
                if (c + 3 < GameManager.COLUMNS && r - 3 >= 0)
                    score += EvaluateWindow(board, c, r, 1, -1, player);
            }
        }

        return score;
    }

    private int EvaluateWindow(int[,] board, int c, int r, int dc, int dr, int player)
    {
        int my = 0, opp = 0, empty = 0;

        for (int i = 0; i < 4; i++)
        {
            int val = board[c + dc * i, r + dr * i];
            if (val == player) my++;
            else if (val == 0) empty++;
            else opp++;
        }

        if (my == 4) return 10000;
        if (my == 3 && empty == 1) return 100;
        if (my == 2 && empty == 2) return 10;

        if (opp == 3 && empty == 1) return -80;
        if (opp == 2 && empty == 2) return -5;

        return 0;
    }
}
