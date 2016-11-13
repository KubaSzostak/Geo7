using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
#endif

namespace System
{

    public class AcPolygonSegment
    {
        public AcPolygonSegment(Line ln)
        {
            this.StartPoint = ln.StartPoint.Convert2d();
            this.EndPoint = ln.EndPoint.Convert2d();
            this.Azimuth = ln.Angle;
        }

        private AcPolygonSegment(Point2d s, Point2d e, Angle a)
        {
            this.StartPoint = s;
            this.EndPoint = e;
            this.Azimuth = a;
        }

        public AcPolygonSegment Reverse()
        {
            return new AcPolygonSegment(EndPoint, StartPoint, Azimuth - new Angle(180, 0, 0));
        }

        public string Id
        {
            get { return Azimuth.AsDeg.ToString("0.000000") + "; " + StartPoint.Id() + "; " + EndPoint.Id(); }
        }


        public Point2d StartPoint {get; private set; }
        public Point2d EndPoint { get; private set; }
        public Angle Azimuth { get; private set; }
        public Angle TempAzimuth { get; set; }
    }


    public class AcPolygonsCreator
    {
        private List<AcPolygonSegment> Lines = new List<AcPolygonSegment>();
        private Dictionary<string, List<AcPolygonSegment>> LinesDict = new Dictionary<string, List<AcPolygonSegment>>();

        public static void Run()
        {
            var pl = new AcPolygonsCreator(); 
           
            pl.LoadLinesData();
            pl.BuildPolygons();
            AcRemoveDuplicatedPolygons.Run();
        }


        public int LoadLinesData()
        {
            Lines.Clear();
            LinesDict.Clear();
            int res = 0;
            using(var trans = Ac.StartTransaction())
	        {
                var acLines = trans.GetAllEntities<Line>();
                Ac.InitProgress(AppServices.Strings.LoadingLines, acLines.Count());
                foreach (var acLn in acLines)
                {
                    if (acLn.Length < 0.000001)
                        continue;
                    var ln = new AcPolygonSegment(acLn);
                    var lnRev = ln.Reverse();

                    Lines.Add(ln);
                    LinesDict.AddListItem(ln.StartPoint.Id(), ln);

                    // Trzeba dodać odwrotną linię, bo nie zawsze startPoint jest 
                    // z tej strony, z której bym chciał
                    Lines.Add(lnRev);
                    LinesDict.AddListItem(lnRev.StartPoint.Id(), lnRev);
                    

                    res++;
                    Ac.SetProgress(res);
                }
                Ac.ClearProgress();
	        }
            return res;
        }

        internal void BuildPolygons(System.Windows.Controls.ProgressBar progressBar)
        {
            using (var trans = Ac.StartTransaction())
                try
                {
                    progressBar.Maximum = Lines.Count;
                    progressBar.Minimum = 0;
                    
                    foreach (var ln in Lines)
                    {
                        Polyline polyline = new Polyline();
                        polyline.AddVertex(ln.StartPoint);
                        if (BuildPolygon(polyline, ln.StartPoint, ln))
                        {
                            polyline.Closed = true;
                            if (polyline.NumberOfVertices > 2)
                                trans.AddEntity(polyline);
                        }
                        progressBar.Value = progressBar.Value + 1;
                        System.Windows.Forms.Application.DoEvents();
                    }
                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    Ac.WriteLn("Geo7: " + ex.Message);
                }
        }
        
        public int BuildPolygons()
        {
            int res = 0;
            using (var trans = Ac.StartTransaction())
            try
            {
                Ac.InitProgress(AppServices.Strings.CreatingPolygons, Lines.Count());
                foreach (var ln in Lines)
                {
                    Polyline polyline = new Polyline();
                    polyline.AddVertex(ln.StartPoint);
                    if (BuildPolygon(polyline, ln.StartPoint, ln))
                    {
                        res++;
                        polyline.Closed = true;
                        if (polyline.NumberOfVertices > 2)
                            trans.AddEntity(polyline);
                    }
                    Ac.ProgressStep();
                }
                Ac.ClearProgress();
                trans.Commit();
            }
            catch (System.Exception ex)
            {
                Ac.WriteLn("Geo7: "+ex.Message);
            }
            return res;
        }

