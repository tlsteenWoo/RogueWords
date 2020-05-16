using Microsoft.Xna.Framework;
using MknGames;
using MknGames.Rogue_Words;
using MknGames.Split_Screen_Dungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueWordsBase
{
    public class RogueEnemy
    {
        public string name = "Rat";
        public float hp = 10;
    }
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
    public class RogueGameMode:RogueWordsGameMode
    {
        BoardScreenClassic bsc;
        bool inBattle = false;
        int battleState = 0;
        float previousScore = 0;
        float score = 0;
        int preBattleX = 0;
        int preBattleY = 0;
        int exitX;
        int exitY;
        int randomBattlePseudoChance;
        int randomBattleChance = 50;
        RogueEnemy enemy;
        RogueEnemy player = new RogueEnemy() { hp = 50 };
        bool[,] consumed;
        bool[,] savedConsumed;

        public RogueGameMode(BoardScreenClassic classic):base(classic)
        {
                bsc.consumeOldVisibleFlag = false;
            bsc.revealNewVisibleFlag = false;
            bsc.consumeCurrentTileFlag = false;
            bsc.applyMultiplierFlag = false;
            bsc.allowChainingFlag = false;
            bsc.drawDiscoveredWordsFlag = false;
            //for(int i = 0; i < )
            bsc.mapH = 50;
            bsc.mapW = 50;
            MainMenuScreenClassic.ignoreRequestedSize = true;
            randomBattlePseudoChance = randomBattleChance;
        }

        public override void OnReset()
        {
            consumed = new bool[bsc.mapW, bsc.mapH];
            for (int x = 0; x < bsc.mapW; ++x)
                for (int y = 0; y < bsc.mapH; ++y)
                {
                    consumed[x, y] = bsc.game1.rand.Next(3) == 0;
                }
        }

        public override void OnUpdate()
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
                        if(t.consumed)
                            t.letter = ' ';
                    }
                }
                bsc.boardTiles[bsc.playerX, bsc.playerY].chain = 0;
                bsc.boardTiles[bsc.playerX, bsc.playerY].consumed = true;
                if (bsc.playerMoved)
                {
                    if (bsc.game1.rand.Next(randomBattlePseudoChance) == 0)
                    {
                        inBattle = true;
                        preBattleX = bsc.playerX;
                        preBattleY = bsc.playerY;
                        savedConsumed = consumed;
                        randomBattlePseudoChance = randomBattleChance;
                    }
                    else
                    {
                        randomBattlePseudoChance--;
                    }
                }
            }
            else
            {
                switch (battleState)
                {
                    case 0:
                        battleState++;
                        bsc.boardTiles[bsc.oldPlayerPosition.X, bsc.oldPlayerPosition.Y].chain = -1;
                        enemy = new RogueEnemy();
                        switch(bsc.game1.rand.Next(4))
                        {
                            case 0:
                            case 1:
                            case 2:
                                enemy.name = "Rat";
                                enemy.hp = 10;
                                break;
                            case 3:
                                enemy.name = "Crocodile";
                                enemy.hp = 20;
                                break;
                        }
                        bsc.requestReset = true;
                        bsc.wordBuildFlag = false;
                        break;
                    case 1:
                        bsc.wordBuildFlag = true;

                        battleState++;
                        break;
                }
                bsc.consumeOldVisibleFlag = true;
                bsc.revealNewVisibleFlag = true;
                bsc.consumeCurrentTileFlag = true;
                if (enemy.hp <= 0)
                {
                    battleState = 0;
                    inBattle = false;
                    bsc.playerX = preBattleX;
                    bsc.playerY = preBattleY;
                    consumed = savedConsumed;
                }
                if(previousScore != score)
                {
                    float damage = score - previousScore;
                    if (damage < 0)
                        player.hp += damage;
                    else
                        enemy.hp -= damage;
                }
                if (bsc.game1.kclick(Microsoft.Xna.Framework.Input.Keys.Enter))
                    bsc.Collect(0);
            }
        }

        public override void OnPostUpdate()
        {
            if(battleState == 1)
            {
                bsc.boardTiles[bsc.playerX, bsc.playerY].letter = '@';
                bsc.score = score;
            }
        }

        public override void OnDraw()
        {
            if(inBattle && enemy != null)
                bsc.game1.drawString("HP: " + enemy.hp, new Microsoft.Xna.Framework.Rectangle(0, 0, 300, 200), Color.Red, new Vector2(0.5f), true);
            if(inBattle)
            bsc.game1.drawString("HP: " + player.hp, new Microsoft.Xna.Framework.Rectangle(0, 200, 300, 200), Color.Green, new Vector2(0.5f), true);
        }
    }
}
