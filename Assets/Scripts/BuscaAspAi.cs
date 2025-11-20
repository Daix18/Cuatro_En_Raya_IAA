using System;
using System.Collections.Generic;
using UnityEngine;

public class BuscaAspAI
{
    public int defaultMaxDepth = 6;
    public int nodeCountLimit = 200000;
    public int initialWindow = 50;
    public bool useIterativeDeepening = true;

    public long NodesVisited { get; private set; }

    const int ROWS = 6;
    const int COLUMNS = 7;

    // ===========================================================
    //                      API PRINCIPAL
    // ===========================================================
    public int GetBestMove(int[,] board, int maxDepth, int player)
    {
        if (maxDepth <= 0)
            maxDepth = defaultMaxDepth;

        int bestMove = -1;
        int bestScore = 0;

        NodesVisited = 0;

        int startDepth = useIterativeDeepening ? 1 : maxDepth;

        for (int depth = startDepth; depth <= maxDepth; depth++)
        {
            int guess = bestMove == -1 ? 0 : bestScore;
            int window = initialWindow;

            int alpha = guess - window;
            int beta = guess + window;

            int resultScore = int.MinValue;
            int resultMove = -1;
            bool finished = false;

            while (!finished)
            {
                NodesVisited = 0;

                resultScore = NegamaxRoot(board, player, depth, alpha, beta, out resultMove);

                if (resultScore <= alpha)
                {
                    alpha -= window;
                    window *= 2;
                    if (alpha < -1000000) alpha = int.MinValue / 4;
                }
                else if (resultScore >= beta)
                {
                    beta += window;
                    window *= 2;
                    if (beta > 1000000) beta = int.MaxValue / 4;
                }
                else
                {
                    finished = true;
                }

                if (NodesVisited > nodeCountLimit)
                    finished = true;
            }

            if (resultMove != -1)
            {
                bestMove = resultMove;
                bestScore = resultScore;
            }
        }

        return bestMove;
    }


    // ===========================================================
    //                  NEGAMAX ROOT
    // ===========================================================
    private int NegamaxRoot(int[,] board, int player, int depth, int alpha, int beta, out int bestMove)
    {
        bestMove = -1;
        int bestScore = int.MinValue;

        List<int> moves = GetLegalMoves(board);
        OrderMovesCenterFirst(moves);

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, player);
            if (row == -1)
                continue;

            int score;

            if (CheckWin(board, player))
            {
                score = 1_000_000;
            }
            else
            {
                score = -Negamax(board, -player, depth - 1, -beta, -alpha);
            }

            UndoMove(board, col);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }

            alpha = Math.Max(alpha, score);
            if (alpha >= beta)
                break;
        }

        return bestScore;
    }


    // ===========================================================
    //                        NEGAMAX
    // ===========================================================
    private int Negamax(int[,] board, int player, int depth, int alpha, int beta)
    {
        NodesVisited++;
        if (NodesVisited > nodeCountLimit)
            return 0;

        if (depth == 0 || IsTerminal(board))
            return Evaluate(board, player);

        List<int> moves = GetLegalMoves(board);
        if (moves.Count == 0) return 0;

        OrderMovesCenterFirst(moves);

        int best = int.MinValue;

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, player);
            if (row == -1) continue;

            int score;

            if (CheckWin(board, player))
            {
                score = 1_000_000 / (defaultMaxDepth - depth + 1);
            }
            else
            {
                score = -Negamax(board, -player, depth - 1, -beta, -alpha);
            }

            UndoMove(board, col);

            best = Math.Max(best, score);
            alpha = Math.Max(alpha, score);
            if (alpha >= beta)
                break;
        }

        return best;
    }


    // ===========================================================
    //                  UTILIDADES DEL TABLERO
    // ===========================================================
    private List<int> GetLegalMoves(int[,] board)
    {
        List<int> moves = new();
        for (int c = 0; c < COLUMNS; c++)
            if (board[c, ROWS - 1] == 0)
                moves.Add(c);
        return moves;
    }

    private int MakeMove(int[,] board, int column, int player)
    {
        for (int r = 0; r < ROWS; r++)
        {
            if (board[column, r] == 0)
            {
                board[column, r] = player;
                return r;
            }
        }
        return -1;
    }

    private void UndoMove(int[,] board, int column)
    {
        for (int r = ROWS - 1; r >= 0; r--)
        {
            if (board[column, r] != 0)
            {
                board[column, r] = 0;
                return;
            }
        }
    }

    private void OrderMovesCenterFirst(List<int> moves)
    {
        moves.Sort((a, b) =>
            Math.Abs(3 - a).CompareTo(Math.Abs(3 - b))
        );
    }

    private bool IsTerminal(int[,] board)
    {
        return CheckWin(board, 1) || CheckWin(board, -1) || IsBoardFull(board);
    }

    private bool IsBoardFull(int[,] board)
    {
        for (int c = 0; c < COLUMNS; c++)
            if (board[c, ROWS - 1] == 0)
                return false;
        return true;
    }


    // ===========================================================
    //                       CHECK WIN
    // ===========================================================
    private bool CheckWin(int[,] b, int p)
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

    // ===========================================================
    //                       EVALUACION
    // ===========================================================
    private int Evaluate(int[,] board, int perspective)
    {
        int score = 0;

        int center = COLUMNS / 2;
        int countCenter = 0;

        for (int r = 0; r < ROWS; r++)
            if (board[center, r] == perspective) countCenter++;

        score += countCenter * 3;

        for (int c = 0; c < COLUMNS; c++)
        {
            for (int r = 0; r < ROWS; r++)
            {
                if (c + 3 < COLUMNS)
                    score += Window(board, c, r, 1, 0, perspective);

                if (r + 3 < ROWS)
                    score += Window(board, c, r, 0, 1, perspective);

                if (c + 3 < COLUMNS && r + 3 < ROWS)
                    score += Window(board, c, r, 1, 1, perspective);

                if (c + 3 < COLUMNS && r - 3 >= 0)
                    score += Window(board, c, r, 1, -1, perspective);
            }
        }

        return score;
    }

    private int Window(int[,] board, int sc, int sr, int dc, int dr, int perspective)
    {
        int me = 0, opp = 0, empty = 0;

        for (int i = 0; i < 4; i++)
        {
            int c = sc + dc * i;
            int r = sr + dr * i;

            int cell = board[c, r];

            if (cell == perspective) me++;
            else if (cell == -perspective) opp++;
            else empty++;
        }

        if (me == 4) return 10000;
        if (me == 3 && empty == 1) return 100;
        if (me == 2 && empty == 2) return 10;

        if (opp == 3 && empty == 1) return -80;
        if (opp == 2 && empty == 2) return -5;

        return 0;
    }
}
