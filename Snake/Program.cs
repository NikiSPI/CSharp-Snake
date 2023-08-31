using System.Diagnostics;
using System.Timers;

public class Frame : Form
{
    public ActionPanel actionPanel;
    public ScoreLabel scoreLabel;
    
    public Frame()
    {
        scoreLabel = new();
        
        actionPanel = new();
        actionPanel.Location = new Point(0, scoreLabel.Size.Height);
        actionPanel.Size = new Size(BackEnd.WIDTH_, BackEnd.HEIGHT_);
        
        FormBorderStyle = FormBorderStyle.FixedSingle; //makes it unresizable
        Icon = Icon.ExtractAssociatedIcon(Path.GetDirectoryName(BackEnd.stackFrame.GetFileName()) + "\\Images\\SnakeIcon.ico");
        Text = " Hungry Snake";
        const int scaleWidthAdd_ = 16, scaleHeightAdd_ = 39;
        Size = new Size(BackEnd.WIDTH_ + scaleWidthAdd_, scoreLabel.Size.Height + BackEnd.HEIGHT_ + scaleHeightAdd_);
        
        KeyPreview = true;
        KeyDown += BackEnd.DetectKey;

        Controls.Add(scoreLabel);
        Controls.Add(actionPanel);
    }
    public class ScoreLabel : Label
    {
        public ScoreLabel()
        {
            Size = new Size(BackEnd.WIDTH_, 60);
            Location = new Point((BackEnd.WIDTH_ - Size.Width) / 2, 0);
            Text = "Start by pressing 'Enter'";
            ForeColor = Color.White;
            Font = new Font("Comic Sans MS", 30);
            TextAlign = ContentAlignment.MiddleCenter;
            BackColor = Color.Black;
        }
        public void ChangeScore(String beforeText, int score)
        {
            if (score >= 0)
            {
                Text = beforeText + score;
            }
            else Text = beforeText;
        }
        
    }
    public class ActionPanel : Panel
    {
        public ActionPanel() //running protected methods of class 'Panel'
        {
            DoubleBuffered = true;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            SolidBrush brush;
        
            //painting the grid and the body of the snake
            for (int i = 0; i < BackEnd.sectionsInWidth_; i++)
            {
                for (int j = 0; j < BackEnd.sectionsInHeight_; j++)
                {
                    if (BackEnd.sectionNums[i, j] != 0 && BackEnd.frameEngine.headPos != new Point(i, j))
                        brush = new (Color.Aqua);
                    else
                    {
                        if ((i % 2 == 0 && j % 2 == 0) || (i % 2 == 1 && j % 2 == 1))
                            brush = new (Color.GreenYellow);
                        else
                            brush = new (Color.LawnGreen);
                    }
                    e.Graphics.FillRectangle(brush, i * BackEnd.sectionPixelWidth_, j * BackEnd.sectionPixelHeight_, BackEnd.sectionPixelWidth_, BackEnd.sectionPixelHeight_);
                }
            }
    
            brush = new(Color.CornflowerBlue); //painting the head
            e.Graphics.FillRectangle(brush, BackEnd.frameEngine.headPos.X * BackEnd.sectionPixelWidth_, BackEnd.frameEngine.headPos.Y * BackEnd.sectionPixelHeight_, BackEnd.sectionPixelWidth_, BackEnd.sectionPixelHeight_);

            brush = new(Color.Red); //painting the apple
            e.Graphics.FillEllipse(brush, BackEnd.frameEngine.applePos.X * BackEnd.sectionPixelWidth_, BackEnd.frameEngine.applePos.Y * BackEnd.sectionPixelHeight_, BackEnd.sectionPixelWidth_, BackEnd.sectionPixelHeight_);
        
        }
        
    }
}

internal class BackEnd
{
    public static StackFrame stackFrame = new StackTrace(new StackFrame(true)).GetFrame(0);
    
    public const int sectionPixelWidth_ = 60, sectionPixelHeight_ = 60, sectionsInWidth_ = 17, sectionsInHeight_ = 15;
    public const int WIDTH_ = sectionPixelWidth_ * sectionsInWidth_, HEIGHT_ = sectionPixelHeight_ * sectionsInHeight_;

    private static Frame frame;
    public static FrameEngine frameEngine;

