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
    public class AcRemoveDuplicates
    {
        Dictionary<string, List<Entity>> IdsDict = new Dictionary<string, List<Entity>>();
        Dictionary<Entity, List<string>> EntitiesDict = new Dictionary<Entity, List<string>>();
        
        private void BuildEntitiesDict(IEnumerable<Entity> entities)
        {
            foreach (var ent in entities)
            {
                var ids = GetEntityIds(ent);
                foreach (var id in ids)
                {
                    IdsDict.AddListItem(id, ent);
                    EntitiesDict.AddListItem(ent, id);
                }
            }
        }

        protected virtual IEnumerable<string> GetEntityIds(Entity entity)
        {
            var res = new List<string>();
            res.Add(entity.Id.ToString());
            return res;
        }

        public void Run(IEnumerable<Entity> entities, bool showProgress)
        {
            BuildEntitiesDict(entities);

            if (showProgress)
                Ac.InitProgress(AppServices.Strings.DeletingDuplicates, entities.Count());

            foreach (var kv in EntitiesDict)
            {
                var ids = kv.Value;
                var ent = kv.Key;

                if (ent.IsErased)
                    continue;

                foreach (var id in ids)
                {

                    var duplEnts = IdsDict.GetList(id);
                    foreach (var duplEnt in duplEnts)
                    {
                        if (duplEnt != ent)
                        {
                            duplEnt.UpgradeOpen();
                            duplEnt.Erase();
                        }
                    }
                }
                if (showProgress)
                    Ac.ProgressStep();
            }
            if (showProgress)
                Ac.ClearProgress();
        }

    }

    public class AcRemoveDuplicatedPolygons : AcRemoveDuplicates
    {
        public static void Run()
        {
            using (var trans = Ac.StartTransaction())
            {
                var polylines = trans.GetAllEntities<Polyline>();
                var polygons = polylines.Where(p => p.Closed == true).ToList();
                var polygonsByLength = polygons.ToLookup(p => p.Length.ToString(Ac.LinearPrecisionFormat));

                Ac.InitProgress(AppServices.Strings.DeletingDuplicates, polygonsByLength.Count);
                Ac.WriteLn(AppServices.Strings.DeletingDuplicates);

                foreach (var polygonsWithLength in polygonsByLength)
                {
                    var ents = polygonsWithLength.ConvertTo<Entity>();
                    AcRemoveDuplicatedPolygons rdp = new AcRemoveDuplicatedPolygons();
                    rdp.Run(ents, false);
                    Ac.ProgressStep();
                }
                Ac.ClearProgress();
                trans.Commit();
            }
        }
        protected override IEnumerable<string>  GetEntityIds(Entity entity)
        {
            List<string> res = new List<string>();
            var poly = entity as Polyline;
            if (poly == null)
                return res;

            if (!poly.Closed)
                return res;

            var points = poly.GetPoints2d();
            var pointsRev = points.ToList();
            pointsRev.Reverse();

            res.AddRange(GetPointsIds(points));
            res.AddRange(GetPointsIds(pointsRev));
            return res;
        }

        private List<string> GetPointsIds(List<Point2d> points)
        {
            List<string> pointsIds = new List<string>();

            for (int i = 0; i < points.Count; i++)
            {
                string id = GetPointsId(points);
                pointsIds.Add(id);
                var point0 = points[0];
                points.RemoveAt(0);
                points.Add(point0);
            }
            return pointsIds;
        }    

        private string GetPointsId(List<Point2d> points)
        {
            List<string> pointIds = new List<string>();
            foreach (var p in points)
	        {
                pointIds.Add(p.Id());
	        }
            return pointIds.Join(", ");
        }        
    }



}
