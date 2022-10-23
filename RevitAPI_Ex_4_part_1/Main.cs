using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RevitAPI_Ex_4_part_1
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        private Document doc;
        private static string levelDown;
        private static string levelHigh;
        public List<Level> listLevel { get; set; } = new List<Level>();
        public double Width { get; private set; }
        public double Depth { get; private set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            levelDown = "Уровень 1";
            levelHigh = "Уровень 2";
            Width = 10000;
            Depth = 10000;
            listLevel = ListLevels.GetLevels(commandData);
            WallsCreate();

            return Result.Succeeded;
        }

        private void WallsCreate()
        {
            double width = UnitUtils.ConvertToInternalUnits(Width, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(Depth, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;
            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));
            List<Wall> walls = new List<Wall>();
            Transaction ts = new Transaction(doc, "Создание стен");
            ts.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, listLevel[0].Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(listLevel[1].Id);
            }
            ts.Commit();
        }

        public class ListLevels
        {
            public static List<Level> GetLevels(ExternalCommandData commandData)
            {
                var uiapp = commandData.Application;
                var uidoc = uiapp.ActiveUIDocument;
                var doc = uidoc.Document;
                List<Level> listLevel = new FilteredElementCollector(doc)
                 .OfClass(typeof(Level))
                 .OfType<Level>()
                 .Where(x => x.Name.Equals(levelDown) || x.Name.EndsWith(levelHigh))
                 .ToList();
                return listLevel;
            }
        }
    }
}
