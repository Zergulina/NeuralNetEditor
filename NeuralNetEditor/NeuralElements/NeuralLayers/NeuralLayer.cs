using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeuralNetEditor.NeuralElements.NeuralLayers
{
    internal abstract class NeuralLayer : INotifyPropertyChanged
    {
        public NeuralLayer()
        {
            Canvas layer = new Canvas();
            layer.Width = 150;
            layer.Height = 100;
            DrawableLayer = layer;

            DrawableLayer.Background = new SolidColorBrush(Colors.White);

            var borderRect = new Rectangle();
            borderRect.Width = DrawableLayer.Width;
            borderRect.Height = DrawableLayer.Height;
            borderRect.Stroke = Brushes.Black;
            borderRect.StrokeThickness = 2;
            DrawableLayer.Children.Add(borderRect);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public byte InputSize { get; protected set; }
        public byte OutputSize { get; protected set; }
        public List<Connection> InConnections { get; } = new List<Connection>();
        public List<Connection> OutConnections { get; } = new List<Connection>();
        public Canvas DrawableLayer { get; protected set; } = null!;
        protected bool isSelected;
        public bool IsSelected {
            get => isSelected;
            set {
                if (isSelected != value)
                {
                    isSelected = value;
                    if (isSelected)
                    {
                        DrawableLayer.Background = new SolidColorBrush(Colors.LightSeaGreen);
                    }
                    else
                    {
                        DrawableLayer.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }
        protected bool isError;
        public bool IsError
        {
            get => isError;
            set
            {
                if (isError != value)
                {
                    isError = value;
                    if (isError)
                    {
                        DrawableLayer.Background = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        DrawableLayer.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }
        public abstract string ConvertToSafeRecord(double xCameraOffset, double yCameraOffset);
        public abstract string ConvertToKeras();
        public abstract List<NeuralLayer> CheckPreviosLayersCompatibility();
    }
}
