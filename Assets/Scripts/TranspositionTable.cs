using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
internal class BoardRecord
{
    public int hashValue;
    public int minScore;
    public int maxScore;
    public int bestMove;
    public int depth;

    public BoardRecord()
    {
        hashValue = 0;
        minScore = 0;
        maxScore = 0;
        bestMove = 0;//columna
        depth = 0;
    }


}
internal class TranspositionTable : Dictionary<int, BoardRecord>
{
    
}
