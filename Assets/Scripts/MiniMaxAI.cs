using UnityEngine;
using System;
using System.Collections.Generic;

public class MiniMaxAI
{
    public long NodesVisited { get; private set; }

    const int ROWS = 6;
    const int COLUMNS = 7;


    // --- MÉTODO RAÍZ ---
    public int GetBestMove(int[,] board, int depth, int aiPlayer)
    {
        NodesVisited = 0;
        int bestMove = -1; // Inicializar a -1 o un valor inválido para detectar errores
        int bestScore = int.MinValue;

        // Configuración inicial de Alpha-Beta
        int alpha = int.MinValue;
        int beta = int.MaxValue;

        List<int> moves = GetOrderedMoves(board);

        foreach (int move in moves)
        {
            if (!IsColumnPlayable(board, move))
                continue;

            // 1. Aplicamos el movimiento de la IA (aiPlayer)
            ApplyMove(board, move, aiPlayer);

            // 2. Llamamos a la recursión. 
            // IMPORTANTE: Pasamos -aiPlayer como 'currentPlayer' porque ahora le toca al rival.
            // 'aiPlayer' se pasa fijo al final para saber quién es el MAXimizador.
            int score = MiniMax(board, depth - 1, alpha, beta, -aiPlayer, aiPlayer);

            // 3. Deshacemos
            UndoMove(board, move);

            // 4. Lógica de Maximización (La raíz siempre es MAX)
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            // Actualizamos Alpha
            alpha = Math.Max(alpha, bestScore);
        }

        Debug.Log($"MiniMax visitó {NodesVisited} nodos. Mejor movimiento: {bestMove} Score: {bestScore}");
        return bestMove;
    }

    // --- MÉTODO RECURSIVO ---
    private int MiniMax(int[,] board, int depth, int alpha, int beta, int currentPlayer, int aiPlayer)
    {
        NodesVisited++;

        // Caso terminal (Debes tener IsTerminal implementado para detectar victorias)
        if (depth == 0 || IsTerminal(board))
        {
            // Pasamos aiPlayer para evaluar desde SU perspectiva
            return Evaluate(board, aiPlayer);
        }

        // ¿Es turno de la IA (Max) o del Rival (Min)?
        bool isMaximizing = (currentPlayer == aiPlayer);

        // Inicializamos bestScore con el peor valor posible para cada caso
        int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

        List<int> moves = GetOrderedMoves(board);

        foreach (int move in moves)
        {
            if (!IsColumnPlayable(board, move))
                continue;

            // --- CORRECCIÓN CLAVE AQUÍ ---
            // Usamos 'currentPlayer', NO 'aiPlayer'. 
            // Si estamos en nivel MAX, pone ficha la IA. Si estamos en MIN, pone ficha el rival.
            ApplyMove(board, move, currentPlayer);

            // Llamada recursiva:
            // Invertimos el jugador actual: -currentPlayer
            // Mantenemos aiPlayer fijo.
            int score = MiniMax(board, depth - 1, alpha, beta, -currentPlayer, aiPlayer);

            UndoMove(board, move);

            if (isMaximizing)
            {
                // Turno de MAX
                if (score > bestScore)
                    bestScore = score;
                alpha = Math.Max(alpha, bestScore);

                if (beta <= alpha)
                    break; // Poda Beta
            }
            else
            {
                // Turno de MIN
                if (score < bestScore)
                    bestScore = score;
                beta = Math.Min(beta, bestScore);

                if (beta <= alpha)
                    break; // Poda Alfa
            }
        }

        return bestScore;
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


    const int THREE_OPEN = 100;
    const int TWO_OPEN = 10;
    const int CENTER_WEIGHT = 6;

    private int Evaluate(int[,] board, int aiPlayer)
    {
        // El oponente es el valor opuesto (Consistente con 1 y -1)
        int oppPlayer = -aiPlayer;

        int score = 0;

        // --- LÓGICA DE LA FUNCIÓN AUXILIAR (ScoreWindow) ---
        int ScoreWindow(int r0, int c0, int dr, int dc)
        {
            int aiCount = 0, oppCount = 0, emptyCount = 0;

            for (int i = 0; i < 4; i++)
            {
                int r = r0 + i * dr;
                int c = c0 + i * dc;

                // Acceso correcto: board[columna, fila]
                int cellValue = board[c, r];

                if (cellValue == aiPlayer) aiCount++;
                else if (cellValue == oppPlayer) oppCount++;
                else emptyCount++;
            }

            if (aiCount > 0 && oppCount > 0) return 0;
            if (aiCount == 4) return int.MaxValue;
            if (oppCount == 4) return int.MinValue;

            if (aiCount == 3 && emptyCount == 1) return THREE_OPEN;
            if (oppCount == 3 && emptyCount == 1) return -THREE_OPEN;

            if (aiCount == 2 && emptyCount == 2) return TWO_OPEN;
            if (oppCount == 2 && emptyCount == 2) return -TWO_OPEN;

            return 0;
        }
        // --- FIN DE LA FUNCIÓN AUXILIAR ---


        // 1. Puntuación del Centro (Prioridad)
        int centerCol = COLUMNS / 2; // Columna 3
        for (int r = 0; r < ROWS; r++)
        {
            if (board[centerCol, r] == aiPlayer) score += CENTER_WEIGHT;
        }


        // 2. Evaluar Horizontal (dr=0, dc=1)
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLUMNS - 3; c++)
                score += ScoreWindow(r, c, 0, 1);

        // 3. Evaluar Vertical (dr=1, dc=0)
        for (int c = 0; c < COLUMNS; c++)
            for (int r = 0; r < ROWS - 3; r++)
                score += ScoreWindow(r, c, 1, 0);

        // 4. Evaluar Diagonal Ascendente (/) (dr=1, dc=1)
        // Empieza de abajo a la izquierda (r=0, c=0) y se mueve UP/RIGHT
        for (int r = 0; r < ROWS - 3; r++)
            for (int c = 0; c < COLUMNS - 3; c++)
                score += ScoreWindow(r, c, 1, 1); // dr=1, dc=1

        // 5. Evaluar Diagonal Descendente (\) (dr=-1, dc=1)
        // Empieza de arriba a la izquierda (r=3, c=0) y se mueve DOWN/RIGHT
        for (int r = 3; r < ROWS; r++)
            for (int c = 0; c < COLUMNS - 3; c++)
                score += ScoreWindow(r, c, -1, 1); // dr=-1, dc=1

        return score;
    }

}
