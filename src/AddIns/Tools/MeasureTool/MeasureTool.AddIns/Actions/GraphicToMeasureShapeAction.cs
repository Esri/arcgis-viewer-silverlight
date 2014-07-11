/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Threading;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace MeasureTool.Addins
{
    public class GraphicToMeasureShapeAction : TriggerAction<DependencyObject>
    {
        private delegate void MethodInvoker();

        protected override void Invoke(object parameter)
        {
            //this.FinishedWork -= LengthArea_FinishedWork;
            //this.FinishedWork += LengthArea_FinishedWork;
            //Thread t = null;

            //if (this.Graphic != null)
            //{
            //    DrawMeasureShape shape = null;

            //    // get attributes from graphic if they exist
            //    if (this.Graphic.Attributes.ContainsKey("DrawMeasure") && this.Graphic.Attributes.ContainsKey("Value1AsString") &&
            //        this.Graphic.Attributes.ContainsKey("Value2AsString"))
            //    {
            //        if (this.Graphic.Geometry is MapPoint)
            //        {
            //            shape = new DrawMeasureShape();
            //            shape.Geometry = this.Graphic.Geometry;
            //            shape.GeometryType = ESRI.ArcGIS.Client.Tasks.GeometryType.Point;
            //            shape.Latitude = System.Convert.ToDouble(this.Graphic.Attributes["Value1AsString"]);
            //            shape.Longitude = System.Convert.ToDouble(this.Graphic.Attributes["Value2AsString"]);
            //        }
            //        else if (this.Graphic.Geometry is Polyline)
            //        {
            //            shape = new DrawMeasureShape();
            //            shape.GeometryType = ESRI.ArcGIS.Client.Tasks.GeometryType.Polyline;
            //            shape.Geometry = this.Graphic.Geometry;
            //            shape.Length = System.Convert.ToDouble(this.Graphic.Attributes["Value1AsString"]);
            //        }
            //        else if (this.Graphic.Geometry is Polygon)
            //        {
            //            shape = new DrawMeasureShape();
            //            shape.GeometryType = ESRI.ArcGIS.Client.Tasks.GeometryType.Polygon;
            //            shape.Geometry = this.Graphic.Geometry;
            //            shape.Length = System.Convert.ToDouble(this.Graphic.Attributes["Value1AsString"]);
            //            shape.Area = System.Convert.ToDouble(this.Graphic.Attributes["Value2AsString"]);
            //        }
            //    }
            //    // else got to do some calculations ...
            //    else
            //    {
            //        if (this.Graphic.Geometry is MapPoint)
            //        {
            //            shape = new DrawMeasureShape();
            //            shape.GeometryType = ESRI.ArcGIS.Client.Tasks.GeometryType.Point;
            //            shape.Geometry = this.Graphic.Geometry;

            //            // calculate
            //            shape.Latitude = (this.Graphic.Geometry as MapPoint).Y;
            //            shape.Longitude = (this.Graphic.Geometry as MapPoint).X;
            //        }
            //        else if (this.Graphic.Geometry is MultiPoint)
            //        {
            //            MultiPoint multiPoint = this.Graphic.Geometry as MultiPoint;

            //            // take the first point
            //            shape = new DrawMeasureShape();
            //            shape.Geometry = multiPoint.Points[0];
            //            shape.GeometryType = ESRI.ArcGIS.Client.Tasks.GeometryType.Point;

            //            // calculate
            //            shape.Latitude = (shape.Geometry as MapPoint).Y;
            //            shape.Longitude = (shape.Geometry as MapPoint).X;
            //        }
            //        else if (this.Graphic.Geometry is Polyline)
            //        {
            //            shape = new DrawMeasureShape();
            //            shape.Geometry = this.Graphic.Geometry;
            //            shape.GeometryType = ESRI.ArcGIS.Client.Tasks.GeometryType.Polyline;

            //            // calculate in a thread
            //            t = new Thread(new ParameterizedThreadStart(DoWork));
            //            t.Start(shape);
            //        }
            //        else if (this.Graphic.Geometry is Polygon)
            //        {
            //            shape = new DrawMeasureShape();
            //            shape.Geometry = this.Graphic.Geometry;
            //            shape.GeometryType = ESRI.ArcGIS.Client.Tasks.GeometryType.Polygon;

            //            // calculate in a thread
            //            t = new Thread(new ParameterizedThreadStart(DoWork));
            //            t.Start(shape);
            //        }
            //    }

            //    // assign if not in a thread
            //    if (t == null)
            //    {
            //        this.MeasureShape = shape;
            //    }
            //}
        }

        //void LengthArea_FinishedWork(object sender, DrawMeasureShape e)
        //{
        //    if (!this.Dispatcher.CheckAccess())
        //    {
        //        this.Dispatcher.BeginInvoke(new MethodInvoker(delegate() { this.LengthArea_FinishedWork(sender, e); }));
        //    }
        //    else
        //    {
        //        this.MeasureShape = e;
        //    }
        //}

        //public event FinishedWorkHandler FinishedWork;
        //public delegate void FinishedWorkHandler(object sender, DrawMeasureShape e);

        public void DoWork(object data)
        {
            //DrawMeasureShape shp = data as DrawMeasureShape;

            //// calculate
            //double len, area;
            //Utils.GetLengthAndArea(shp.Geometry, true, out len, out area);

            //// assign values
            //shp.Length = len;
            //if (shp.Geometry is Polygon)
            //{
            //    shp.Area = area;
            //}

            //// throw event
            //if (this.FinishedWork != null)
            //{
            //    this.FinishedWork(this, shp);
            //}
        }

        #region Dependency Properties
        //public static DependencyProperty MeasureShapeProperty = DependencyProperty.Register(
        //    "MeasureShape", typeof(DrawMeasureShape), typeof(GraphicToMeasureShapeAction), null);

        //public DrawMeasureShape MeasureShape
        //{
        //    get
        //    {
        //        return this.GetValue(MeasureShapeProperty) as DrawMeasureShape;
        //    }
        //    set
        //    {
        //        this.SetValue(MeasureShapeProperty, value);
        //    }
        //}

        public static DependencyProperty GraphicProperty = DependencyProperty.Register(
            "GraphicProperty", typeof(Graphic), typeof(GraphicToMeasureShapeAction), null);

        public Graphic Graphic
        {
            get
            {
                return this.GetValue(GraphicProperty) as Graphic;
            }
            set
            {
                this.SetValue(GraphicProperty, value);
            }
        }
        #endregion Dependency Properties
    }
}
