using Microsoft.Xna.Framework;
using MknGames.Rogue_Words;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueWordsBase
{
    public class RogueEnemy
    {
        public int hp = 50;
    }
    public class RogueGameMode
    {
        BoardScreenClassic bsc;
        bool inBattle = false;
        bool initializedBattle = false;
        int previousScore = 0;
        int score = 0;
        RogueEnemy enemy;
        RogueEnemy player = new RogueEnemy();
        bool[,] consumed;

        public RogueGameMode(BoardScreenClassic classic)
        {
            bsc = classic;
                bsc.consumeOldVisibleFlag = false;
            bsc.revealNewVisibleFlag = false;
            bsc.consumeCurrentTileFlag = false;
            //for(int i = 0; i < )
            bsc.mapH = 20;
            bsc.mapW = 20;
            MainMenuScreenClassic.ignoreRequestedSize = true;
        }

        public void onReset()
        {
            consumed = new bool[bsc.mapW, bsc.mapH];
            for (int x = 0; x < bsc.mapW; ++x)
                for (int y = 0; y < bsc.mapH; ++y)
                {
                    consumed[x, y] = bsc.game1.rand.Next(3) == 0;
                }
        }

        public void update()
        {
            previousScore = score;
            score = bsc.score;
            if (!inBattle)
            {
                bsc.chainWord = "";
                bsc.chainTiles.Clear();
                bsc.consumeOldVisibleFlag = false;
                bsc.revealNewVisibleFlag = false;
                bsc.consumeCurrentTileFlag = false;
                bsc.wordBuildFlag = false;
                bsc.playerDeadline = 999999;
                bsc.boardTiles[bsc.playerX, bsc.playerY].letter = '@';
                foreach (Tile t in bsc.boardTiles)
                {
                    if (t.X != bsc.playerX || t.Y != bsc.playerY)
                    {
                        t.chain = -1;
                        t.visible = false;
                        if (consumed != null)
                            t.consumed = consumed[t.X,t.Y];
                    }
                }
                bsc.boardTiles[bsc.playerX, bsc.playerY].chain = 0;
                bsc.boardTiles[bsc.playerX, bsc.playerY].consumed = true;
                if (bsc.playerMoved)
                {
                    if (bsc.game1.rand.Next(10) == 0)
                        inBattle = true;
                }
            }
            else
            {
                if(!initializedBattle)
                {
                    initializedBattle = true;
                    bsc.boardTiles[bsc.oldPlayerPosition.X, bsc.oldPlayerPosition.Y].chain = -1;
                    enemy = new RogueEnemy();
                }
                bsc.consumeOldVisibleFlag = true;
                bsc.revealNewVisibleFlag = true;
                bsc.consumeCurrentTileFlag = true;
                bsc.wordBuildFlag = true;
                if (enemy.hp <= 0)
                {
                    initializedBattle = false;
                    inBattle = false;
                }
                if(previousScore != score)
                {
                    int damage = score - previousScore;
                    enemy.hp -= damage;
                }
                if (bsc.game1.kclick(Microsoft.Xna.Framework.Input.Keys.Enter))
                    bsc.Collect(0);
            }
        }

        public void Draw()
        {
            if(inBattle && enemy != null)
            bsc.game1.drawString("HP: " + enemy.hp, new Microsoft.Xna.Framework.Rectangle(0, 0, 300, 200), Color.Red, new Vector2(0.5f), true);
        }
    }
}
