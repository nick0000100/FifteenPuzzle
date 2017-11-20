using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FifteenPuzzle
{
    public class PuzzlePage : ContentPage
    {
        // Size
        static readonly int NUM = 4;

        // Pieces and empty space
        PuzzlePiece[,] Pieces = new PuzzlePiece[NUM, NUM];
        int EmptyRow = NUM - 1;
        int EmptyCol = NUM - 1;

        // Makes the things that will go on the page and checks/info.
        StackLayout Holder;
        AbsoluteLayout Board;
        Button ShuffleButton;
        Picker BackgroundSelect;
        string Background;
        double SquareSize;
        bool CurrentAction;
        bool IsPlaying;

        public PuzzlePage()
        {
            // Remove later
            IsPlaying = true;
            Board = new AbsoluteLayout()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Creates the picker
            BackgroundSelect = new Picker
            {
                Title = "Choose a background",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            BackgroundSelect.Items.Add("None");
            BackgroundSelect.Items.Add("Kirby");
            BackgroundSelect.Items.Add("Dojo");
            BackgroundSelect.Items.Add("CSharp");

            BackgroundSelect.SelectedIndexChanged += (sender, args) =>
            {
                Background = BackgroundSelect.Items[BackgroundSelect.SelectedIndex];
                if(Background == "None")
                {
                    MakePieces();
                }
                else
                {
                    MakePieces(Background);
                }
            };

            MakePieces();

            // Makes button
            ShuffleButton = new Button
            {
                Text = "Shuffle",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            ShuffleButton.Clicked += Shuffle;

            // Attaches the stuff to the things.
            Holder = new StackLayout
            {
                Children =
                {
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Children =
                        {
                            ShuffleButton,
                            BackgroundSelect
                        }
                    },
                    Board
                },
            };
            Holder.SizeChanged += OnStackSizeChanged;

            this.Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0);
            this.Content = Holder;
        }

        void MakePieces(string background = null)
        {
            Board.Children.Clear();
            EmptyCol = NUM - 1;
            EmptyRow = NUM - 1;

            // Things to display on pieces.
            string PuzzleContent = @"<coding.dojo!/>";
            string WinContent = "CONGRATULATIONS";
            int Index = 0;

            // Creates pieces.
            for (int row = 0; row < NUM; row++)
            {
                for (int col = 0; col < NUM; col++)
                {
                    if (row == NUM - 1 && col == NUM - 1)
                    {
                        break;
                    }
                    PuzzlePiece Piece;
                    
                    if(background == null)
                    {
                        Piece = new PuzzlePiece(PuzzleContent[Index], WinContent[Index], Index, " ")
                        {
                            Row = row,
                            Col = col
                        };
                    }
                    else
                    {
                        Piece = new PuzzlePiece(' ', WinContent[Index], Index, $"FifteenPuzzle.Images.{background}.part{Index}.jpg")
                        {
                            Row = row,
                            Col = col
                        };
                    }
                    

                    // Adds tapping functionality.
                    TapGestureRecognizer TapGestureRecognizer = new TapGestureRecognizer
                    {
                        Command = new Command(OnPieceTapped),
                        CommandParameter = Piece
                    };
                    Piece.GestureRecognizers.Add(TapGestureRecognizer);

                    Pieces[row, col] = Piece;
                    Board.Children.Add(Piece);
                    Index++;
                }
            }
            Position();
        }

        // When a piece is touched.
        async void OnPieceTapped(object sender)
        {
            if (CurrentAction)
            {
                return;
            }
            CurrentAction = true;
            PuzzlePiece Tapped = (PuzzlePiece)sender;
            await Move(Tapped.Row, Tapped.Col);
            CurrentAction = false;

            if (IsPlaying)
            {
                int Index;
                for (Index = 0; Index < NUM * NUM - 1; Index++)
                {
                    int Row = Index / NUM;
                    int Col = Index % NUM;
                    PuzzlePiece Piece = Pieces[Row, Col];
                    if (Piece == null || Piece.Index != Index)
                    {
                        break;
                    }
                }
                if (Index == NUM * NUM - 1)
                {
                    // UNCOMMENT LATER
                    //IsPlaying = false;
                    await Win();
                }
            }
        }

        // Checks if the tapped piece is able to move the moves it,
        // does nothing if it is an unmovable piece.
        async Task Move(int oldRow, int oldCol, uint length = 100)
        {
            if (oldRow == EmptyRow && oldCol != EmptyCol)
            {
                int Direction = Math.Sign(oldCol - EmptyCol);
                int StartCol = EmptyCol + Direction;
                int EndCol = oldCol + Direction;

                for (int col = StartCol; col != EndCol; col += Direction)
                {
                    await Movement(EmptyRow, col, EmptyRow, EmptyCol, length);
                }
            }
            else if (oldCol == EmptyCol && oldRow != EmptyRow)
            {
                int Direction = Math.Sign(oldRow - EmptyRow);
                int StartRow = EmptyRow + Direction;
                int EndRow = oldRow + Direction;

                for (int row = StartRow; row != EndRow; row += Direction)
                {
                    await Movement(row, EmptyCol, EmptyRow, EmptyCol, length);
                }
            }
        }

        // Moves the piece that was tapped.
        async Task Movement(int row, int col, int newRow, int newCol, uint length)
        {
            PuzzlePiece MovePiece = Pieces[row, col];

            Rectangle NewLocation = new Rectangle(SquareSize * EmptyCol, SquareSize * EmptyRow, SquareSize, SquareSize);

            await MovePiece.LayoutTo(NewLocation, length);

            Pieces[newRow, newCol] = MovePiece;
            MovePiece.Row = newRow;
            MovePiece.Col = newCol;
            Pieces[row, col] = null;
            EmptyCol = col;
            EmptyRow = row;
        }

        // Shuffles things.
        async void Shuffle(object sender, EventArgs args)
        {
            Button Button = sender as Button;
            Button.IsEnabled = false;
            BackgroundSelect.IsEnabled = false;
            Random rand = new Random();

            CurrentAction = true;

            for (int i = 0; i < 100; i++)
            {
                await Move(rand.Next(NUM), EmptyCol, 25);
                await Move(EmptyRow, rand.Next(NUM), 25);
            }

            Button.IsEnabled = true;
            BackgroundSelect.IsEnabled = true;
            CurrentAction = false;

            this.IsPlaying = true;
        }

        // Changes the content based on device orientation.
        void OnStackSizeChanged(object sender, EventArgs args)
        {
            double Width = Holder.Width;
            double Height = Holder.Height;

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            Holder.Orientation = (Width < Height) ? StackOrientation.Vertical : StackOrientation.Horizontal;

            SquareSize = Math.Min(Width, Height) / NUM;
            Board.WidthRequest = NUM * SquareSize;
            Board.HeightRequest = NUM * SquareSize;

            Position();
        }

        // Positions the pieces correctly
        void Position()
        {
            foreach (View view in Board.Children)
            {
                PuzzlePiece Piece = (PuzzlePiece)view;
                Piece.SetLabelFont(0.4 * SquareSize, FontAttributes.Bold);

                Rectangle NewRectagle = new Rectangle(Piece.Col * SquareSize, Piece.Row * SquareSize, SquareSize, SquareSize);

                AbsoluteLayout.SetLayoutBounds(Piece, NewRectagle);
            }
        }

        // Calls the win animation on the pieces.
        async Task Win()
        {
            ShuffleButton.IsEnabled = false;
            BackgroundSelect.IsEnabled = false;
            CurrentAction = true;
            for(int spin = 0; spin < 2; spin++)
            {
                foreach(PuzzlePiece Piece in Pieces)
                {
                    if(Piece != null)
                    {
                        await Piece.AnimateWin(spin == 1);
                    }
                }
                if(spin == 0)
                {
                    await Task.Delay(1500);
                }
            }
            ShuffleButton.IsEnabled = true;
            BackgroundSelect.IsEnabled = true;
            CurrentAction = false;
        }

    }
}