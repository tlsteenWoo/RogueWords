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
        public QuestGameMode(BoardScreenClassic bsc)
        {
            this.bsc = bsc;
            currentHealth = healthMax;
            for (int i = 0; i < bsc.chainColors.Length; ++i)
                bsc.chainColors[i] = Color.SaddleBrown;
        }

        public override void OnReset()
        {
            base.OnReset();
        }

        public override void OnLoadContent()
        {
            base.OnLoadContent();
            bsc.playerDeadline = 9999;
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
            else
                currentHealth += scoreDelta;
            if(scoreGained > scoreTarget)
            {
                scoreGained -= scoreTarget;
                xp++;
            }
            if(xp > xpTarget)
            {
                xp -= xpTarget;
                bsc.targetCommonLevel=Math.Min(bsc.targetCommonLevel+1,bsc.commonSizes.Length-1);
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
        }
        public override void OnDraw()
        {
            base.OnDraw();
            float heightPct = 0.03f;
            for (int i = 0; i < 3; ++i)
            {
                Rectf r = Backpack.percentage(bsc.ViewportRect, 0, 0.15f + heightPct * i, 0.9f, heightPct);
                bsc.game1.drawSquare(r, Color.Black, 0);
                float a = scoreGained;
                float b = scoreTarget;
                if(i == 1)
                {
                    a = xp;
                    b = xpTarget;
                }
                if(i==2)
                {
                    a = currentHealth;
                    b = healthMax;
                }
                float pct = a / b;
                for (int j = 0; j < a; ++j)
                {
                    float ratio = 1 / b;
                    float pad = 0.001f;
                    float width = ratio - pad * 2;
                    Rectf r2 = Backpack.percentagef(r, ratio*j+pad, 0.1f, width, 0.8f);
                    bsc.game1.drawSquare(r2, Color.Green, 0);
                }
            }
            for(int i = 0; i < bsc.targetCommonLevel + 1;++i)
                bsc.game1.drawNgon(new Vector2(50 + i * 50, 50), Color.Red, 0, 5, 25, 2);
        }
    }
}
