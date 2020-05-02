using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueWordsBase
{
    public class Tile
    {
        public char letter = 'A';
        public bool consumed;
        public bool visible;
        public int chain;
        public int collectionMultiplier;
        public int X;
        public int Y;
        int[] letterValueTable = new int[26]
        {
                1,//a
                3,//b
                3,//c
                2,//d
                1,//e
                4,//f
                2,//g
                4,//h
                1,//i
                8,//j
                5,//k
                1,//l
                3,//m
                1,//n
                1,//o
                3,//p
                10,//q
                1,//r
                1,//s
                1,//t
                1,//u
                4,//v
                4,//w
                8,//x
                4,//y
                10,//z
        };
        public Tile(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public int value
        {
            get
            {
                if (letter > 65)
                    return letterValueTable[letter - 65];
                return 0;
            }
        }
    }
}