    private static int[] movementDirection = {0, 0, 0, 0}; //up, down, left, right
    private static int[,] directionSetUp = { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 } };
    private static int directionsTurn = 0;
    public static int Xdirections = 1, Ydirections = 0;

    public static int[,] sectionNums = new int[sectionsInWidth_, sectionsInHeight_];
    private const int startingSnakeLength = 3;
    static int sectionSetNum = 0, tailSetNum = 1;

    private static bool gameRunning;
    private static System.Timers.Timer timer;
    private const int timerInterval = 120;
    
    public class FrameEngine
    {
        public Point headPos = new(2 + startingSnakeLength, sectionsInHeight_ / 2);
        private Point tailPos;

        public Point applePos = new(sectionsInWidth_ - 2, sectionsInHeight_ / 2);
        private Random appleRand = new();

        public int score = 0;

        public void UpdateFrameAssets()
        {
            SetDirection();
            
            MoveHeadCheck();

            for (int i = 0; i < sectionsInWidth_; i++) //finding the tail
            {
                for (int j = 0; j < sectionsInHeight_; j++)
                {
                    if (sectionNums[i, j] == tailSetNum)
                    {
                        tailPos = new Point(i, j);
                    }
                }
            }

            AppleIsEatenCheck();
        }

        private void SetDirection()
        {
            for (int i = 0; i < 4; i++)
            {
                if (movementDirection[i] == 1)
                {
                    //Console.WriteLine("<Direction Update>");
                    if ((i < 2 && Ydirections != -directionSetUp[i, 1]) ||
                        (i > 1 && Xdirections != -directionSetUp[i, 0]))
                    {
                        Xdirections = directionSetUp[i, 0];
                        Ydirections = directionSetUp[i, 1];
                    }
                    

                    for (int j = 0; j < 4; j++) movementDirection[j]--;
                    directionsTurn--;
                    break;
                }
            }
        }
        public void RepeatabilityCheck(int incrWay)
        {
            if (movementDirection[incrWay] <= 0 && directionsTurn <= 4)
            {
                directionsTurn++;
                movementDirection[incrWay] = directionsTurn;
            }
        }

        private void MoveHeadCheck()
        {
            headPos.X += Xdirections;
            headPos.Y += Ydirections;

            if (headPos.X < 0 || headPos.X >= sectionsInWidth_ || headPos.Y < 0 || headPos.Y >= sectionsInHeight_ ||
                sectionNums[headPos.X, headPos.Y] != 0)
            {
                GameEnd();
            }

            sectionNums[headPos.X, headPos.Y] = ++sectionSetNum;
        }
        private void AppleIsEatenCheck()
        {
            if (headPos == applePos)
            {
                GenerateNewApple();
                frame.scoreLabel.ChangeScore("Score: ", ++score);
            }
            else
            {
                sectionNums[tailPos.X, tailPos.Y] = 0;
                tailSetNum++;
            }
        }
        private void GenerateNewApple()
        {
            do applePos = new Point(appleRand.Next(0, sectionsInWidth_), appleRand.Next(0, sectionsInHeight_));
            while (sectionNums[applePos.X, applePos.Y] != 0);
        }
        
    }

    public void RunProgram()
    {
        frame = new();
        BuildStart();
        
        timer = new (timerInterval);
        timer.Elapsed += TimerListener;

        //anTimer = new (anTimerInterval);
        //anTimer.Elapsed += AnTimerListener;

        Application.Run(frame);
    }

    private void BuildStart()
    { 
        for(int i = 0; i < sectionsInWidth_ - 2; i++)
        for (int j = 0; j < sectionsInHeight_ - 2; j++)
            sectionNums[i, j] = 0;

        for(int i = 3; i < startingSnakeLength + 3; i++) //setting the body integers to > 0
            sectionNums[i, sectionsInHeight_ / 2] = ++sectionSetNum;
        
        frameEngine = new();
        
        frame.actionPanel.Refresh();
    }

    private static void GameEnd()
    {
        Console.WriteLine("You Crashed!");
        Environment.Exit(0);
    }

    private void TimerListener(object sender, ElapsedEventArgs e)
    {
        frameEngine.UpdateFrameAssets();
        frame.actionPanel.Refresh();
    }

    public static void DetectKey(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up:
                frameEngine.RepeatabilityCheck(0);
                //Console.WriteLine("<up "+ directionsTurn>);
                break;
                
            case Keys.Down:
                frameEngine.RepeatabilityCheck(1);
                //Console.WriteLine("<down "+ directionsTurn>);
                break;

            case Keys.Left:
                frameEngine.RepeatabilityCheck(2);
                //Console.WriteLine("<left "+ directionsTurn>);
                break;
            
            case Keys.Right:
                frameEngine.RepeatabilityCheck(3);
                //Console.WriteLine("<right "+ directionsTurn>);
                break;
            
            case Keys.Escape:
                if (gameRunning)
                {
                    gameRunning = false;
                    frame.scoreLabel.ChangeScore("Return with 'Enter' or 'Esc'", -1);
                    Console.WriteLine("The Game is Paused");
                    timer.Stop();
                }
                else 
                {
                    gameRunning = true;
                    frame.scoreLabel.ChangeScore("Score: ", frameEngine.score);
                    Console.WriteLine("The Game is Resumed");
                    timer.Start();
                }
                break;
            
            case Keys.Return: // = Enter
                if (!gameRunning)
                {
                    gameRunning = true;
                    Console.WriteLine("The Game Started!");
                    frame.scoreLabel.ChangeScore("Score: ", frameEngine.score);
                    timer.Start();
                }
                break;
            
        }
    }

}

static class Program 
{
    static void Main()
    {
        BackEnd engine = new();
        engine.RunProgram();
    }
}