        private List<AcPolygonSegment> GetEndPointLines(AcPolygonSegment currentSegment)
        {
            List<AcPolygonSegment> res = new List<AcPolygonSegment>();
            string endPointId = currentSegment.EndPoint.Id();
            var endPointLines = LinesDict.GetList(endPointId);
            foreach (var ln in endPointLines)
            {
                // Usuń linie, które są takie same jak currentSegment
                if (ln.Id == currentSegment.Id)
                    continue;
                if (ln.Reverse().Id == currentSegment.Id)
                    continue;
                res.Add(ln);
            }
            return res;
        }

        private bool BuildPolygon(Polyline polyline, Point2d startPoint, AcPolygonSegment currentSegment)
        {            
            if (currentSegment.EndPoint.Id() == startPoint.Id())
                return true; 
            // Nie dodawaj ostatniego zdublowanego wertexu, bo potem przy zmianie początku polilini ID się nie zgadzają

            polyline.AddVertex(currentSegment.EndPoint);

            var endPointLines = GetEndPointLines(currentSegment);
            if (endPointLines.Count < 1)
                return false;



            AcPolygonSegment nextLine = GetNextLine(endPointLines, currentSegment);

            return BuildPolygon(polyline, startPoint, nextLine);
        }

        private AcPolygonSegment GetNextLine(List<AcPolygonSegment> lines, AcPolygonSegment currentSegment)
        {
            Angle azimuth0 = currentSegment.Azimuth - new Angle(180, 0, 0);
            // UpdateTempAzimuths
            foreach (var ln in lines)
            {
                ln.TempAzimuth = ln.Azimuth - azimuth0;
            }

            // Get Next Line, TempAzimuth = Max
            AcPolygonSegment nextLine = lines.First();
            foreach (var ln in lines)
            {
                if (ln.TempAzimuth.AsRad > nextLine.TempAzimuth.AsRad)
                    nextLine = ln;
            }
            return nextLine;
        }

    }

    public class AcJoinPolygonsWithTexts
    {

        public void Run()
        {
            using (var trans = Ac.StartTransaction())
            {
                var allTexts = trans.GetAllEntities<DBText>().Where(t => !string.IsNullOrEmpty(t.TextString.Trim()));
                var polygons = trans.GetAllEntities<Polyline>().Where(p => (p.Closed == true) && (p.Length > 0.01));

                Ac.InitProgress(AppServices.Strings.ProcessingData, polygons.Count());
                foreach (var p in polygons)
                {
                    var pTexts = GetTextsInPolygon(p, allTexts);
                    ChangePolygonLayer(trans, p, pTexts);
                    Ac.ProgressStep();
                }
                Ac.ClearProgress();
                trans.Commit();
            }
        }

        private void ChangePolygonLayer(AcTransaction trans, Polyline polygon, List<DBText> texts)
        {
            polygon.UpgradeOpen();
            string layerName = "";
            if (texts.Count < 1)
                layerName = "#PolygonsWithoutTextInside";
            else if (texts.Count == 1)
                layerName = texts[0].TextString;
            else
            {
                var textStrings = from t in texts select t.TextString.Trim();
                string prefix = "#" + textStrings.Count().ToString() + "-";
                layerName = prefix + textStrings.ToList().Join("-");
            }

            layerName = Ac.GetValidName(layerName); 
            polygon.LayerId = trans.CreateLayer(layerName);
        }



        private List<DBText> GetTextsInPolygon(Polyline polygon, IEnumerable<DBText> texts)
        {
            List<DBText> res = new List<DBText>();
            var curves = polygon.GetOffsetCurves(-0.01);
            if (curves.Count < 1)
                return res;
            var polygonOff = curves[0] as Polyline;

            foreach (var t in texts)
            {
                if (polygon.IsInside(t.Position))
                    res.Add(t);
            }
            return res;
        }

    }

    
}
