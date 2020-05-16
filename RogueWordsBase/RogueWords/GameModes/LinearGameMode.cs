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
        float[] comboMeterTarget = new float[] { 5, 10, 15, 1 };
        Texture2D barTx;

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
        }


        public override void OnLoadContent()
        {
            base.OnLoadContent();
            barTx = bsc.game1.Content.Load<Texture2D>("Sprites/bar");
        }

        public override void OnReset()
        {
            base.OnReset();
            combo = 0;
            comboMeter = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (bsc.game1.kclick(Microsoft.Xna.Framework.Input.Keys.Enter))
                bsc.Collect(0);
            float comboMeterRate = comboMeterTarget[combo]/100;
            if(comboMeterDelta < 0)
            {
                comboMeter-=comboMeterRate;
                comboMeterDelta+=comboMeterRate;
            }
            if(comboMeterDelta > 0)
            {
                comboMeter+=comboMeterRate;
                comboMeterDelta-=comboMeterRate;
            }
            if (comboMeter >= comboMeterTarget[combo] && combo < comboMeterTarget.Length - 1)
            {
                comboMeter -= comboMeterTarget[combo];
                combo++;
            }
            if(comboMeter <= 0 && combo > 0)
            {
                combo--;
                comboMeter += comboMeterTarget[combo];
            }
            comboMeter = MathHelper.Clamp(comboMeter, 0, comboMeterTarget[combo]);
        }

        public override void OnTileCollected(Tile t)
        {
            base.OnTileCollected(t);
            if (collectionWordFound)
            {
                comboMeterDelta += t.value;
            }
            else
            {
                comboMeterDelta -= t.value;
            }
            t.collectionMultiplier = ComboMultiplier;
            //bsc.scoreMultiplier = 1 + (float)combo * 0.5f;
        }

        public override void OnDraw()
        {
            base.OnDraw();
            Rectf p = Backpack.percentage(bsc.ViewportRect, 0, 0.15f, 1, 0.1f);
            bsc.game1.drawSquare(p, bsc.monochrome(0.2f), 0);
            Rectf bar = Backpack.percentagef(p, 0.3f, 0.3f, 0.4f, 0.4f);
            Rectf text = Backpack.percentagef(p, 0.7f, 0, 0.1f, 1);
            Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Blue };
            bsc.game1.drawSquare(text, Color.Black, 0);
            bsc.game1.drawString(ComboMultiplier + "x", (Rectangle)text, colors[combo], new Vector2(0.5f), true);
            Rectf p2 = Backpack.percentagef(bar, 0.03f, 0.1f, 0.94f, 0.8f);
            bsc.game1.drawSquare(p2, Color.Black, 0);
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
                Rectf p3 = Backpack.percentagef(p2, 0, 0, pct, 1);
                bsc.game1.drawSquare(p3, colors[i], 0);
                //p = DrawProgressBar(valueA, valueB, 0.15f, 0.1f, colors[i]);
            }
            bsc.game1.drawTexture(barTx, bar.GetLocation(), Color.White, 0, bar.Width, bar.Height);
            bsc.playRectY = p.GetBottom() / bsc.ViewportRect.Height;
            bsc.playRectHeight = 1 - bsc.playRectY;

            bsc.game1.drawSquare(bsc.multiplierRect, Color.Black, 0);
            bsc.game1.drawString(ComboMultiplier + "x", (Rectangle)bsc.multiplierRect, colors[combo], default(Vector2), true);
        }
    }
}
