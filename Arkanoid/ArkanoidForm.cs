using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ArkanoidGame
{
    public partial class ArkanoidForm : Form
    {
        private const int Width = 800;
        private const int Height = 600;
        private const int PaddleWidth = 100;
        private const int PaddleHeight = 10;
        private const int BallSize = 10;
        private const int BlockRowCount = 5;
        private const int BlockColumnCount = 10;
        private const int BlockWidth = 75;
        private const int BlockHeight = 20;
        private const int BlockSpacing = 10;
        private const int BonusChance = 30;
        private const int BonusSpeed = 3;

        private System.Windows.Forms.Timer gameTimer;
        private PictureBox paddle;
        private List<PictureBox> balls;
        private PictureBox[,] blocks;
        private int ballDirX = 1;
        private int ballDirY = -1;
        private Random random;
        private List<PictureBox> bonuses;
        private bool isIncreasedSpeed;
        private bool isIncreasedBallSpeed;
        private ToolTip bonusToolTip;

        public ArkanoidForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ArkanoidForm
            // 
            this.ClientSize = new System.Drawing.Size(Width, Height);
            this.Text = "Arkanoid";
            this.ResumeLayout(false);
        }

        private void InitializeGame()
        {
            random = new Random();
            bonuses = new List<PictureBox>();
            balls = new List<PictureBox>();

            paddle = new PictureBox
            {
                Width = PaddleWidth,
                Height = PaddleHeight,
                BackColor = Color.Blue
            };
            paddle.Location = new Point((Width - PaddleWidth) / 2, Height - PaddleHeight * 2);
            Controls.Add(paddle);

            CreateBall();

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 10;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            CreateBlocks();

            bonusToolTip = new ToolTip();
        }

        private void CreateBall()
        {
            PictureBox ball = new PictureBox
            {
                Width = BallSize,
                Height = BallSize,
                BackColor = Color.Red
            };
            ball.Location = new Point(Width / 2 - BallSize / 2, Height / 2 - BallSize / 2);
            balls.Add(ball);
            Controls.Add(ball);
        }

        private void CreateBlocks()
        {
            blocks = new PictureBox[BlockRowCount, BlockColumnCount];

            for (int row = 0; row < BlockRowCount; row++)
            {
                for (int col = 0; col < BlockColumnCount; col++)
                {
                    blocks[row, col] = new PictureBox
                    {
                        Width = BlockWidth,
                        Height = BlockHeight,
                        BackColor = Color.Green,
                        Tag = "block",
                        Left = col * (BlockWidth + BlockSpacing),
                        Top = row * (BlockHeight + BlockSpacing) + 50
                    };
                    Controls.Add(blocks[row, col]);
                }
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
          
            foreach (PictureBox ball in balls.ToList())
            {
                ball.Left += (isIncreasedBallSpeed ? 2 * ballDirX : ballDirX);
                ball.Top += (isIncreasedBallSpeed ? 2 * ballDirY : ballDirY);

                if (ball.Left <= 0 || ball.Left + ball.Width >= Width)
                    ballDirX = -ballDirX;

                if (ball.Top <= 0)
                    ballDirY = -ballDirY;

                if (ball.Bounds.IntersectsWith(paddle.Bounds))
                    ballDirY = -ballDirY;

                for (int row = 0; row < BlockRowCount; row++)
                {
                    for (int col = 0; col < BlockColumnCount; col++)
                    {
                        if (ball.Bounds.IntersectsWith(blocks[row, col].Bounds) && blocks[row, col].Visible)
                        {
                            ballDirY = -ballDirY;
                            blocks[row, col].Visible = false;
                            CheckBonus(blocks[row, col].Location);
                        }
                    }
                }

                if (ball.Top + ball.Height >= Height)
                {
                    balls.Remove(ball);
                    Controls.Remove(ball);
                }
            }

            if (balls.Count == 0)
            {
                gameTimer.Stop();
                MessageBox.Show("Game Over");
                Close();
            }

            MoveBonuses();
            CheckBonusCollision();
        }

        private void CheckBonus(Point location)
        {
            if (random.Next(100) < BonusChance)
            {
                var bonusType = random.Next(3);

                PictureBox bonus = new PictureBox
                {
                    Width = 20,
                    Height = 20,
                    BackColor = Color.Yellow,
                    Location = location
                };

                bonuses.Add(bonus);
                Controls.Add(bonus);

                switch (bonusType)
                {
                    case 0:
                        bonus.Tag = "extraBall";
                        bonusToolTip.SetToolTip(bonus, "Dodatkowa kula");
                        break;
                    case 1:
                        bonus.Tag = "increaseSpeed";
                        bonusToolTip.SetToolTip(bonus, "Zwiększenie prędkości platformy");
                        break;
                    case 2:
                        bonus.Tag = "increaseBallSpeed";
                        bonusToolTip.SetToolTip(bonus, "Zwiększenie prędkości lotu kulki");
                        break;
                }
            }
        }

        private void MoveBonuses()
        {
            foreach (PictureBox bonus in bonuses.ToList())
            {
                bonus.Top += BonusSpeed;

                if (bonus.Bounds.IntersectsWith(paddle.Bounds))
                {
                    ApplyBonus(bonus.Tag.ToString());
                    Controls.Remove(bonus);
                    bonuses.Remove(bonus);
                }

                if (bonus.Top > Height)
                {
                    Controls.Remove(bonus);
                    bonuses.Remove(bonus);
                }
            }
        }

        private void ApplyBonus(string bonusType)
        {
            switch (bonusType)
            {
                case "extraBall":
                    CreateBall();
                    break;
                case "increaseSpeed":
                    isIncreasedSpeed = true;
                    break;
                case "increaseBallSpeed":
                    isIncreasedBallSpeed = true;
                    break;
            }
        }

        private void CheckBonusCollision()
        {
            foreach (PictureBox bonus in bonuses.ToList())
            {
                if (bonus.Bounds.IntersectsWith(paddle.Bounds))
                {
                    ApplyBonus(bonus.Tag.ToString());
                    Controls.Remove(bonus);
                    bonuses.Remove(bonus);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Left && paddle.Left > 0)
                paddle.Left -= isIncreasedSpeed ? 10 : 5;

            if (e.KeyCode == Keys.Right && paddle.Left + paddle.Width < Width)
                paddle.Left += isIncreasedSpeed ? 10 : 5;
        }
    }
}