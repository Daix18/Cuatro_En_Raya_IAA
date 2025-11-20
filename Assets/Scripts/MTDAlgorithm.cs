using System;
using System.Collections.Generic;
using UnityEngine;

// --- EL STRUCT---//
public struct ScoringMove
{
    public int Move;
    public int Score;

    public ScoringMove(int _move, int _score)
    {
        Move = _move;
        Score = _score;
    }
}


public class MTDAlgorithm
{
    // CONSTANTES
    const int ROWS = 6;
    const int COLUMNS = 7;
    public long NodesVisited { get; private set; }
    // NOTA: Estas constantes viejas ya no se usan tanto porque la lógica nueva 
    // usa números directos en ScoreWindow, pero las dejamos para evitar errores.
    const int WIN_SCORE = 100000;

    // --- NUEVA TABLA DE EVALUACIÓN (Heatmap) ---
    // Esto le dice a la IA qué casillas son estratégicamente mejores por defecto.
    private readonly int[,] _evaluationTable = new int[7, 6]
    {
            {3, 4, 5, 7, 5, 4}, // Columna 0
            {4, 6, 8, 10, 8, 6}, // Columna 1
            {5, 8, 11, 13, 11, 8}, // Columna 2
            {7, 10, 13, 16, 13, 10}, // Columna 3 (Centro)
            {5, 8, 11, 13, 11, 8}, // Columna 4
            {4, 6, 8, 10, 8, 6}, // Columna 5
            {3, 4, 5, 7, 5, 4}  // Columna 6
    };

    private int _maximumExploredDepth = 0;
    private TranspositionTable _transpositionTable;
    private ZobristKey _keys;
    private int _globalGuess = 0;

    public const int MaxIterations = 10;
    private int MaxDepth;

    public MTDAlgorithm()
    {
        _keys = new ZobristKey();
        _transpositionTable = new TranspositionTable();
    }


   //Metodo de acceso para el gamemanager

    public int GetBestMove(int[,] board, int SearchDepth, int initialPlayer)
    {
        NodesVisited = 0;
        // 1. Reiniciar variables si es necesario
        _maximumExploredDepth = 0;
        MaxDepth = SearchDepth;
        // 2. Llamar al algoritmo MTD
        int? result = MTD(board, initialPlayer);

        // 3. Si mtd devuelve un movimiento
        if (result.HasValue)
        {
            return result.Value;
        }
        else
        {
            // -1 indica error mtd no a encontrado un movimiento o bien tablero lleno
            return -1;
        }
    }

   

    // --- LÓGICA MTD(f) (Sin cambios aquí) ---
    ScoringMove Test(int[,] board, int depth, int gamma, int initialPlayer)
    {
        NodesVisited++;
        int bestMove = 0, bestScore = int.MinValue;
        ScoringMove scoringMove = new ScoringMove();
       int currentPlayer = (depth % 2 == 0) ? initialPlayer : -initialPlayer;

        if (depth > _maximumExploredDepth) _maximumExploredDepth = depth;

        var found = _transpositionTable.TryGetValue(_keys.HashValue(board), out var record);

        if (found && record.depth >= MaxDepth - depth)
        {
            if (record.minScore > gamma) return new ScoringMove(record.bestMove, record.minScore);
            if (record.maxScore < gamma) return new ScoringMove(record.bestMove, record.maxScore);
        }
        else if (!found)
        {
            record = new BoardRecord
            {
                hashValue = _keys.HashValue(board),
                depth = MaxDepth - depth,
                minScore = int.MinValue,
                maxScore = int.MaxValue
            };
        }

        if (IsTerminal(board) || depth == MaxDepth)
        {
            int eval = Evaluate(board, currentPlayer);
            record.maxScore = eval;
            record.minScore = eval;
            _transpositionTable[record.hashValue] = record;
            return new ScoringMove(-1, eval);
        }

        List<int> moves = GetOrderedMoves(board);
        if (moves.Count == 0) return new ScoringMove(-1, 0);

        foreach (int move in moves)
        {
            ApplyMove(board, move, currentPlayer);
            scoringMove = Test(board, depth + 1, -gamma, initialPlayer);
            int invertedScore = -scoringMove.Score;
            UndoMove(board, move);

            if (invertedScore > bestScore)
            {
                bestScore = invertedScore;
                bestMove = move;
            }
        }

        if (bestScore < gamma) record.maxScore = bestScore;
        else record.minScore = bestScore;

        record.bestMove = bestMove;
        _transpositionTable[record.hashValue] = record;

        return new ScoringMove(bestMove, bestScore);
    }

    //Metodo de inicializacion de mtd

    public int? MTD(int[,] board, int initialPlayer)
    {
        int gamma, guess = _globalGuess;
        ScoringMove scoringMove = new ScoringMove(-1, 0);//inicializado a mov -1 no valido
        _maximumExploredDepth = 0;

        for (int i = 0; i < MaxIterations; i++)
        {
            gamma = guess;
            scoringMove = Test(board, 0, gamma - 1, initialPlayer);
            guess = scoringMove.Score;
            if (gamma == guess)
            {
                _globalGuess = guess;
                return scoringMove.Move;
            }
        }
        _globalGuess = guess;
        return scoringMove.Move;
    }


