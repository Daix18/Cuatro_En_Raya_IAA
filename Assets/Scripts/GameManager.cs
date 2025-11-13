using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int ROWS = 6;
    public static int COLUMNS = 7;

    public int[,] board = new int[COLUMNS, ROWS];

    public Transform panel;

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        for (int c = 0; c < COLUMNS; c++)
        {
            for (int r = 0; r < ROWS; r++)
            {
                board[c, r] = 0;
            }
        }
    }

    public void PlayerMove(int column)
    {
        for (int row = 0; row < ROWS; row++)
        {
            if (board[column, row] == 0)
            {
                board[column, row] = 1;
                UpdateVisual(column, row, Color.red);
                //AI move here
                return;
            }
        }
    }

    void UpdateVisual(int column, int row, Color color)
    {
        Transform circle = panel.GetChild(column).GetChild(row);
        var image = circle.GetComponent<UnityEngine.UI.Image>();
        image.color = color;
        Debug.Log("Column " + column + ", Row " + row + " -> Color: " + color);
    }
}