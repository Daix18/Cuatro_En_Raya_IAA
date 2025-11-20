using System;
using UnityEngine;

public class ZobristKey
{
    // [42 casillas, 2 jugadores]
    // Índice 0-41 representa la casilla.
    // Índice 0 = Player 1, Índice 1 = AI (-1).
    public int[,] Keys { get; set; }
    private const int ROWS = 6; // Necesario para calcular el índice lineal

    public ZobristKey()
    {
        System.Random rnd = new System.Random();

        // 7 columnas * 6 filas = 42 casillas totales
        // El 2 es para Jugador 1 y Jugador 2
        Keys = new int[42, 2];

        Console.WriteLine("Generando Claves Zobrist para Connect 4");

        for (int i = 0; i < 42; i++) // Recorre las casillas (0 a 41)
        {
            for (int j = 0; j < 2; j++) // Recorre los jugadores (0 y 1)
            {
                // Genera un número aleatorio para esa combinación
                Keys[i, j] = rnd.Next(int.MaxValue);
            }
        }
    }


    // Indices en  [linearIndex, playerIndex]
    public int GetKey(int index, int PlayerIndex)
    {
        return Keys[index, PlayerIndex];
    }


    public int HashValue(int[,] board)
    {
        int currentHash = 0;
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        int position;
        int playerIndex; // Será 0 o 1 para buscar en las Keys
        int cellValue;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                cellValue = board[row, col];

                // Asumiendo que 0 sigue siendo espacio vacío
                if (cellValue != 0)
                {
                    // 1. Calculamos la posición lineal
                    position = (row * cols) + col;

                    // 2. Traducimos tus valores (-1 y 1) a índices válidos (0 y 1)
                    // Si es 1 (Oponente) -> Usamos índice 0
                    // Si es -1 (AI)      -> Usamos índice 1
                    if (cellValue == 1)
                    {
                        playerIndex = 0;
                    }
                    else // Asumimos que es -1
                    {
                        playerIndex = 1;
                    }

                    // 3. Obtenemos la clave y aplicamos XOR
                    currentHash ^= GetKey(position, playerIndex);
                }
            }
        }

        return currentHash;
    }

}

