using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j;
using Neo4j.Driver;
using Autodesk.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;
using RevitServices.Elements;
using DynamoElement = Revit.Elements.Element; //Dynamo wraper 
using Revit.Elements;
using Element = Autodesk.Revit.DB.Element;
namespace MasterThesisTUM21Ganga
{
   

    public class GetVoidObjectData
    {

        #region Decleration of Revit 

        public static UIApplication uiApp = DocumentManager.Instance.CurrentUIApplication;
        public static Document DBdoc = DocumentManager.Instance.CurrentDBDocument;
        public static UIDocument uiDoc = DocumentManager.Instance.CurrentUIDocument;

        #endregion Decleration of Revit 

        GetVoidObjectData()
        {

        }
        // ======================================================================================================
        public static List<DynamoElement> GetSelectedVoidElements()
        {
            List<DynamoElement> VoidElements = new List<DynamoElement>();
            // Get the handle of current revit document.
            UIDocument uidoc = uiApp.ActiveUIDocument;

            // Get selected elements from current document.
            ICollection<ElementId> SelectedVoidIDs = uiDoc.Selection.GetElementIds();

            if (SelectedVoidIDs.Count == 0)
            {
                // If no elements selected.
              TaskDialog.Show( "Element Selection", "You haven't selected any Void elements", TaskDialogCommonButtons.Ok);                
            }
            else
            {
                foreach (ElementId VoidElementId in SelectedVoidIDs)
                {
                    //Autodesk.Revit.Element element = uidoc.Document.GetElement(VoidElementId);
                    DynamoElement element = (uidoc.Document.GetElement(VoidElementId)).ToDSType(true); //Convert to dynamo element =ToDSType(true)
                    {
                       VoidElements.Add(element);
                     
                    }
                }
            }
            return VoidElements;
        }
        private static List<ElementId> GetSelectedVoidsID()
        {
            /// Select some elements in Revit before invoking this command
            // Get the handle of current revit document.
            UIDocument uidoc = uiApp.ActiveUIDocument;

            // Get the element selection of current document.
            Selection selection = uidoc.Selection;

            // Get selected elements from current document.
            ICollection<ElementId> SelectedVoidIDs = uiDoc.Selection.GetElementIds();
            List<ElementId> ids = new List<ElementId>();
            if (SelectedVoidIDs.Count == 0)
            {
                // If no elements selected.
                //TaskDialog.Show("Revit", "You haven't selected any elements.");
            }
            else
            {
                foreach (ElementId VoidElementId in SelectedVoidIDs)
                {
                    ids.Add(VoidElementId);
                }
            }
            return ids;
        }
        // ======================================================================================================
        public static string[] GetCentroidOfSelectedVoidElements()
        {
            ICollection<ElementId> SelectedVoid_IDs = uiDoc.Selection.GetElementIds();
            Element VoidElement = DBdoc.GetElement(SelectedVoid_IDs.First());

            Options Opts = new Options();
            Opts.DetailLevel = ViewDetailLevel.Fine;

            GeometryElement geo = VoidElement.get_Geometry(Opts);

            Solid VoidElementSolid = null;
            string[] PtCoords = new string[3]; //Store XYZ position 

            foreach (GeometryObject geomObj in geo)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    VoidElementSolid = solid;
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            VoidElementSolid = solid;
                        }
                    }
                }
            }
            if (VoidElementSolid != null)
            {
                XYZ centroid = VoidElementSolid.ComputeCentroid();
                //Internally, Revit always uses imperial feet for all length related units:But I want to store values in metric system 
                //1 feet = 0.3048 meter 
                PtCoords[0] = (centroid.X * 0.3048).ToString();
                PtCoords[1] = (centroid.Y * 0.3048).ToString();
                PtCoords[2] = (centroid.Z * 0.3048).ToString();
               
            }
            else
            {
                TaskDialog.Show("Error", "Centroid is not available");
            }
            return PtCoords;
        }
        // ======================================================================================================

        //public static List<List<string>> GetSelectedVoidElementsParameters()
        //{
        //    ICollection<ElementId> SelectedVoid_IDs = uiDoc.Selection.GetElementIds();
        //    List<Element> VoidElements = new List<Element>();
        //    foreach(ElementId id in SelectedVoid_IDs)
        //    {
        //        VoidElements.Add(DBdoc.GetElement(id));
        //    }
        //    Options Opts = new Options();
        //    Opts.DetailLevel = ViewDetailLevel.Fine;

        //    foreach (Element Elm in VoidElements)
        //    {
        //        GeometryElement geo = Elm.get_Geometry(Opts);

        //        Solid VoidElementSolid = null;
        //        string[] PtCoords = new string[3]; //Store XYZ position 

        //        foreach (GeometryObject geomObj in geo)
        //        {
        //            if (geomObj is Solid)
        //            {
        //                Solid solid = (Solid)geomObj;
        //                VoidElementSolid = solid;
        //            }
        //            else if (geomObj is GeometryInstance)
        //            {
        //                GeometryInstance geomInst = (GeometryInstance)geomObj;
        //                GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
        //                foreach (GeometryObject instGeomObj in instGeomElem)
        //                {
        //                    if (instGeomObj is Solid)
        //                    {
        //                        Solid solid = (Solid)instGeomObj;
        //                        VoidElementSolid = solid;
        //                    }
        //                }
        //            }
        //        }
        //        if (VoidElementSolid != null)
        //        {
        //            XYZ centroid = VoidElementSolid.ComputeCentroid();
        //            //Internally, Revit always uses imperial feet for all length related units:But I want to store values in metric system 
        //            //1 feet = 0.3048 meter 
        //            PtCoords[0] = (centroid.X * 0.3048).ToString();
        //            PtCoords[1] = (centroid.Y * 0.3048).ToString();
        //            PtCoords[2] = (centroid.Z * 0.3048).ToString();

        //        }
        //        else
        //        {
        //            TaskDialog.Show("Error", "Centroid is not available");
        //        }
        //    }

        //    return PtCoords;
        //}
        // ======================================================================================================

        public static List<Element> GetSelectedVoidElementList()
        {
            List<Element> VoidElements = new List<Element>();
            // Get the handle of current revit document.
            UIDocument uidoc = uiApp.ActiveUIDocument;

            // Get selected elements from current document.
            ICollection<ElementId> SelectedVoidIDs = uiDoc.Selection.GetElementIds();

            if (SelectedVoidIDs.Count == 0)
            {
                // If no elements selected.
                TaskDialog.Show("Element Selection", "You haven't selected any Void elements", TaskDialogCommonButtons.Ok);
            }
            else
            {
                foreach (ElementId VoidElementId in SelectedVoidIDs)
                {
                    Element element = uidoc.Document.GetElement(VoidElementId);                  
                    VoidElements.Add(element);                   
                }
            }
            return VoidElements;
        }
    }
}
