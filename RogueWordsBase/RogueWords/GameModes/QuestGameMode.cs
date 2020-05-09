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
    public class QuestGameMode : RogueWordsGameMode
    {
        BoardScreenClassic bsc;
        int scoreTarget = 10;
        int scoreGained = 0;
        int previousScore = 0;
        int currentScore = 0;
        int xp = 0;
        int xpTarget = 10;
        int currentHealth = 0;
        int healthMax = 50;
        int collectionCount;
        int previousCollectionCount;
        private string collectionWord;
        private bool collectionWordFound;

        public QuestGameMode(BoardScreenClassic bsc)
        {
            this.bsc = bsc;
            currentHealth = healthMax;
            for (int i = 0; i < bsc.chainColors.Length; ++i)
                bsc.chainColors[i] = Color.SaddleBrown;
            bsc.drawDiscoveredWordsFlag = false;
            bsc.drawMultiplierFlag = false;
            bsc.collectOnWordExhaustionFlag = false;
        }

        public override void OnReset()
        {
            base.OnReset();
        }

        public override void OnLoadContent()
        {
            base.OnLoadContent();
            bsc.playerDeadline = 9999;
            UpdateLevel();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (bsc.game1.kclick(Microsoft.Xna.Framework.Input.Keys.Enter))
                bsc.Collect(0);
            previousScore = currentScore;
            currentScore = bsc.score;
            int scoreDelta = currentScore - previousScore;
            if (scoreDelta > 0)
                scoreGained += scoreDelta;
            CheckDamage();
            if(currentHealth <= 0)
            {
                bsc.rwg.activeScreen = bsc.parentScreen;
            }
            if(scoreGained > scoreTarget)
            {
                scoreGained -= scoreTarget;
                xp++;
            }
            if(xp > xpTarget)
            {
                xp -= xpTarget;
                bsc.targetCommonLevel=bsc.targetCommonLevel+1;
                if (bsc.targetCommonLevel >= bsc.commonSizes.Length)
                    bsc.rwg.activeScreen = bsc.parentScreen;
                else
                    UpdateLevel();
            }
            CheckScrolls();
        }
        void CheckScrolls()
        {
            if(bsc.game1.kclick(Microsoft.Xna.Framework.Input.Keys.D0))
            {
                SetVisible(bsc.playerX - 1, bsc.playerY, false);
                SetVisible(bsc.playerX + 1, bsc.playerY, false);
                SetVisible(bsc.playerX, bsc.playerY-1, false);
                SetVisible(bsc.playerX, bsc.playerY+1, false);
            }
        }
        void SetVisible(int x, int y, bool value)
        {
            if (bsc.PointInBounds(x, y))
                bsc.boardTiles[x, y].visible = value;
        }
        void CheckDamage()
        {
            previousCollectionCount = collectionCount;
            collectionCount = bsc.collectionTiles.Count;
            if (previousCollectionCount != collectionCount)
            {
                if (previousCollectionCount == 0)
                {
                    collectionWord = string.Empty;
                    for (int i = bsc.collectionTiles.Count-1; i >= 0; --i)
                    {
                        collectionWord += bsc.collectionTiles.ElementAt(i).letter;
                    }
                    collectionWordFound = false;
                    if (bsc.dictionary[collectionWord[0]].ContainsKey(collectionWord.Length) && bsc.dictionary[collectionWord[0]][collectionWord.Length].ContainsKey(collectionWord))
                        collectionWordFound = true;
                }
                if (collectionCount > 0)
                {
                    if (!collectionWordFound)
                        currentHealth -= bsc.collectionTiles.First().value;
                    else
                        scoreGained += bsc.collectionTiles.First().value;
                }
            }
        }
        public void UpdateLevel()
        {
            switch(bsc.targetCommonLevel)
            {
                case 0:
                    bsc.assuredBranchLimit = 2;
                    break;
                case 1:
                    bsc.assuredBranchLimit = 2;
                    break;
                case 2:
                    bsc.assuredBranchLimit = 2;
                    break;
                case 3:
                    bsc.assuredBranchLimit = 1;
                    break;
                case 4:
                    bsc.assuredBranchLimit = 1;
                    break;
                case 5:
                    bsc.assuredBranchLimit = 1;
                    break;
                default:
                    bsc.assuredBranchLimit = 2;
                    break;
            }
            bsc.assuredBranchLimit = 1;
        }
        public override void OnDraw()
        {
            base.OnDraw();
            float heightPct = 0.03f;
            var scoreBar = DrawProgressBar(scoreGained, scoreTarget, 0.15f, heightPct);
            var xpBar = DrawProgressBar(xp, xpTarget, 0.15f + heightPct, heightPct);
            var hpBar = DrawProgressBar(currentHealth, healthMax, 1 - heightPct, heightPct);
            if (bsc.collectionTiles.Count > 0)
            {
                float progress = bsc.collectionElapsed / bsc.collectionDuration;
                var tile = bsc.collectionTiles.First();
                Vector2 va = tile.position;
                Vector2 vb = scoreBar.GetLocation();
                Color c = Color.Green;
                if (!collectionWordFound)
                {
                    vb = hpBar.GetLocation();
                    c = Color.Red;
                }
                bsc.game1.drawLine(Vector2.Lerp(va, vb, progress - 0.1f), Vector2.Lerp(va, vb, progress), c, 2);
            }
            bsc.playRectY = xpBar.GetBottom()/bsc.ViewportRect.Height;
            bsc.playRectHeight = 1 - bsc.playRectY - heightPct;

            for(int i = 0; i < bsc.targetCommonLevel + 1;++i)
                bsc.game1.drawNgon(new Vector2(50 + i * 50, 50), Color.Red, 0, 5, 25, 2);
        }
        Rectf DrawProgressBar(float a, float b, float y, float heightPct)
        {
            Rectf r = Backpack.percentage(bsc.ViewportRect, 0, y, 1, heightPct);
            bsc.game1.drawSquare(r, Color.Black, 0);
            float pct = a / b;
            for (int j = 0; j < a; ++j)
            {
                float ratio = 1 / b;
                float pad = 0.001f;
                float width = ratio - pad * 2;
                Rectf r2 = Backpack.percentagef(r, ratio * j + pad, 0.1f, width, 0.8f);
                bsc.game1.drawSquare(r2, Color.Green, 0);
            }
            return r;
        }
    }
}
