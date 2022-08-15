using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace PrototypeUI_MasterThesisGanga
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Prototype_Main : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and documnet objects
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = commandData.Application.Application;
            #region
            //string filename = @"C:\Users\gssanila\Desktop\VoidPenetrationFamily.rfa";
            //Family family = null;

            //Transaction trans = new Transaction(doc);
            //trans.Start("GS");
            //string fsFamilyName = "HistoryVoidElements_Cuboid_GS";
            //string fsName = "HistoryVoidElements_Cuboid_GS";

            //FamilySymbol familySymbol = (from fs in new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilySymbol))
            //    .Cast<FamilySymbol>()
            //    where (fs.Family.Name == fsFamilyName && fs.Name == fsName)
            //    select fs).First();


            //double x = -44.3185;
            //double y = 134.4685;
            //double z = -46.3796;

            //XYZ locationVoid = new XYZ(x, y, z);

            //FamilyInstance instanceNew = doc.Create.NewFamilyInstance(locationVoid, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            //doc.LoadFamily(filename, out family);
            //trans.Commit();
            #endregion

            MainForm GUI = new MainForm(uiapp, doc);
            GUI.Show();

            return Result.Succeeded;
        }
    }
}


