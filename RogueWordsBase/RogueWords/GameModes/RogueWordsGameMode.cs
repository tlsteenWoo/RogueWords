using Microsoft.Xna.Framework;
using MknGames;
using MknGames.Rogue_Words;
using MknGames.Split_Screen_Dungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueWordsBase.RogueWords.GameModes
{
    public class RogueWordsGameMode
    {
        bool loaded = false;
        public BoardScreenClassic bsc;
        public int previousCollectionCount;
        public int collectionCount;
        public string collectionWord;
        public bool collectionWordFound;

        public RogueWordsGameMode(BoardScreenClassic bsc)
        {
            this.bsc = bsc;
        }
        public virtual void OnReset()
        {

        }
        public virtual void OnLoadContent()
        {
            loaded = true;
        }
        public virtual void OnUpdate()
        {
            if (!loaded)
                OnLoadContent();
            previousCollectionCount = collectionCount;
            collectionCount = bsc.collectionTiles.Count;
            if (previousCollectionCount != collectionCount)
            {
                if (previousCollectionCount == 0)
                {
                    collectionWord = string.Empty;
                    for (int i = bsc.collectionTiles.Count - 1; i >= 0; --i)
                    {
                        collectionWord += bsc.collectionTiles.ElementAt(i).letter;
                    }
                    collectionWordFound = false;
                    if (bsc.dictionary[collectionWord[0]].ContainsKey(collectionWord.Length) && bsc.dictionary[collectionWord[0]][collectionWord.Length].ContainsKey(collectionWord))
                        collectionWordFound = true;
                }
                if (collectionCount > 0)
                {
                    OnTileCollected(bsc.collectionTiles.First());
                }
            }
        }
        public virtual void OnTileCollected(Tile t)
        {

        }
        public virtual void OnPostUpdate()
        {

        }
        public virtual void OnDraw()
        {

        }
        public Rectf DrawProgressBar(float valueA, float valueB, float yPercentage, float heightPercentage, Color color = default(Color))
        {
            Rectf r = Backpack.percentage(bsc.ViewportRect, 0, yPercentage, 1, heightPercentage);
            //bsc.game1.drawSquare(r, Color.Black, 0);
            float pct = valueA / valueB;
            float widthPercentage = 1 / valueB;
            float pad = 0.001f;
            float width = widthPercentage - pad * 2;
            for (int j = 0; j < valueA; ++j)
            {
                Rectf r2 = Backpack.percentagef(r, widthPercentage * j + pad, 0.1f, width, 0.8f);
                bsc.game1.drawSquare(r2, color, 0);
            }
            return r;
        }
    }
}
