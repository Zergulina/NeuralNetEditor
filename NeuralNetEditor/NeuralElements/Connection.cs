using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeuralNetEditor.NeuralElements
{
    internal class Connection
    {
        public Connection(NeuralLayer startLayer) {
            DrawableConnection = new Path();
            DrawableConnection.Stroke = Brushes.Black;
            DrawableConnection.StrokeThickness = 2;
            StartLayer = startLayer;
            EndPoint = new Point(Canvas.GetLeft(startLayer.DrawableLayer) + startLayer.DrawableLayer.Width / 2, Canvas.GetTop(startLayer.DrawableLayer) + startLayer.DrawableLayer.Height / 2);
        }

        public Connection(NeuralLayer startLayer, NeuralLayer endLayer)
        {
            DrawableConnection = new Path();
            DrawableConnection.Stroke = Brushes.Black;
            DrawableConnection.StrokeThickness = 2;
            StartLayer = startLayer;
            EndLayer = endLayer;
        }
        private Point startPoint;
        public Point StartPoint {
            get => startPoint;
            set {
                startPoint = value;
                BuildDrawableConnection();
            }
        }

        private Point endPoint;
        public Point EndPoint {
            get => endPoint;
            set
            {
                endPoint = value;
                BuildDrawableConnection();
            }
        }
        public NeuralLayer startLayer;
        public NeuralLayer StartLayer
        {
            get => startLayer;
            set
            {
                startLayer = value;
                StartPoint = new Point(Canvas.GetLeft(StartLayer.DrawableLayer) + StartLayer.DrawableLayer.Width / 2, Canvas.GetTop(StartLayer.DrawableLayer) + StartLayer.DrawableLayer.Height / 2);
            }
        }
        public NeuralLayer? endLayer;
        public NeuralLayer? EndLayer
        {
            get => endLayer;
            set
            {
                endLayer = value;
                if (EndLayer != null)
                {
                    EndPoint = new Point(Canvas.GetLeft(EndLayer.DrawableLayer) + EndLayer.DrawableLayer.Width / 2, Canvas.GetTop(EndLayer.DrawableLayer) + EndLayer.DrawableLayer.Height / 2);
                }
            }
        }
        public Path DrawableConnection { get; private set; }

        private void BuildDrawableConnection()
        {
            GeometryGroup geometryGroup = new GeometryGroup();

            LineGeometry lineGeometry = new LineGeometry(new Point(StartPoint.X, StartPoint.Y), new Point(EndPoint.X, EndPoint.Y));
            geometryGroup.Children.Add(lineGeometry);

            Point middlePoint = new Point((EndPoint.X + StartPoint.X) / 2, (EndPoint.Y + StartPoint.Y) / 2);

            double d = Math.Sqrt(Math.Pow(StartPoint.X - EndPoint.X, 2) + Math.Pow(StartPoint.Y - EndPoint.Y, 2));
            double X = StartPoint.X - EndPoint.X;
            double Y = StartPoint.Y - EndPoint.Y;

            double X4 = middlePoint.X - (X / d) * 10;
            double Y4 = middlePoint.Y - (Y / d) * 10;

            double Xp = StartPoint.Y - EndPoint.Y;
            double Yp = EndPoint.X - StartPoint.X;

            double norm = Math.Sqrt(Math.Pow(Xp, 2) + Math.Pow(Yp, 2));
            Xp /= norm;
            Yp /= norm;

            // Поворот на 30 градусов (π/6 радиан)
            double angle1 = Math.PI / 6;
            double angle2 = -Math.PI / 6 - Math.PI;
            double XpLeft = Xp * Math.Cos(angle1) - Yp * Math.Sin(angle1);
            double YpLeft = Xp * Math.Sin(angle1) + Yp * Math.Cos(angle1);

            double XpRight = Xp * Math.Cos(angle2) - Yp * Math.Sin(angle2);
            double YpRight = Xp * Math.Sin(angle2) + Yp * Math.Cos(angle2);

            double length = 10; // Длина стрелки
            double X5 = middlePoint.X + XpLeft * length;
            double Y5 = middlePoint.Y + YpLeft * length;

            double X6 = middlePoint.X + XpRight * length;
            double Y6 = middlePoint.Y + YpRight * length;

            PathFigure arrowPathFigure = new PathFigure();

            LineGeometry lineGeometryE = new LineGeometry(new Point(X4, Y4), new Point(X5, Y5));
            geometryGroup.Children.Add(lineGeometryE);

            LineGeometry lineGeometryF = new LineGeometry(new Point(X4, Y4), new Point(X6, Y6));
            geometryGroup.Children.Add(lineGeometryF);

            DrawableConnection.Data = geometryGroup;
        }
    }
}