    // --- UTILIDADES PRIVADAS ---
    private bool IsColumnPlayable(int[,] board, int col) => board[col, ROWS - 1] == 0;

    private void ApplyMove(int[,] board, int col, int player)
    {
        for (int r = 0; r < ROWS; r++)
            if (board[col, r] == 0) { board[col, r] = player; return; }
    }

    private void UndoMove(int[,] board, int col)
    {
        for (int r = ROWS - 1; r >= 0; r--)
            if (board[col, r] != 0) { board[col, r] = 0; return; }
    }

    private List<int> GetOrderedMoves(int[,] board)
    {
        List<int> moves = new List<int>();
        // Orden optimizado: Centro primero (columna 3), luego lados
        int[] order = { 3, 2, 4, 1, 5, 0, 6 };
        foreach (int c in order) if (IsColumnPlayable(board, c)) moves.Add(c);
        return moves;
    }

    private bool IsTerminal(int[,] board) => CheckWin(board, 1) || CheckWin(board, -1) || IsBoardFull(board);

    private bool IsBoardFull(int[,] board)
    {
        for (int c = 0; c < COLUMNS; c++) if (board[c, ROWS - 1] == 0) return false;
        return true;
    }

    private bool CheckWin(int[,] b, int p)
    {
        // Verificación rápida de victoria
        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 0; r < ROWS; r++)
                if (b[c, r] == p && b[c + 1, r] == p && b[c + 2, r] == p && b[c + 3, r] == p) return true;
        for (int c = 0; c < COLUMNS; c++)
            for (int r = 0; r < ROWS - 3; r++)
                if (b[c, r] == p && b[c, r + 1] == p && b[c, r + 2] == p && b[c, r + 3] == p) return true;
        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 0; r < ROWS - 3; r++)
                if (b[c, r] == p && b[c + 1, r + 1] == p && b[c + 2, r + 2] == p && b[c + 3, r + 3] == p) return true;
        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 3; r < ROWS; r++)
                if (b[c, r] == p && b[c + 1, r - 1] == p && b[c + 2, r - 2] == p && b[c + 3, r - 3] == p) return true;
        return false;
    }

    // ==========================================
    //      AQUÍ ESTÁN LAS NUEVAS FUNCIONES
    // ==========================================

    // --- EVALUACIÓN OPTIMIZADA ---
    private int Evaluate(int[,] board, int player)
    {
        int score = 0;

        // 1. Puntuación Posicional (Heatmap)
        for (int c = 0; c < COLUMNS; c++)
            for (int r = 0; r < ROWS; r++)
                if (board[c, r] == player) score += _evaluationTable[c, r];
                else if (board[c, r] == -player) score -= _evaluationTable[c, r];

        // 2. Análisis de Ventanas (Pasando coordenadas, NO arrays)

        // Horizontal (-)
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLUMNS - 3; c++)
                score += ScoreWindow(board, c, r, 1, 0, player);

        // Vertical (|)
        for (int c = 0; c < COLUMNS; c++)
            for (int r = 0; r < ROWS - 3; r++)
                score += ScoreWindow(board, c, r, 0, 1, player);

        // Diagonal Ascendente (/)
        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 0; r < ROWS - 3; r++)
                score += ScoreWindow(board, c, r, 1, 1, player);

        // Diagonal Descendente (\)
        for (int c = 0; c < COLUMNS - 3; c++)
            for (int r = 3; r < ROWS; r++) // Nota: empieza en r=3 porque va hacia abajo
                score += ScoreWindow(board, c, r, 1, -1, player);

        return score;
    }

    // Ahora recibe el tablero y direcciones, no crea arrays nuevos
    private int ScoreWindow(int[,] board, int c0, int r0, int dc, int dr, int player)
    {
        int opp = -player;
        int myCount = 0;
        int oppCount = 0;
        int emptyCount = 0;

        // Recorremos las 4 celdas virtualmente
        for (int i = 0; i < 4; i++)
        {
            int c = c0 + (i * dc);
            int r = r0 + (i * dr);
            int cell = board[c, r];

            if (cell == player) myCount++;
            else if (cell == opp) oppCount++;
            else emptyCount++;
        }

        // --- Lógica de Puntuación ---

        if (myCount == 4) return 100000;
        if (oppCount == 4) return -100000;

        // Ventana mixta (sucia), no vale nada
        if (myCount > 0 && oppCount > 0) return 0;

        int score = 0;

        // Ataque
        if (myCount == 3 && emptyCount == 1) score += 100;
        else if (myCount == 2 && emptyCount == 2) score += 10;

        // Defensa (Bloqueo)
        // NOTA: Penalizamos fuertemente dejar ganar al rival
        if (oppCount == 3 && emptyCount == 1) score -= 8000;

        return score;
    }
}

