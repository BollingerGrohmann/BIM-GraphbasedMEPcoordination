using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MasterThesisTUM21Ganga;

namespace PrototypeUI_MasterThesisGanga
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private UIApplication uiApp;
        private Document DBdoc;
        private UIDocument uiDoc;
        public string uri;
        public string user;

        public string password { get; set; }

        public MainForm(UIApplication uiapp, Document doc)
        {
            this.uiApp = uiapp;
            this.DBdoc = doc;
            this.uiDoc = uiapp.ActiveUIDocument;
            InitializeComponent();
            
            
            uri= "neo4j+s://6984bc34.databases.neo4j.io:7687";
            Properties.Settings.Default.Save();            
            txtUsername.Text= Properties.Settings.Default["user"].ToString();          
            txtPassword.Text= Properties.Settings.Default["password"].ToString();
            user = txtUsername.Text;
            password = txtPassword.Text;


            string FromDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string ToDate = dateTimePicker2.Value.ToString("yyyy-MM-dd");
            //listBoxHistoryLog.Items.Add(password);
            //listBoxHistoryLog.Items.Add(FromDate);
            //listBoxHistoryLog.Items.Add(ToDate);

            string[] row1 = { "ARC", "", "", "" };
            string[] row2 = { "STR", "", "", "" };
            string[] row3 = { "MEP", "", "", "" };

            Table_Approval.Rows.Add(row1);
            Table_Approval.Rows.Add(row2);
            Table_Approval.Rows.Add(row3);
        }
        //string theDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");



        private List<Element> GetSelectedElement()
        {
            //listBoxHistoryLog.Items.Add("check 1");
            // Get selected elements from current document.

            //Autodesk.Revit.UI.Selection.Selection selection = uiDoc.Selection;
            ElementId selectedId = uiDoc.Selection.GetElementIds().First();

            ICollection<ElementId> selectedIds = uiDoc.Selection.GetElementIds();
            List<Element> SelectedVoids = new List<Element>();
            if (selectedIds.Count != 0)
            {
                foreach (ElementId id in selectedIds)
                {
                    Element Voidelement = uiDoc.Document.GetElement(id);
                    SelectedVoids.Add(Voidelement);
                }
            }
            return SelectedVoids;
        }
        public string [] GetCentroidOfSelectedVoidElements(Element GetSelectedElement)
        {    
            Options Opts = new Options();
            Opts.DetailLevel = ViewDetailLevel.Fine;

            GeometryElement geo = GetSelectedElement.get_Geometry(Opts);

            Solid VoidElementSolid = null;
            string[] PtCoords = new string[3]; //Store XYZ position 
            //string VoidCentroid = ""; //Store XYZ position 

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

                //listBoxHistoryLog.Items.Add(centroid.X);
                //listBoxHistoryLog.Items.Add(centroid.Y);
                //listBoxHistoryLog.Items.Add(centroid.Z);
                //VoidCentroid = (centroid.X * 0.3048).ToString() +","+ (centroid.Y * 0.3048).ToString() + "," + (centroid.Z * 0.3048).ToString();

            }
            else
            {
                TaskDialog.Show("Error", "Centroid is not available");
            }
            return PtCoords;
        }
        private List<Dictionary<string, string>> SelectedVoidParametersDict()
        {
            List<Element> GetSelectedVoidElementList = GetSelectedElement();
            List<Dictionary<string, string>> VoidParametersDict = new List<Dictionary<string, string>>();

            foreach (Element El in GetSelectedVoidElementList)
            {
                Dictionary<string, string> TempElParam = new Dictionary<string, string>();

                //// Void Identifier             
                string voidName = El.LookupParameter("Name").AsString();
                string IfcGUID = El.LookupParameter("IfcGUID").AsString();
                string VoidStoreyNo = El.LookupParameter("Storey No").AsString();
                string UpdateDate = El.LookupParameter("Database update date").AsString();

                ////MEP Approval Parameters
                string MEPStatus = El.LookupParameter("MEP Approval Status").AsString();
                string MEPComment = El.LookupParameter("MEP Comment").AsString();
                string MEPApprovalDate = El.LookupParameter("MEP Approval Date").AsString();

                ////ARC Approval parameters
                string ARCStatus = El.LookupParameter("ARC Approval Status").AsValueString();
                string ARCComment = El.LookupParameter("ARC Comment").AsString();
                string ARCApprovalDate = El.LookupParameter("ARC Approval Date").AsString();

                ////STR Approval parameters
                string STRStatus = El.LookupParameter("STR Approval Status").AsValueString();
                string STRComment = El.LookupParameter("STR Comment").AsString();
                string STRApprovalDate = El.LookupParameter("STR Approval Date").AsString();

                ////Dimensions
                string VoidShape = El.LookupParameter("Shape").AsString();

                //Revit reading in feet. But in database values are in meter
                string VoidWidth = Convert.ToString(El.LookupParameter("Width").AsDouble() * 0.3048);
                string VoidHeight = Convert.ToString(El.LookupParameter("Height").AsDouble() * 0.3048); 
                string VoidDiameter = Convert.ToString(El.LookupParameter("Diameter").AsDouble() * 0.3048);
                string VoidDepth = Convert.ToString(El.LookupParameter("Depth").AsDouble() * 0.3048);
                string[] VoidCentroid = GetCentroidOfSelectedVoidElements(El);           

                TempElParam.Add("Void Name", voidName);
                TempElParam.Add("IfcGUID", IfcGUID);
                TempElParam.Add("Storey No", VoidStoreyNo);
                TempElParam.Add("Update Date", UpdateDate);

                TempElParam.Add("MEP Approval Status", MEPStatus);
                TempElParam.Add("MEP Comment", MEPComment);
                TempElParam.Add("MEP Approval Date", MEPApprovalDate);

                TempElParam.Add("ARC Approval Status", ARCStatus);
                TempElParam.Add("ARC Comment", ARCComment);
                TempElParam.Add("ARC Approval Date", ARCApprovalDate);
                TempElParam.Add("STR Approval Status", STRStatus);
                TempElParam.Add("STR Comment", STRComment);
                TempElParam.Add("STR Approval Date", STRApprovalDate);

                TempElParam.Add("Shape", VoidShape);
                TempElParam.Add("Width", VoidWidth);
                TempElParam.Add("Height", VoidHeight);
                TempElParam.Add("Diameter", VoidDiameter);
                TempElParam.Add("Depth", VoidDepth);

                TempElParam.Add("X_Coord", VoidCentroid[0]);
                TempElParam.Add("Y_Coord", VoidCentroid[1]);
                TempElParam.Add("Z_Coord", VoidCentroid[2]);

                #region Check
                //string[] keyList = {"Void Name", "IfcGUID", "Storey No",
                //    "MEP Approval Status", "MEP Comment", "MEP Approval Date",
                //    "ARC Approval Status", "ARC Comment", "ARC Approval Date",
                //    "STR Approval Status", "STR Comment", "STR Approval Date",
                //    "Shape", "Width", "Height", "Diameter", "Depth", "Update Date" };

                //listBoxHistoryLog.Items.Add("VoidWidth: " + VoidWidth);
                //listBoxHistoryLog.Items.Add("VoidHeight: " + VoidHeight);
                //listBoxHistoryLog.Items.Add("VoidDiameter: " + VoidDiameter);
                //listBoxHistoryLog.Items.Add("VoidDepth: " + VoidDepth);
                //listBoxHistoryLog.Items.Add("IfcGUID: " + IfcGUID);
                //listBoxHistoryLog.Items.Add("Name: " + voidName);
                //listBoxHistoryLog.Items.Add("VoidStoreyNo: " + VoidStoreyNo);
                //listBoxHistoryLog.Items.Add("UpdateDate: " + UpdateDate);
                //listBoxHistoryLog.Items.Add("VoidShape: " + VoidShape);
                //listBoxHistoryLog.Items.Add("MEPStatus: " + MEPStatus);
                //listBoxHistoryLog.Items.Add("ARCStatus: " + ARCStatus);
                //listBoxHistoryLog.Items.Add("STRApprovalDate: " + STRApprovalDate);
                #endregion test

                VoidParametersDict.Add(TempElParam);
            }
            return VoidParametersDict;
        }

        /// <summary>
        /// Retrieve all pre-selected element's of the specified type,
        /// if any elements at all have been pre-selected. If not,
        /// retrieve all elements of specified type in the database.
        /// </summary>
        /// <param name="a">Return value container</param>
        /// <param name="uidoc">Active document</param>
        /// <param name="t">Specific type</param>
        /// <returns>True if some elements were retrieved</returns>
        /// 
        public static bool GetSelectedElementsOrAll(List<Element> a,UIDocument uidoc,Type t)
        {
            Document doc = uidoc.Document;
            ICollection<ElementId> ids
              = uidoc.Selection.GetElementIds();

            if (0 < ids.Count)
            {
                a.AddRange(ids.Select<ElementId, Element>(id => doc.GetElement(id)).Where<Element>(e => t.IsInstanceOfType(e)));
            }
            else
            {
                a.AddRange(new FilteredElementCollector(doc).OfClass(t));
            }
            return 0 < a.Count;
        }

        private void buttonUpdateProposal_Click(object sender, EventArgs e)
        {         
            List<Dictionary<string, string>> VoidParametersDict = SelectedVoidParametersDict();
            string ProposalDiscipline = comboBoxProposalDiscipline.SelectedItem.ToString();
            GraphDBNeo4j.ProposeVoidsToDataBase(uri, user, password, VoidParametersDict, ProposalDiscipline);
        }

        private void buttonProvideChoiceOfApproval_Click(object sender, EventArgs e)
        {    
            List<Dictionary<string, string>> VoidParametersDict = SelectedVoidParametersDict();
            string ApprovalDiscipline = comboBoxApprovalDiscipline.SelectedItem.ToString();         
            GraphDBNeo4j.NEWUpdate_VoidsGraphInDB(uri, user, password, VoidParametersDict[0], ApprovalDiscipline);
        }

        //private void buttonApprovalHistory_Click(object sender, EventArgs e)
        //{
        //    string IfcGUID = SelectedVoidParametersDict()[0]["IfcGUID"];
        //    listBoxHistoryLog.Items.Add(IfcGUID);
        //    int iterationNo = Convert.ToInt32(numericUpDownProposalIterationNo.Value);
        //    listBoxHistoryLog.Items.Add(iterationNo.ToString());

        //    Dictionary<string, Dictionary<string, string>> GetApprovalHistory = GraphDBNeo4j.GetApprovalHistory_Selected(uri, user, password, IfcGUID, iterationNo);

        //    GraphDBNeo4j.DialogResultApproval(GetApprovalHistory);
        //    //listBoxHistoryLog.Items.Add(GraphDBNeo4j.DialogResultApproval(GetApprovalHistory));
        //    //

        //}
        private void buttonPositions_Click(object sender, EventArgs e)
        {
            string IfcGUID = SelectedVoidParametersDict()[0]["IfcGUID"];
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> PositionHistory = MasterThesisTUM21Ganga.GraphDBNeo4j.GetPreviousPositions(uri, user, password, IfcGUID);

            string TaskDialogMsg = " Position and geometry history of the selected void" + "\n";

            var qry = from outer in PositionHistory
                      from inner in outer.Value
                      select inner.Key + ": " + inner.Value + "\n";// +  " ("+ outer.Key+ ")" + "/n";
            //var version = from outer in allOffset select outer.Key;
            int count = 0;
            foreach (string s in qry)
            {

                if (count % 4 == 0)
                {
                    TaskDialogMsg = TaskDialogMsg + "\n";
                }
                TaskDialogMsg = TaskDialogMsg + s;
                count++;
            }
            listBoxHistoryLog.Items.Add(TaskDialogMsg);
            //TaskDialog.Show("Message", TaskDialogMsg);
            //TaskDialogResult ShowResultGeometryPositions = MasterThesisTUM21Ganga.GraphDBNeo4j.DialogResultGeometryPositions(PositionHistory);
        }

        private void buttonGetStatus_Click(object sender, EventArgs e)
        {


            string IfcGUID = SelectedVoidParametersDict()[0]["IfcGUID"];
            //listBoxHistoryLog.Items.Add(IfcGUID);

            //int iterationNo = Convert.ToInt32(numericUpDownProposalIterationNo.Value);
            //listBoxHistoryLog.Items.Add(iterationNo.ToString());

            List<string> LastVersions = GraphDBNeo4j.GetExistingVoidVersions(uri, user, password, IfcGUID);
            int LastVersion = Convert.ToInt32(LastVersions.First());
            //listBoxHistoryLog.Items.Add("Approval status of the version "+ LastVersion.ToString());

            //Dictionary<string, Dictionary<string, string>> GetApprovalHistoryDict = GraphDBNeo4j.GetApprovalHistory_Selected(uri, user, password, IfcGUID, iterationNo);
            Dictionary<string, Dictionary<string, string>> GetApprovalHistoryDict = GraphDBNeo4j.GetApprovalHistory_Selected(uri, user, password, IfcGUID, LastVersion);
   
            
            string[] row_ARC = { "ARC", "Not Checked", "", "" };
            string[] row_STR = { "STR", "Not Checked", "", "" };
            string[] row_MEP = { "MEP", " ", "", "" };

            //List< KeyValuePair<string,string>> ApprovalstatusLatest = new List<KeyValuePair<string, string>>();

            //string[] row1new = { "MEP", "Status", "Comment", "Date" };

            foreach (var apprvalNo in GetApprovalHistoryDict)
            {
                if (apprvalNo.Value["ApprovalDiscipline"] == "ARC")
                {
                    row_ARC[1] = apprvalNo.Value["Approval_Status"];
                    row_ARC[2] = apprvalNo.Value["Approval_Comment"];
                    row_ARC[3] = apprvalNo.Value["Approval_Date"];
                }
                if (apprvalNo.Value["ApprovalDiscipline"] == "STR")
                {
                    row_STR[1] = apprvalNo.Value["Approval_Status"];
                    row_STR[2] = apprvalNo.Value["Approval_Comment"];
                    row_STR[3] = apprvalNo.Value["Approval_Date"];
                }
                if (apprvalNo.Value["ApprovalDiscipline"] == "MEP")
                {
                    row_MEP[1] = apprvalNo.Value["Approval_Status"];
                    row_MEP[2] = apprvalNo.Value["Approval_Comment"];
                    row_MEP[3] = apprvalNo.Value["Approval_Date"];
                }
                else if (apprvalNo.Value["ApprovalDiscipline"] != "MEP")
                {
                    row_MEP[1] = "";
                    //row_MEP[2] = apprvalNo.Value["Approval_Comment"];
                    //row_MEP[3] = apprvalNo.Value["Approval_Date"];
                }
            

            }

            //Dic,status,comment,date            
            Table_Approval.Rows[0].SetValues(row_ARC);
            Table_Approval.Rows[1].SetValues(row_STR);
            Table_Approval.Rows[2].SetValues(row_MEP);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string IfcGUID = SelectedVoidParametersDict()[0]["IfcGUID"];
            //listBoxHistoryLog.Items.Add(IfcGUID);

            //List<string> LastVersions = GraphDBNeo4j.GetExistingVoidVersions(uri, user, password, IfcGUID);
            //int LastVersion = Convert.ToInt32(LastVersions.First());
            //listBoxHistoryLog.Items.Add(LastVersion.ToString());

            string FromDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string ToDate = dateTimePicker2.Value.ToString("yyyy-MM-dd");
            //listBoxHistoryLog.Items.Add(FromDate);
            //listBoxHistoryLog.Items.Add(ToDate);

            List<string> ResultVersions = GraphDBNeo4j.GetVoidVersionsUsingDate(uri, user, password, IfcGUID, FromDate, ToDate);

            for(int i = 0; i< ResultVersions.Count; i++)
            {
                listBoxHistoryLog.Items.Add(ResultVersions[i]);
            }

        }

        private void buttonApprovalHistory_Click(object sender, EventArgs e)
        {


            string IfcGUID = SelectedVoidParametersDict()[0]["IfcGUID"];        

            string FromDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string ToDate = dateTimePicker2.Value.ToString("yyyy-MM-dd");


            Dictionary<List<string>, Dictionary<string, Dictionary<string, string>>> AllHistory = GraphDBNeo4j.GetAllHistoryWithDate(uri, user, password, IfcGUID, FromDate, ToDate);

            //listBoxHistoryLog.Items.Clear();
            listBoxHistoryLog.Items.Add("");
            listBoxHistoryLog.Items.Add( "APPROVAL HISTORY");
        
            //listBoxHistoryLog.Items.Add("\n");

            foreach (var item in AllHistory)
            {
                List<string> temp = item.Key;
                listBoxHistoryLog.Items.Add("");
                listBoxHistoryLog.Items.Add("---------------> Iteration " + temp[0] + " " + temp[1]); //temp(0) = iteration loop number 0,1,2                
                //listBoxHistoryLog.Items.Add(temp[1]); // temp(1) = status From: &fromdate  To: &todate
             
                Dictionary<string, Dictionary<string, string>> approvals = item.Value;

                var qry = from outer in approvals
                          from inner in outer.Value
                          select inner.Key + ": " + inner.Value + "\n";
               
                if (approvals.Values.Count !=0)
                {
                    int count = 0;
                    foreach (var approvalInfo in approvals.Values)
                    {                     
                        //listBoxHistoryLog.Items.Add("- Choice of approval " + (count + 1).ToString());

                        string tempstring1 = "* " + approvalInfo["Approval_Status"] + " by " + approvalInfo["ApprovalDiscipline"] + " on " + approvalInfo["Approval_Date"];
                        listBoxHistoryLog.Items.Add(tempstring1);

                        
                        if (string.IsNullOrEmpty(approvalInfo["Approval_Comment"])== false) //if (approvalInfo["Approval_Comment"] != "")
                        {
                            string tempstring2 = " " + " with comment : " + approvalInfo["Approval_Comment"];
                            listBoxHistoryLog.Items.Add(tempstring2);
                        }
                        listBoxHistoryLog.Items.Add(" ");

                    }
                }
                else
                {
                    listBoxHistoryLog.Items.Add("!! Specialist planners hasn't updated choice of approval");
                }

              
            }

          
        }

        private void buttonHistoryvisualization_Click(object sender, EventArgs e)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> PositionHistory = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            string IfcGUID = ""; 
            try
            {
                IfcGUID = SelectedVoidParametersDict()[0]["IfcGUID"];
                PositionHistory = GraphDBNeo4j.GetPreviousPositions(uri, user, password, IfcGUID);
                MA_EventHandler.PositionHistory = PositionHistory;
                ExternalEvents.MyExEv.Raise();
            }
            catch
            {

            }

            string FromDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string ToDate = dateTimePicker2.Value.ToString("yyyy-MM-dd");
            Dictionary<List<string>, Dictionary<string, Dictionary<string, string>>> AllHistory = GraphDBNeo4j.GetAllHistoryWithDate(uri, user, password, IfcGUID, FromDate, ToDate);

            List<string> version_Period = new List<string>();
            foreach (var item in AllHistory)
            {
                List<string> temp = item.Key;               
                version_Period.Add(temp[1]);
            }


            List<string> Versionnos = PositionHistory.Keys.ToList();
            try
            {

                listBoxHistoryLog.Items.Add("");
                //listBoxHistoryLog.Items.Clear();
                listBoxHistoryLog.Items.Add("CHANGE HISTORY");
                listBoxHistoryLog.Items.Add("");
                int n = 0;
                foreach (string item in Versionnos)
                {

                    int VersionNumber = Convert.ToInt32(item);
                    listBoxHistoryLog.Items.Add("---------------> Iteration " + VersionNumber + " " + version_Period[n]); 
                    
                    Dictionary<string, string> VoidGeometry = new Dictionary<string, string>();
                    Dictionary<string, string> VoidPosition = new Dictionary<string, string>();
                    VoidGeometry = PositionHistory[item]["Geometry"];
                    VoidPosition = PositionHistory[item]["Position"];

                    listBoxHistoryLog.Items.Add(VoidGeometry["Shape"]);
                    listBoxHistoryLog.Items.Add("Dimention : ( " + VoidGeometry["Width"] + " ; " + VoidGeometry["Height"] + " ; " + VoidGeometry["Depth"] + " )");
                    //listBoxHistoryLog.Items.Add("     &     ");
                    listBoxHistoryLog.Items.Add("Position X,Y,Z : (" + VoidPosition["PosX"] + "; " + VoidPosition["PosY"] + " ; " + VoidPosition["PosZ"] + " )");
                    listBoxHistoryLog.Items.Add(" ");


                    foreach (var itemx in PositionHistory.Values)
                    {
                        //VoidGeometry = itemx["Geometry"];
                        //VoidPosition = itemx["Position"];
                        //listBoxHistoryLog.Items.Add( VoidGeometry["Shape"]);
                        //listBoxHistoryLog.Items.Add(    "Dimention : ( "+ VoidGeometry["Width"] + " ; " + VoidGeometry["Height"] + " ; " + VoidGeometry["Depth"] + " )");
                        ////listBoxHistoryLog.Items.Add("     &     ");
                        //listBoxHistoryLog.Items.Add(    "Position X,Y,Z : (" + VoidPosition["PosX"] + "; "+ VoidPosition["PosY"] + " ; "+ VoidPosition["PosZ"] + " )");
                        //listBoxHistoryLog.Items.Add(" ");

                        #region test MA_EventHandler
                        //double Height;
                        //double Depth; //lenght
                        //double Diameter;
                        //double Width;
                        //double PosX;
                        //double PosY;
                        //double PosZ;
                        //listBoxHistoryLog.Items.Add("VoidGeometry keys");
                        //listBoxHistoryLog.Items.AddRange(VoidGeometry.Keys.ToArray());
                        //listBoxHistoryLog.Items.Add("..   ..");
                        //listBoxHistoryLog.Items.Add("VoidPosition keys");             
                        //listBoxHistoryLog.Items.AddRange(VoidPosition.Keys.ToArray());
                        //MA_EventHandler.VoidVersionNumber = VersionNumber;
                        //MA_EventHandler.Coord_x = Convert.ToDouble(VoidPosition["PosX"]);
                        //MA_EventHandler.Coord_y = Convert.ToDouble(VoidPosition["PosY"]);
                        //MA_EventHandler.Coord_z = Convert.ToDouble(VoidPosition["PosZ"]);
                        //MA_EventHandler.Voidheight = Convert.ToDouble(VoidGeometry["Height"]);
                        //MA_EventHandler.VoidLength = Convert.ToDouble(VoidGeometry["Depth"]);
                        //MA_EventHandler.VoidWidth = Convert.ToDouble(VoidGeometry["Width"]);
                        //ExternalEvents.MyExEv.Raise();
                        //listBoxHistoryLog.Items.Add("SHOWING HISTORY" + "\n");
                        ////listBoxHistoryLog.Items.Add("elementid" + MA_EventHandler.ElementIDvalue.ToString());
                        //listBoxHistoryLog.Items.Add("element Pos X" + MA_EventHandler.Coord_x.ToString());
                        //listBoxHistoryLog.Items.Add("element Pos Y" + MA_EventHandler.Coord_y.ToString());
                        //listBoxHistoryLog.Items.Add("element Pos Z" + MA_EventHandler.Coord_z.ToString());
                        #endregion test
                    }
                    n++;
                }
            }
            catch (Exception ex )
            {
                listBoxHistoryLog.Items.Add(ex);
            }

        }

        #region TEST
        private void button1_Click(object sender, EventArgs e)
        {
            //MA_EventHandler.VoidVersionNumber = 1;
            MA_EventHandler.Coord_x =1.0;
            MA_EventHandler.Coord_y = 10;
            MA_EventHandler.Coord_z = 0.0;

            MA_EventHandler.Voidheight = 1;
            MA_EventHandler.VoidLength = 2;
            MA_EventHandler.VoidWidth = 5;

            ExternalEvents.MyExEv.Raise();

            listBoxHistoryLog.Items.Add("-----------New Family Instance created--------------------");
            //listBoxHistoryLog.Items.Add(MA_EventHandler.ElementIDvalue);

        }

        private void buttoncheck_Click(object sender, EventArgs e)
        {
        List<Element> GetSelectedVoidElementList = GetSelectedElement();
            foreach (Element El in GetSelectedVoidElementList)
            {
                Dictionary<string, string> TempElParam = new Dictionary<string, string>();

                //// Void Identifier             
                string voidName = El.LookupParameter("Name").AsString();
                string IfcGUID = El.LookupParameter("IfcGUID").AsString();
                string VoidWidth = El.LookupParameter("Width").AsValueString().ToString();
                string VoidHeight = Convert.ToString(El.LookupParameter("Height").AsDouble()* 0.3048);
                

                string VoidDiameter = El.LookupParameter("Diameter").AsValueString().ToString();
                string VoidDepth = El.LookupParameter("Depth").AsValueString().ToString();

                listBoxHistoryLog.Items.Add("IfcGUID: " + IfcGUID);
                listBoxHistoryLog.Items.Add("VoidWidth: " + VoidWidth);
                listBoxHistoryLog.Items.Add("VoidHeight: " + VoidHeight);
                listBoxHistoryLog.Items.Add("Height: " + Height.ToString());
                listBoxHistoryLog.Items.Add("VoidDiameter: " + VoidDiameter);
                listBoxHistoryLog.Items.Add("VoidDepth: " + VoidDepth);
            } 
            
        }
        #endregion

        private void Table_Approval_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
