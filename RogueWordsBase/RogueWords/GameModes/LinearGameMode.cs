using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class LinearGameMode : RogueWordsGameMode
    {
        int combo = 0;
        float comboMeter = 0;
        float comboMeterDelta = 0;
        float[] comboMeterTarget = new float[] { 10, 8, 6, 1 };
        Texture2D barTx;
        int reshuffleCount = 0;
        int reshuffleLimit = 3;
        int level = 0;

        public float ComboMultiplier
        {
            get { return 1 + (float)combo * 0.5f; }
        }
        public LinearGameMode(BoardScreenClassic bsc) : base(bsc)
        {
            bsc.setMultiplierToCurrentComboFlag = false;
            bsc.collectOnWordExhaustionFlag = false;
            bsc.drawMultiplierFlag = false;
            bsc.allowChainingFlag = false;
            bsc.drawDiscoveredWordsFlag = false;
            bsc.drawNoMoreWordsFlag = false;
        }


        public override void OnLoadContent()
        {
            base.OnLoadContent();
            barTx = bsc.game1.Content.Load<Texture2D>("Sprites/bar");
            bsc.playerDeadline = 9999;
        }

        public override void OnReset()
        {
            base.OnReset();
            combo = 0;
            comboMeter = 0;
            comboMeterDelta = 0;
        }

        void UpdateLevel()
        {
            //switch(level)
            //{
            //    case 0:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 1;
            //        break;
            //    case 1:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 1;
            //        break;
            //    case 2:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 2;
            //        break;
            //    case 3:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 2;
            //        break;
            //    case 4:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 3;
            //        break;
            //    case 5:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 3;
            //        break;
            //    case 6:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 4;
            //        break;
            //    case 7:
            //        bsc.assuredBranchLimit = 2;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 4;
            //        break;
            //    case 8:
            //        bsc.assuredBranchLimit = 1;
            //        bsc.provideBadUnassuredFlag = false;
            //        bsc.targetCommonLevel = 5;
            //        break;
            //    case 9:
            //        bsc.assuredBranchLimit = 1;
            //        bsc.provideBadUnassuredFlag = true;
            //        bsc.targetCommonLevel = 5;
            //        break;
            //}
        }
        public void HandleReshuffle()
        {
            if(bsc.playerMoved)
            {
                reshuffleCount = 0;
            }
            if (bsc.pointerDown() && bsc.pointerUpOld() &&
                bsc.boardTiles[bsc.playerX, bsc.playerY].rect.Contains(bsc.pointer().ToPoint()))
            {
                if (reshuffleCount < reshuffleLimit - 1)
                {
                    bsc.ReshuffleLetters();
                    reshuffleCount++;
                }
            }
        }

        void UpdateComboMeter()
        {
            float comboMeterRate = 0.1f;// comboMeterTarget[combo]/100;
            if (comboMeterDelta < 0)
            {
                comboMeter -= comboMeterRate;
                comboMeterDelta += comboMeterRate;
            }
            if (comboMeterDelta > 0)
            {
                comboMeter += comboMeterRate;
                comboMeterDelta -= comboMeterRate;
            }
            if (comboMeter >= comboMeterTarget[combo] && combo < comboMeterTarget.Length - 1)
            {
                comboMeter -= comboMeterTarget[combo];
                combo++;
            }
            if (comboMeter <= 0 && combo > 0)
            {
                combo--;
                comboMeter += comboMeterTarget[combo];
            }
            comboMeter = MathHelper.Clamp(comboMeter, 0, comboMeterTarget[combo]);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (bsc.game1.kclick(Microsoft.Xna.Framework.Input.Keys.Enter) ||
                (bsc.pointerDown() && bsc.pointerUpOld() && bsc.chainWordRect.ContainsPoint(bsc.pointer())))
                bsc.Collect(0);

            HandleReshuffle();

            UpdateComboMeter();
        }

        public override void OnTileCollected(Tile t)
        {
            base.OnTileCollected(t);
            if (collectionWordFound)
            {
                comboMeterDelta += t.value;
                t.collectionMultiplier = ComboMultiplier;
            }
            else
            {
                comboMeterDelta -= t.value;
                t.collectionMultiplier = 0;
            }
            //bsc.scoreMultiplier = 1 + (float)combo * 0.5f;
        }

        public override void OnDraw()
        {
            base.OnDraw();
            Rectf zone = Backpack.percentage(bsc.ViewportRect, 0, 0.15f, 1, 0.1f);
            bsc.game1.drawSquare(zone, bsc.monochrome(0.2f), 0);
            Rectf bar = Backpack.percentagef(zone, 0.3f, 0.3f, 0.4f, 0.4f);
            Rectf text = Backpack.percentagef(zone, 0.7f, 0, 0.1f, 1);
            Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Blue };
            Color[] textColors = new Color[colors.Length];
            for (int i = 0; i < textColors.Length; ++i)
                textColors[i] = colors[i];
            textColors[colors.Length - 1] = Color.White;
            bsc.game1.drawSquare(text, Color.Black, 0);
            bsc.game1.drawString(ComboMultiplier + "x", (Rectangle)text, textColors[combo], new Vector2(0.5f), true);
            Rectf meter = Backpack.percentagef(bar, 0.03f, 0.1f, 0.94f, 0.8f);
            bsc.game1.drawSquare(meter, Color.Black, 0);
            for (int i = 0; i < comboMeterTarget.Length; ++i)
            {
                if (combo < i) break;
                float valueA = 0;
                float valueB = 0;
                    valueA = comboMeter;
                if (combo > i)
                    valueA = comboMeterTarget[i];
                valueB = comboMeterTarget[i];
                float pct = 1;
                if (valueB != 0)
                    pct = valueA / valueB;
                Rectf p3 = Backpack.percentagef(meter, 0, 0, pct, 1);
                bsc.game1.drawSquare(p3, colors[i], 0);
                //p = DrawProgressBar(valueA, valueB, 0.15f, 0.1f, colors[i]);
            }
            bsc.game1.drawTexture(barTx, bar.GetLocation(), Color.White, 0, bar.Width, bar.Height);
            bsc.playRectY = zone.GetBottom() / bsc.ViewportRect.Height;
            bsc.playRectHeight = 1 - bsc.playRectY;

            if(bsc.collectionTiles.Count > 0)
            {
                Vector2 start = bsc.collectionTiles.First().position;
                Vector2 end = new Vector2(bsc.ViewportRect.Width, 0);
                float pct = bsc.collectionElapsed / bsc.collectionDuration;
                Vector2 lineA = Vector2.Lerp(start, end, pct);
                Vector2 lineB = Vector2.Lerp(start, end, pct-.1f);
                Color color = Color.Green;
                if (!collectionWordFound)
                    color = Color.Red;
                bsc.game1.drawLine(lineA, lineB, color, 3);
            }
            //bsc.game1.drawSquare(bsc.multiplierRect, Color.Black, 0);
            //bsc.game1.drawString(ComboMultiplier + "x", (Rectangle)bsc.multiplierRect, colors[combo], default(Vector2), true);
        }
    }
}
