using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MTDAlgorithm
{
    //    // Variable para depuración (saber qué tan profundo llegó realmente)
    //    private int _maximumExploredDepth = 0;

    //    // La Memoria Caché (donde guardamos posiciones ya calculadas)
    //    private TranspositionTable _transpositionTable;

    //    // El generador de IDs únicos para los tableros
    //    private ZobristKey _keys;

    //    // _globalGuess: La "intuición" de la IA.
    //    // MTD(f) basado en resultado del turno anterior como punto de partida para este nuevo turno.
    //    // Esto hace que la búsqueda sea mucho más rápida.
    //    private int _globalGuess = int.MaxValue;

    //    // Límite de intentos para adivinar  (evita bucles infinitos)
    //    public const int MaxIterations = 10;

    //    // Profundidad de búsqueda:  7 movimientos en el futuro.
    //    public const int MaxDepth = 7;

    //    // Constructor: Inicializa las herramientas (llaves y tabla vacía)
    //    public MTDAlgorithm()
    //    {
    //        _keys = new ZobristKey();
    //        _transpositionTable = new TranspositionTable();
    //    }


    //ScoringMove Test(GameState board, int depth, int gamma)
    //{
    //    int bestMove, bestScore;
    //    GameState newBoard;
    //    ScoringMove scoringMove = new ScoringMove();

    //    // Actualizamos la estadística de profundidad (solo informativo)
    //    if (depth > _maximumExploredDepth) _maximumExploredDepth = depth;

    //    // --- PASO 1: CONSULTAR LA MEMORIA ---

    //    // Calculamos el Hash Zobrist y buscamos en la tabla si ya estuvimos aquí
    //    var found = _transpositionTable.TryGetValue(_keys.HashValue(board), out var record);

    //    if (found) // ¡Lo encontramos!
    //    {
    //        // Verificamos si el registro guardado es lo suficientemente profundo.
    //        // Si necesitamos mirar 5 pasos adelante, pero el registro solo miró 2, no nos sirve.
    //        if (record.depth > MaxDepth - depth)
    //        {
    //            // Lógica de MTD/Alpha-Beta:
    //            // Si el peor escenario guardado ya es mejor que gamma, cortamos y devolvemos eso.
    //            if (record.minScore > gamma)
    //            {
    //                scoringMove = new ScoringMove(record.minScore, record.bestMove);
    //                return scoringMove;
    //            }
    //            // Si el mejor escenario guardado es peor que gamma, cortamos.
    //            if (record.maxScore < gamma)
    //            {
    //                scoringMove = new ScoringMove(record.maxScore, record.bestMove);
    //                return scoringMove;
    //            }
    //        }
    //    }
    //    // Si no encontramos nada o no servía, creamos un registro nuevo vacío para llenarlo después.
    //    else
    //    {
    //        record = new BoardRecord();
    //        record.hashValue = _keys.HashValue(board);
    //        record.depth = MaxDepth - depth;
    //        record.minScore = int.MinValue; // -Infinito
    //        record.maxScore = int.MaxValue; // +Infinito
    //    }


    //    // Si el juego terminó (Game Over) O si alcanzamos la profundidad máxima (7)
    //    if (board.IsEnded || depth == MaxDepth)
    //    {
    //        // Aquí se aplica lógica NEGAMAX (Maximizar mi puntaje, minimizar el del rival)

    //        // Si es un turno par (mi turno), evalúo normal.
    //        if (depth % 2 == 0)
    //        {
    //            record.maxScore = board.Evaluate();
    //        }
    //        // Si es turno impar (turno del rival), invierto el signo.
    //        else
    //        {
    //            record.maxScore = -board.Evaluate();
    //        }

    //        // En un nodo hoja, el mínimo y máximo son iguales (es un valor exacto) calculado es lo q vale el tablero en realidad no una suposicion
    //        record.minScore = record.maxScore;

    //        // Guardamos en memoria
    //        _transpositionTable[record.hashValue] = record;

    //        // Devolvemos el resultado (move -1 porque aquí no movemos, solo evaluamos)
    //        scoringMove = new ScoringMove(record.maxScore, -1);
    //    }
    //    // --- PASO 3: EXPANSIÓN (RECURSIVIDAD) ---
    //    else
    //    {
    //          bestMove = 0;
    //          bestScore = int.MinValue; // Empezamos asumiendo la peor puntuación posible
    //          int[] possibleMoves;

    //          // Obtenemos la lista de movimientos (0, 1, 2, 3, 4, 5)
    //           possibleMoves = board.PossibleMoves();

    //            foreach (int move in possibleMoves)
    //            {
    //                // Simulamos el movimiento creando un nuevo tablero clonado
    //                newBoard = board.BoardFromMove(move);

    //                // RECURSIVIDAD MÁGICA:
    //                // Llamamos a Test para el siguiente turno (depth + 1).
    //                // Usamos '-gamma' porque cambiamos de perspectiva (mi rival intenta ganarme).
    //                scoringMove = Test(newBoard, (byte)(depth + 1), -gamma);

    //                // El rival nos devuelve un score favorable para él. y desfavorable pra mi. Para mi su score vale lo contrsrio q para el pq me perjudica Invertimos el score.
    //                int invertedScore = -scoringMove.Score;

    //                // Si este movimiento es mejor que el mejor que tenía hasta ahora, lo guardo.
    //                if (invertedScore > bestScore)
    //                {
    //                    record.bestMove = move;
    //                    bestScore = invertedScore;
    //                    bestMove = move;
    //                }

    //                // Actualización de cotas (Alpha-Beta) para la Tabla de Transposición
    //                if (bestScore < gamma)
    //                {
    //                    record.maxScore = bestScore; // Cota superior
    //                }
    //                else
    //                {
    //                    record.minScore = bestScore; // Cota inferior
    //                }
    //            }

    //            // Guardamos el análisis en la memoria antes de salir
    //            _transpositionTable[record.hashValue] = record;

    //            // Devolvemos el mejor movimiento encontrado en esta rama
    //            scoringMove = new ScoringMove(bestScore, bestMove);
    //    }
    //        return scoringMove;
    //}

    //public ScoringMove? MTD(GameState board) // La función principal que inicia MTD(f)
    //{
    //    int i;
    //    int gamma, guess = _globalGuess;// guesss empieza siendo una suposicion inicial del pasado. gamma es la suposicion actual en cada iteracion. guess se actualiza en cada iteracion con el resultado de Test
    //    ScoringMove scoringMove = null;
    //    _maximumExploredDepth = 0;// Reiniciamos la profundidad explorada cada vez que llamamos a MTD

    //    string output = "";

    //    for (i = 0; i < MaxIterations; i++)
    //    {
    //        gamma = guess;// Establecemos gamma como la suposición actual. guess será actualizada en cada iteración con el resultado de Test anterior
    //        scoringMove = Test(board, 0, gamma - 1);// Llamada a Test con gamma-1 y depth 0
    //        guess = scoringMove.Score;
    //        if (gamma == guess)// Si la suposición es correcta, terminamos
    //        {
    //            _globalGuess = guess;// Actualizamos la suposición global para la próxima vez
    //            output += "guess encontrado en iteracion " + i;
    //            return scoringMove;
    //        }
    //    }
    //    output += "guess no encontrado";
    //    _globalGuess = guess;

    //    return scoringMove;
    //}

    //public int GetBestMove(int[,] board, int depth, int aiPlayer)
    //{
    //    //// 1. Clonamos el tablero para no modificar el original
    //    int[,] boardClone = (int[,])board.Clone();
    //    // 2. Creamos el "Tablero Inteligente" (Wrapper)
    //    MTDBoard smartBoard = new MTDBoard(boardClone, _zk);
    //}


}
