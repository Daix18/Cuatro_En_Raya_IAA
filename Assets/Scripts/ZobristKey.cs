using System;
using UnityEngine;

public class ZobristKey
{
    // [42 casillas, 2 jugadores]
    // Índice 0-41 representa la casilla.
    // Índice 0 = Player 1, Índice 1 = AI (-1).
    public int[,] Keys { get; set; }

    public const int NumKeys = 10 * 6;
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
    
 
   
    public int HashValue (int c, int r, int player)
    {
           int hashValue = 0;
           int currentplayerIndex = (player == 1) ? 0 : 1;

            // Calculamos el índice lineal 
            int linearIndex = c * ROWS + r; //indice lineal de 0 a 41 porque hay 42 casillas

            hashValue ^= GetKey(linearIndex, currentplayerIndex);
        return hashValue;
    }
}

