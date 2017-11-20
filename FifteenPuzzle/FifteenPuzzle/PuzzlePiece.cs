using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FifteenPuzzle
{
    public class PuzzlePiece : ContentView
    {
        Label Label;
        string NormContent;
        string WinContent;

        public int Index { get; private set; }

        public int Row { get; set; }

        public int Col { get; set; }

        public PuzzlePiece(char contentChar, char winChar, int index, string img)
        {
            this.NormContent = contentChar.ToString();
            this.WinContent = winChar.ToString();
            this.Index = index;

            // The main letters
            Label = new Label
            {
                Text = this.NormContent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            // The Image
            Image image = new Image
            {
                Source = ImageSource.FromResource(img),
                Aspect = Aspect.Fill,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };

            // Small piece number.
            Label PieceNumber = new Label
            {
                Text = (index + 1).ToString(),
                TextColor = Color.Red,
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End
            };

            RelativeLayout Layout = new RelativeLayout();
            Layout.Children.Add(image, Constraint.Constant(0),
                                        Constraint.Constant(0),
                                        Constraint.RelativeToParent((parent) => { return parent.Width; }),
                                        Constraint.RelativeToParent((parent) => { return parent.Height; }));

            Layout.Children.Add(Label, Constraint.Constant(0),
                                        Constraint.Constant(0),
                                        Constraint.RelativeToParent((parent) => { return parent.Width; }),
                                        Constraint.RelativeToParent((parent) => { return parent.Height; }));
            Layout.Children.Add(PieceNumber, Constraint.Constant(0),
                                            Constraint.Constant(0),
                                            Constraint.RelativeToParent((parent) => { return parent.Width; }),
                                            Constraint.RelativeToParent((parent) => { return parent.Height; }));

            // Creates the spacing between the pieces and the spacing from the edges.
            //this.Padding = new Thickness(3);
            this.Content = new Frame
            {
                OutlineColor = Color.Accent,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Content = Layout,
                BackgroundColor = Color.Bisque
            };
            this.BackgroundColor = Color.Transparent;
        }

        public async Task AnimateWin(bool isReverse)
        {
            uint length = 150;
            await Task.WhenAll(this.ScaleTo(3, length), this.RotateTo(180, length));
            Label.Text = isReverse ? NormContent : WinContent;
            await Task.WhenAll(this.ScaleTo(1, length), this.RotateTo(360, length));
            this.Rotation = 0;
        }

        public void SetLabelFont(double fontSize, FontAttributes attributes)
        {
            Label.FontSize = fontSize;
            Label.FontAttributes = attributes;
        }
    }
}