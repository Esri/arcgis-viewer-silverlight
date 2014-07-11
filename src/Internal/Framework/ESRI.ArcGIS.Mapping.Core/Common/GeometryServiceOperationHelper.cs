/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Bing;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class GeometryServiceOperationHelper
    {
        const string FallbackGeometryServer = "http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer";
        private static readonly SpatialReference Geographic = new SpatialReference((int)WKIDs.Geographic);
        private static readonly SpatialReference WebMercator = new SpatialReference((int)WKIDs.WebMercator);
        private static readonly SpatialReference WebMercatorAuxillarySphere = new SpatialReference((int)WKIDs.WebMercatorAuxiliarySphere);
        private static readonly SpatialReference RevisedWebMercatorAuxSphere = new SpatialReference((int)WKIDs.RevisedWebMercatorAuxSphere);

        private string GeometryServiceUrl;
        private string ProxyUrl;
        private delegate void ProjectCompleteDelegate(object o, GraphicsEventArgs args);

        public GeometryServiceOperationHelper(string geometryServiceUrl, string proxyUrl = null)
        {
            GeometryServiceUrl = geometryServiceUrl;
            ProxyUrl = proxyUrl;
        }        

        public void ProjectExtent(Envelope envelope, SpatialReference outputSpatialReference, object userState = null)
        {
            if (outputSpatialReference == null)
                throw new ArgumentNullException("outputSpatialReference");
            if (outputSpatialReference.Equals(envelope.SpatialReference))
            {
                OnProjectExtentCompleted(new ProjectExtentCompletedEventArgs() { Extent = envelope });
                return;
            }

            // Use client side transform (if possible)
            if (Geographic.Equals(envelope.SpatialReference) 
                && (WebMercator.Equals(outputSpatialReference) 
                || WebMercatorAuxillarySphere.Equals(outputSpatialReference) 
                || RevisedWebMercatorAuxSphere.Equals(outputSpatialReference)))
            {
                MapPoint p1 = new MapPoint(envelope.XMin, envelope.YMin);
                MapPoint p2 = new MapPoint(envelope.XMax, envelope.YMax);
                Envelope output = new Envelope(Transform.GeographicToWebMercator(p1),
                                               Transform.GeographicToWebMercator(p2));
                output.SpatialReference = outputSpatialReference;
                OnProjectExtentCompleted(new ProjectExtentCompletedEventArgs() { Extent = output });
                return;
            }
            else if (Geographic.Equals(outputSpatialReference) 
                && (WebMercator.Equals(envelope.SpatialReference) 
                || WebMercatorAuxillarySphere.Equals(envelope.SpatialReference) 
                || RevisedWebMercatorAuxSphere.Equals(envelope.SpatialReference)))
            {
                MapPoint p1 = new MapPoint(envelope.XMin, envelope.YMin);
                MapPoint p2 = new MapPoint(envelope.XMax, envelope.YMax);
                Envelope output = new Envelope(Transform.WebMercatorToGeographic(p1),
                                               Transform.WebMercatorToGeographic(p2));
                output.SpatialReference = outputSpatialReference;
                OnProjectExtentCompleted(new ProjectExtentCompletedEventArgs() { Extent = output });
                return;
            }

            if (string.IsNullOrEmpty(GeometryServiceUrl))
                GeometryServiceUrl = FallbackGeometryServer;

            List<Graphic> graphics = new List<Graphic>(1);
            graphics.Add(new Graphic() { Geometry = envelope});
            projectGraphicsAsync(GeometryServiceUrl, graphics, outputSpatialReference, geomService_ExtentProjectCompleted, userState);
        }

        public void ProjectGraphics(IList<Graphic> graphics, SpatialReference outputSpatialReference, object userState = null)
        {
            if (string.IsNullOrEmpty(GeometryServiceUrl))
                GeometryServiceUrl = FallbackGeometryServer;

            projectGraphicsAsync(GeometryServiceUrl, graphics, outputSpatialReference, geomService_GraphicsProjectCompleted, userState);
        }

        public void ProjectPoints(PointCollection points, SpatialReference outputSpatialReference, object userState = null)
        {
            if (points == null)
                throw new ArgumentNullException("points");
            if (string.IsNullOrEmpty(GeometryServiceUrl))
                GeometryServiceUrl = FallbackGeometryServer;
            List<Graphic> graphics = new List<Graphic>(points.Count);
            foreach (MapPoint pt in points)
                graphics.Add(new Graphic() { Geometry = pt });
            projectGraphicsAsync(GeometryServiceUrl, graphics, outputSpatialReference, geomService_PointsProjectCompleted, userState);
        }

        public void ProjectAddressCandidatesAsync(List<AddressCandidate> addressCandidates, SpatialReference outputSpatialReference, object userState = null)
        {
            if (string.IsNullOrEmpty(GeometryServiceUrl))
                GeometryServiceUrl = FallbackGeometryServer;

            List<Graphic> graphics = GetGraphicsFromAddressCandidates(addressCandidates);

            projectGraphicsAsync(GeometryServiceUrl, graphics, outputSpatialReference, geomService_AddressCandidatesProjectCompleted, userState);
        }
        private List<Graphic> GetGraphicsFromAddressCandidates(IList<AddressCandidate> addressCandidates)
        {
            List<Graphic> graphics = new List<Graphic>();
            foreach (AddressCandidate candidate in addressCandidates)
            {
                Graphic graphic = new Graphic()
                {
                    Geometry = candidate.Location,
                };
                if (candidate.Attributes != null)
                {
                    foreach (KeyValuePair<string, object> keyValuePair in candidate.Attributes)
                        graphic.Attributes.Add(keyValuePair.Key, keyValuePair.Value);
                }
                if (!string.IsNullOrEmpty(candidate.Address))
                    graphic.Attributes["^^^address^^^"] = candidate.Address;
                if (candidate.Score != default(int))
                    graphic.Attributes["^^^score^^^"] = candidate.Score;
                graphics.Add(graphic);
            }
            return graphics;
        }

        private List<AddressCandidate> GetAddressCandidatesFromGraphics(IList<Graphic> graphics)
        {
            return GetAddressCandidatesFromGraphics(graphics, true);
        }
        private List<AddressCandidate> GetAddressCandidatesFromGraphics(IList<Graphic> graphics, bool addAttributes)
        {
            List<AddressCandidate> candidates = new List<AddressCandidate>();
            foreach (Graphic graphic in graphics)
            {
                object o;
                string addr = null;
                if (graphic.Attributes.TryGetValue("^^^address^^^", out o))
                    addr = o as string;
                int score = default(int);
                if (graphic.Attributes.TryGetValue("^^^score^^^", out o))
                {
                    if (o is int)
                        score = (int)o;
                }
                Dictionary<string, object> dict = null;
                if (graphic.Attributes != null)
                {
                    dict = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> keyValuePair in graphic.Attributes)
                        dict.Add(keyValuePair.Key, keyValuePair.Value);
                }
                AddressCandidate candidate = new AddressCandidate(addr, graphic.Geometry as MapPoint, score, addAttributes ? dict : null);
                candidates.Add(candidate);
            }
            return candidates;
        }

        void projectGraphicsAsync(string geometryServiceUrl, IList<Graphic> graphics, SpatialReference outputSpatialReference,
            ProjectCompleteDelegate onProjectGeometryComplete, object userState)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (graphics.Count < 1)
            {
                OnProjectExtentComplete(new ProjectGraphicsCompletedEventArgs() { Graphics = graphics });
                return;
            }

            if (outputSpatialReference.WKID == 4326 && graphics.All(g => g.Geometry != null 
                && g.Geometry.SpatialReference != null && isWebMercator(g.Geometry.SpatialReference)))
            {
                // Project from web mercator to GCS client-side
                var wm = new ESRI.ArcGIS.Client.Projection.WebMercator();
                List<Graphic> outGraphics = new List<Graphic>();
                foreach (var g in graphics)
                {
                    var outGraphic = cloneGraphic(g);
                    outGraphic.Geometry = wm.ToGeographic(g.Geometry);
                    outGraphics.Add(outGraphic);
                }
                OnProjectExtentComplete(new ProjectGraphicsCompletedEventArgs() { Graphics = outGraphics });
            }
            else if (isWebMercator(outputSpatialReference) && graphics.All(g => g.Geometry != null
                && g.Geometry.SpatialReference != null && g.Geometry.SpatialReference.WKID == 4326))
            {
                // Project from GCS to web mercator client-side
                var wm = new ESRI.ArcGIS.Client.Projection.WebMercator();
                List<Graphic> outGraphics = new List<Graphic>();
                foreach (var g in graphics)
                {
                    var outGraphic = cloneGraphic(g);
                    outGraphic.Geometry = wm.FromGeographic(g.Geometry);
                    outGraphics.Add(outGraphic);
                }
                OnProjectExtentComplete(new ProjectGraphicsCompletedEventArgs() { Graphics = outGraphics });
            }
            else
            {
                GeometryService geomService = new GeometryService(geometryServiceUrl) { ProxyURL = this.ProxyUrl };
                geomService.ProjectCompleted += (o, e) =>
                {
                    int len1 = graphics.Count;
                    int len2 = e.Results.Count;
                    System.Diagnostics.Debug.Assert(len1 == len2, Resources.Strings.ExceptionNumberofGraphicsBeforeAndAfterProjectionAreNotEqual);
                    for (int i = 0; i < len1 && i < len2; i++)
                    {
                        var targetGraphic = cloneGraphic(graphics[i]);
                        targetGraphic.Geometry = e.Results[i].Geometry;
                        e.Results[i] = targetGraphic;                        
                    }
                    onProjectGeometryComplete(o, e);
                };
                geomService.Failed += (o, e) =>
                {
                    if ((e.Error is ESRI.ArcGIS.Client.Tasks.ServiceException || e.Error is System.Net.WebException) && geometryServiceUrl != FallbackGeometryServer)
                    {
                        projectGraphicsAsync(FallbackGeometryServer, graphics, outputSpatialReference, onProjectGeometryComplete, e.UserState);
                    }
                    else
                    {
                        OnGeometryServiceOperationFailed(new ExceptionEventArgs(new Exception(e.Error == null ? e.Error.Message : ""), e.UserState));
                    }
                };
                geomService.ProjectAsync(graphics, outputSpatialReference, userState);
            }
        }

        void geomService_AddressCandidatesProjectCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results == null || e.Results.Count < 1)
            {
                OnGeometryServiceOperationFailed(new Core.ExceptionEventArgs(new Exception(Resources.Strings.ExceptionNoResultsFromProjection), e.UserState));
                return;
            }

            List<AddressCandidate> candidates = GetAddressCandidatesFromGraphics(e.Results);

            OnProjectAddressCandidatesCompleted(new ProjectAddressCandidatesCompletedEventArgs() { AddressCandidates = candidates, UserState = e.UserState });
        }

        void geomService_ExtentProjectCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results == null || e.Results.Count < 1)
            {
                OnGeometryServiceOperationFailed(new Core.ExceptionEventArgs(new Exception(Resources.Strings.ExceptionNoResultsFromProjection), e.UserState));
                return;
            }
            Graphic g = e.Results[0];
            OnProjectExtentCompleted(new ProjectExtentCompletedEventArgs() { Extent = g.Geometry as Envelope, UserState = e.UserState });
        }

        void geomService_PointsProjectCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results == null || e.Results.Count < 1)
            {
                OnGeometryServiceOperationFailed(new Core.ExceptionEventArgs(new Exception(Resources.Strings.ExceptionNoResultsFromProjection), e.UserState));
                return;
            }
            PointCollection points = new PointCollection();
            foreach (Graphic g in e.Results)
            {
                points.Add(g.Geometry as MapPoint);
            }

            OnProjectPointsCompleted(new ProjectPointsCompletedEventArgs() { Points = points, UserState = e.UserState });
        }

        void geomService_GraphicsProjectCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results == null)
            {
				OnGeometryServiceOperationFailed(new Core.ExceptionEventArgs(new Exception(Resources.Strings.ExceptionNoResultsFromProjection), e.UserState));
                return;
            }
            OnProjectExtentComplete(new ProjectGraphicsCompletedEventArgs() { Graphics = e.Results, UserState = e.UserState });
        }

        protected virtual void OnGeometryServiceOperationFailed(Core.ExceptionEventArgs args)
        {
            if (GeometryServiceOperationFailed != null)
                GeometryServiceOperationFailed(this, args);
        }

        protected virtual void OnProjectExtentCompleted(ProjectExtentCompletedEventArgs args)
        {
            if (ProjectExtentCompleted != null)
                ProjectExtentCompleted(this, args);
        }

        protected virtual void OnProjectPointsCompleted(ProjectPointsCompletedEventArgs args)
        {
            if (ProjectPointsCompleted != null)
                ProjectPointsCompleted(this, args);
        }

        protected virtual void OnProjectExtentComplete(ProjectGraphicsCompletedEventArgs args)
        {
            if (ProjectGraphicsCompleted != null)
                ProjectGraphicsCompleted(this, args);
        }

        protected virtual void OnProjectAddressCandidatesCompleted(ProjectAddressCandidatesCompletedEventArgs args)
        {
            if (ProjectAddressCandidatesCompleted != null)
                ProjectAddressCandidatesCompleted(this, args);
        }

        public event EventHandler<ProjectAddressCandidatesCompletedEventArgs> ProjectAddressCandidatesCompleted;
        public event EventHandler<ProjectGraphicsCompletedEventArgs> ProjectGraphicsCompleted;
        public event EventHandler<ProjectExtentCompletedEventArgs> ProjectExtentCompleted;
        public event EventHandler<ProjectPointsCompletedEventArgs> ProjectPointsCompleted;
        public event EventHandler<ExceptionEventArgs> GeometryServiceOperationFailed;

        private bool isWebMercator(SpatialReference sr)
        {
            return sr.WKID == 3857 || sr.WKID == 102100 || sr.WKID == 102113;
        }

        private Graphic cloneGraphic(Graphic sourceGraphic)
        {
            var targetGraphic = new Graphic();
            if (sourceGraphic.Attributes != null)
            {
                foreach (KeyValuePair<string, object> pair in sourceGraphic.Attributes)
                    targetGraphic.Attributes.Add(pair);
            }

            targetGraphic.Symbol = sourceGraphic.Symbol;
            targetGraphic.Selected = sourceGraphic.Selected;
            targetGraphic.TimeExtent = sourceGraphic.TimeExtent;
            targetGraphic.Geometry = sourceGraphic.Geometry;

            return targetGraphic;
        }
    }

    public class ProjectExtentCompletedEventArgs : EventArgs
    {
        public Envelope Extent;
        public object UserState;
    }

    public class ProjectPointsCompletedEventArgs : EventArgs
    {
        public PointCollection Points;
        public object UserState;
    }

    public class ProjectGraphicsCompletedEventArgs : EventArgs
    {
        public IList<Graphic> Graphics;
        public object UserState;
    }

    public class ProjectAddressCandidatesCompletedEventArgs : EventArgs
    {
        public List<AddressCandidate> AddressCandidates;
        public object UserState;
    }

    internal enum WKIDs
    {
        CylindricalEqualAreaWorld = 54034,
        RevisedWebMercatorAuxSphere = 3857,
        WebMercatorAuxiliarySphere = 102100,
        WebMercator = 102113,
        Geographic = 4326
    }
}
