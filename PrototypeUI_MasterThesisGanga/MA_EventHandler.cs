using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrototypeUI_MasterThesisGanga
{
    
    public class MA_EventHandler : IExternalEventHandler
    {
        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>> PositionHistory { get; set; } 
        public static double Coord_x { get; set; } 
        public static double Coord_y { get; set; }
        public static double Coord_z { get; set; }
        public static XYZ PrevLocation { get; set; }
        public static double Voidheight { get; set; }
        public static double VoidLength { get; set; }
        public static double VoidWidth { get; set; }
        public static int VoidVersionNumber { get; set; }
        //public static string ElementIDvalue { get; set; }
       

        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;

            Transaction trans = new Transaction(doc);
            trans.Start("GSS");

            List<string> Versionnos = PositionHistory.Keys.ToList();
            foreach (string item in Versionnos)
            {
                int VersionNumber = Convert.ToInt32(item);
                Dictionary<string, string> VoidGeometry = new Dictionary<string, string>();
                Dictionary<string, string> VoidPosition = new Dictionary<string, string>();

                VoidGeometry = PositionHistory[item]["Geometry"];
                VoidPosition = PositionHistory[item]["Position"];

                MA_EventHandler.VoidVersionNumber = VersionNumber;
                MA_EventHandler.Coord_x = Convert.ToDouble(VoidPosition["PosX"]);
                MA_EventHandler.Coord_y = Convert.ToDouble(VoidPosition["PosY"]);
                MA_EventHandler.Coord_z = Convert.ToDouble(VoidPosition["PosZ"]);

                MA_EventHandler.Voidheight = Convert.ToDouble(VoidGeometry["Height"]);
                MA_EventHandler.VoidLength = Convert.ToDouble(VoidGeometry["Depth"]);
                MA_EventHandler.VoidWidth = Convert.ToDouble(VoidGeometry["Width"]);

                //string fsFamilyName = "VoidPenetrationFamily";
                //string fsName = "WallPenetrationType";
                //Void History_Visualization_element Family

                string fsFamilyName = "HistoryVoidElements_Cuboid_GS";
                string fsName = "HistoryVoidElements_Cuboid_GS";


                FamilySymbol familySymbol = (from fs in new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                                             where (fs.Family.Name == fsFamilyName && fs.Name == fsName)
                                             select fs).First();

                double tofeetconverter = 0.3048; //1 feet =0,3048m

                double x = Coord_x / tofeetconverter;
                double y = Coord_y / tofeetconverter;
                double z = Coord_z / tofeetconverter;

                double VoidHt = Voidheight / tofeetconverter; //in feet
                double VoidLn = VoidLength / tofeetconverter;
                double VoidWt = VoidWidth / tofeetconverter;
                int VoidVrNo = VoidVersionNumber;

                XYZ locationVoid = new XYZ(x, y, z);
                PrevLocation = locationVoid;

                //MessageBox.Show("Running 1");
                //MessageBox.Show(Convert.ToString(x)+ ";" + Convert.ToString(y) + ";" + Convert.ToString(z));

                FamilyInstance instanceNew = doc.Create.NewFamilyInstance(locationVoid, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                //FamilyInstance instanceNew = doc.Create.NewFamilyInstance(locationVoid, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                //FamilyInstance instanceNew1 = doc.Create.NewFamilyInstance(locationVoid1, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                //MessageBox.Show("Running 2");
                instanceNew.LookupParameter("Height Input").Set(VoidHt); //in feet
                instanceNew.LookupParameter("Length Input").Set(VoidWt); 
                instanceNew.LookupParameter("Width Input").Set(VoidLn);
                //instanceNew.LookupParameter("VoidVersionNumber").Set(VoidVrNo);

                //instanceNew.LookupParameter("HeightVoid").Set(VoidHt); //in feet
                //instanceNew.LookupParameter("LengthVoid").Set(VoidLn);
                //instanceNew.LookupParameter("WidthVoid").Set(VoidWt);
                //instanceNew.LookupParameter("VoidVersionNumber").Set(VoidVrNo);

                string[] multilinelist = { "V " + Convert.ToString(VoidVersionNumber), "" };
                string multiline = String.Join(" \n", multilinelist);
                instanceNew.LookupParameter("VersionNo").Set(multiline);


                //XYZ P1 = new XYZ(doubles_pt1[0], doubles_pt1[1], doubles_pt1[2]);
                //XYZ P2 = new XYZ(doubles_pt2[0], doubles_pt2[1], doubles_pt2[2]);

                XYZ P1 = new XYZ(0,0,0);
                XYZ P2 = new XYZ(100,100,100);

                //List<XYZ> Points = new List<XYZ>();
                //Points.Add(P1);
                //Points.Add(P2);

                //Line line = Line.CreateBound(P1, P2);
                //XYZ dir = line.Direction;
                //double xx = dir.X, yy = dir.Y, zz = dir.Z;
                //XYZ normal = new XYZ(zz - yy, xx - zz, yy - xx);

                //Plane plane = Plane.CreateByNormalAndOrigin(normal, P1);
                //SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                //ModelCurve modelCurve = doc.Create.NewModelCurve(line, sketchPlane);

                //List<ElementId> setOfLines = new List<ElementId>();

                //for (var i = 0; i < Points.Count - 1; i++)
                //{
                //    Line line = Line.CreateBound(Points[i], Points[i + 1]);
                //    //XYZ normal = line.Direction.Normalize();
                //    XYZ dir = line.Direction;
                //    double xx = dir.X, yy = dir.Y, zz = dir.Z;
                //    XYZ normal = new XYZ(zz - yy, xx - zz, yy - xx);

                //    Plane plane = Plane.CreateByNormalAndOrigin(normal, Points[i]);
                //    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                //    ModelCurve modelCurve = doc.Create.NewModelCurve(line, sketchPlane);
                //    setOfLines.Add(modelCurve.Id);
                //}
                //doc.Create.NewGroup(setOfLines);



                //MessageBox.Show("Running 2");

                //MA_EventHandler.ElementIDvalue = "dd";//instanceNew.Id.ToString();
            }

            trans.Commit();
        }
              
        public string GetName()
        {
            return "das";
        }



    }
}
