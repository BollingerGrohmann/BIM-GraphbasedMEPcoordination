using System;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace MasterThesisTUM21Ganga
{

    //public class VoidApprovalinformation
    //{
    //    int IterationNo;
    //    int ApprovalVersionNo;
    //    string IfcGUID;
    //    string Name;
    //    string ApprovalDiscipline;
    //    string Approval_Status;
    //    string Approval_Comment;
    //    string Approval_Date;
    //}
    public class GraphDBNeo4j : IDisposable
    {
        GraphDBNeo4j() { }

        #region ConnectNeo4jDriver

        private bool _disposed = false;
        private readonly IDriver _driver;

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _driver?.Dispose();
            }

            _disposed = true;
        }
        private GraphDBNeo4j(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }
        public void Dispose()
        {
            _driver?.Dispose();
        }

        #endregion ConnectNeo4jDriver

        #region useful Neo4j Queries 

        //MATCH(n)
        //WHERE id(n) = 112
        //DETACH DELETE n

        //MATCH(n) DETACH DELETE n


        //MATCH(:Void { IfcGUID: "1cKSLO3vP5_vnq4YR$u$sJ" })-[:Has_State {TO: ''}]-(Vr1:Version)-[*]-(all_connected:ARC)-[*]-(GetARC)
        //MATCH((Vr1:Version)-[*]-(all_connected:MEP)-[*]-(GetMEP))
        //RETURN GetARC, GetMEP

        //MATCH(:Void { IfcGUID: "1cKSLO3vP5_vnq4YR$u$sJ" })-[:Has_State {TO: ''}]-(Vr1: Version)
        //MATCH p = (Vr1: Version) -[*] - (all_connected) -[*] - (GetARC)
        //WHERE all_connected.name = 'STR' OR all_connected.name = 'ARC' OR all_connected.name = 'MEP' OR all_connected.name = 'Geometry'
        //RETURN GetARC

        // MATCH(p:Void)
        //WHere exists(p.IfcGUID)
        //Return p.IfcGUID as GUIDs
        //.......--------------------------
        //get versions of void which has lie on u2 storey

        //Match(str:Storey{ name: "U2"})-[]-(pos:Position)-[]-(voidVr:Version) return voidVr
        //Match(str:Storey{ name: "U2"})-[]-(pos:Position)-[]-(voidVr:Version)-[]-(voids:Void) return voids


        //Get structure approval data of given void
        //MATCH(:Void { IfcGUID: "09uCMKO7P6DQ8dOqHFtBbG" })-[:Has_State { TO: "01.10.2021"}]-(Vr1: Version) -[] - (VrApp: Approval_Version) -[] - (Appdis: ApprovalDiscipline{ name: "STR"})-[] - (all)
        //RETURN(all) as STRApprovalInformation

        //MATCH(:Void { IfcGUID: "1h2057DXYgP7c8E6kna5rUxt" })-[:Has_State { TO: ''}]-(Vr1: Version) -[] - (VrApp: Approval_Version) -[] -(Apprdisc:ApprovalDiscipline) -[]-(all) RETURN(all.name) as Approvaldata, Apprdisc.name as Discipline, VrApp.VersionNo as Approval_VrNo ,Vr1.VersionNo as IterationNo

    #endregion usefulQueries

        //]]]]][[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[

            #region Decleration of Revit 

        public static UIApplication uiApp = DocumentManager.Instance.CurrentUIApplication;
                public static Document DBdoc = DocumentManager.Instance.CurrentDBDocument;
                public static UIDocument uiDoc = DocumentManager.Instance.CurrentUIDocument;

                #endregion Decleration of Revit 


        //]]]]][[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[

        #region Write Data to Database
        private void CreateGraphsForApprovalVoids_fn(Dictionary<string, string> ParamDict, string ProposalDiscipline, int ProposalVersion)
        {
            string void_name = ParamDict["Void Name"];
            string IfcGUID = ParamDict["IfcGUID"];
            int VersionNo = ProposalVersion;
            
            string ProposalDiscipline_Status=""; 
            string ProposalDiscipline_Comment="";
            string ProposalDiscipline_Approval_Date="";
            if(ProposalDiscipline == "MEP")
            {
                ProposalDiscipline_Status = ParamDict["MEP Approval Status"].ToString();
                ProposalDiscipline_Comment = ParamDict["MEP Comment"];
                ProposalDiscipline_Approval_Date = ParamDict["MEP Approval Date"].ToString();
            }
            else if (ProposalDiscipline == "ARC")
            {
                ProposalDiscipline_Status = ParamDict["ARC Approval Status"].ToString();
                ProposalDiscipline_Comment = ParamDict["ARC Comment"];
                ProposalDiscipline_Approval_Date = ParamDict["ARC Approval Date"].ToString();           
            }

            #region STR &ARC
            //string ARC = "ARC";
            //string ARC_Approval_Status = VoidParametersDict["ARC Approval Status"];
            //string ARC_Comment = VoidParametersDict["ARC Comment"];
            //string ARC_Approval_Date = VoidParametersDict["ARC Approval Date"];

            //string STR = "STR";
            //string STR_Approval_Status = VoidParametersDict["STR Approval Status"];
            //string STR_Comment = VoidParametersDict["STR Comment"];
            //string STR_Approval_Date = VoidParametersDict["STR Approval Date"];
            #endregion STR &ARC

            //Geometry parameters of the selected Voids
            string Dimensions = "Geometry";
            string VoidShape = ParamDict["Shape"].ToString();
            string VoidWidth = ParamDict["Width"].ToString();
            string VoidHeight = ParamDict["Height"].ToString();
            string VoidDiameter = ParamDict["Diameter"].ToString();
            string VoidDepth = ParamDict["Depth"].ToString();

            //Position Parameters of the selected voids

            string Position = "Position";
            //string[] VoidCentroid = GetVoidObjectData.GetCentroidOfSelectedVoidElements(); //Get Geometric centoid of the SelectedVoids
            //string X_coord = VoidCentroid[0];
            //string Y_coord = VoidCentroid[1];
            //string Z_coord = VoidCentroid[2];

            string X_coord = ParamDict["X_Coord"];
            string Y_coord = ParamDict["Y_Coord"];
            string Z_coord = ParamDict["Z_Coord"];

            string StoreyName = ParamDict["Storey No"];


            string Date = ParamDict["Update Date"]; 
           
            if (Date==null || Date==""|| Date.Length == 0)
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd");
            }

            string void_VersionNo_Name = "V_" + VersionNo.ToString();

            using (var session = _driver.Session())
            {
                var CreateGraphData_result = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (v0:Void { name: $void_name, IfcGUID: $IfcGUID }) " +
                                        "CREATE (vn:Version { name: $void_VersionNo_Name, VersionNo: $VersionNo }) " +
                                        "CREATE (v0)-[state_n:Has_State { FROM: date($Date), TO: ''}]->(vn) " +
                                   
                                       //Create Proposal parameter nodes and relationships
                                        "CREATE (mep:Proposal_Discipline{ name: $ProposalDiscipline }) " + //create Node MEP
                                        "CREATE (vn)-[:Proposed_By]->(mep) " +       // create relationship
                                        "CREATE (sn1:Proposal_Status { name: $ProposalDiscipline_Status }) " +
                                        "CREATE (mep)-[:Status]->(sn1) " +
                                        "CREATE (cn1:Proposal_Comment { name: $ProposalDiscipline_Comment }) " +
                                        "CREATE (mep)-[:Comment]->(cn1) " +
                                        "CREATE (apn1:Proposal_Date { name: $ProposalDiscipline_Approval_Date }) " +
                                        "CREATE (mep)-[:Date]->(apn1) " +

                                                                                //-----------------------------------------------------------------------
                                        "CREATE (Pos:Position { name: $Position }) " +
                                        "CREATE (vn)-[:Has]->(Pos) " +

                                        "CREATE (srn:Storey { name: $StoreyName }) " +
                                        "CREATE (Pos)-[:Storey_No]->(srn) " +
                                        "CREATE (xcod:PosX { name: $X_coord}) " +
                                        "CREATE (Pos)-[:X_Coodinate]->(xcod) " +
                                        "CREATE (ycod:PosY { name: $Y_coord }) " +
                                        "CREATE (Pos)-[:Y_Coodinate]->(ycod) " +
                                        "CREATE (zcod:PosZ { name: $Z_coord }) " +
                                        "CREATE (Pos)-[:Z_Coodinate]->(zcod) " +

                                        //Geometric parameters - create node and assign relationship
                                        "CREATE (dim:Geometry { name: $Dimensions }) " + //dim:type (class) { name: nodename =geometry }
                                        "CREATE (vn)-[:Has]->(dim) " +

                                        "CREATE (shp:Shape { name: $VoidShape }) " +
                                        "CREATE (dim)-[:Shape]->(shp) " +

                                        "CREATE (wth:Width { name: $VoidWidth}) " +
                                        "CREATE (dim)-[:Width]->(wth) " +

                                        "CREATE (ht:Height { name: $VoidHeight }) " +
                                        "CREATE (dim)-[:Height]->(ht) " +

                                        "CREATE (dia:Diameter { name: $VoidDiameter }) " +
                                        "CREATE (dim)-[:Diameter]->(dia) " +

                                        "CREATE (dpt:Depth { name: $VoidDepth }) " +
                                        "CREATE (dim)-[:Depth]->(dpt) " +
                                        //-----------------------------------------------------------------------
                                        "RETURN * ", //Return all elements 
                                                     //-----------------------------------------------------------------------
                        new
                        {
                            void_name,
                            IfcGUID,
                            void_VersionNo_Name,
                            VersionNo,
                            Date,
                            //MEP,
                            //MEP_Approval_Status,
                            //MEP_Comment,
                            //MEP_Approval_Date,
                            ProposalDiscipline,
                            ProposalDiscipline_Status,
                            ProposalDiscipline_Comment,
                            ProposalDiscipline_Approval_Date,
                            Position,
                            StoreyName,
                            X_coord,
                            Y_coord,
                            Z_coord,
                            Dimensions,
                            VoidShape,
                            VoidWidth,
                            VoidHeight,
                            VoidDiameter,
                            VoidDepth
                        });

                    return "Finished";
                });
            }            
        }

        public static TaskDialogResult Create_VoidsGraphInDB(string uri, string user, string password, List<Dictionary<string, string>> VoidParametersDict, string ProposalDiscipline)
        {
           
            string TaskDialogMsg = "";
            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                List<string> IfcGUID_List = DBConnection.IfcGUID_ListinDB_fn();

                for (int i = 0; i < VoidParametersDict.Count; i++)
                {
                    string IfcGUID = (VoidParametersDict[i])["IfcGUID"];
                    string ProposalVersion_temp= DBConnection.LastVoidProposalVersionsInDB(IfcGUID).First();
                    int ProposalVersion = Convert.ToInt32(ProposalVersion_temp);

                     bool ExistCheckIfcGUID = IfcGUID_List.Contains(IfcGUID);
                    if (ExistCheckIfcGUID == false) //create only if it is not existing 
                    {
                        DBConnection.CreateGraphsForApprovalVoids_fn(VoidParametersDict[i], ProposalDiscipline, ProposalVersion);
                        TaskDialogMsg = "Created proposal version V" + ProposalVersion.ToString() + " of the selected Voids in DB";
                    }
                   else
                   {
                       TaskDialogMsg = "The selected Void's proposal version V" + ProposalVersion.ToString()+ " is already existing in DB"; //(ifcGUID:" + IfcGUID + ")
                   }

                }
            }
            return TaskDialog.Show("Message ", TaskDialogMsg);
        }     
        private List<string> IfcGUID_ListinDB_fn()
        {
            List<string> IfcGUID_List = new List<string>();

            using (var session = _driver.Session())
            {
                List<string> temp_IfcGUID_List = new List<string>();

                var result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH (p:Void) " + 
                                             "WHERE exists(p.IfcGUID) " +
                                             "RETURN p.IfcGUID as IfcGUIDs ",
                        new { });
                    foreach (var IfcGUID_x in Queryresult)
                    {
                        temp_IfcGUID_List.Add(IfcGUID_x["IfcGUIDs"].As<string>());
                    }
                    return temp_IfcGUID_List;
                });
                IfcGUID_List = result;
                
            }
            return IfcGUID_List;
        }    

        private List<string> LastVoidProposalVersionsInDB(string IfcGUID)
        {
            List<string> VersionNumbers = new List<string>();
            using (var session = _driver.Session())
            {
                List<string> Versions_temp = new List<string>();

                var result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID })-[:Has_State {TO: ''}]-(Vr1: Version)" +
                     "RETURN Vr1.VersionNo as VersionNumbers", 
                     new { IfcGUID });

                    foreach (var versions_x in Queryresult)
                    {
                        Versions_temp.Add(versions_x["VersionNumbers"].As<string>());
                    }
                    return Versions_temp;
                });
                VersionNumbers = result;                
            }
            return VersionNumbers;
        }

        public static List<string> GetExistingVoidVersions(string uri, string user, string password, string IfcGUID)
        {
            List<string> VersionNo = new List<string>();
            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                VersionNo = DBConnection.LastVoidProposalVersionsInDB(IfcGUID);
            }
            return VersionNo;
        }

        //]]]]][[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[

        private string Retrieve_VoidLastVersionNoInDB(string IfcGUID)
        {
            string LastVersionNumber;
            using (var session = _driver.Session())
            {

                var result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH (:Void {IfcGUID: $IfcGUID })-[:Has_State {TO: ''}]-(Vr:Version) " +
                                        "RETURN Vr.VersionNo AS VersionNumber",
                        new { IfcGUID });
                    return Queryresult.Single()[0].As<string>();
                });
                LastVersionNumber = result;
            }
            return LastVersionNumber;
        }

        //]]]]][[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[

        private Dictionary<string, string> RetrieveVoidVersionGraphData_fn(string IFC_Guid)
        {
            Dictionary<string, string> pp = new Dictionary<string, string>();

            using (var session = _driver.Session())
            {
                List<string> temp_key = new List<string>();
                List<string> temp_value = new List<string>();
                Dictionary<string, string> temp_dict;

                //string VersionNo = "V" + Version_Number.ToString();

                var RetrieveHistory_result = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (:Void {IFCGuid: $IFC_Guid})-[*]-(voidX:Void)-[*]-(:Property {name: $VersionNo}) " +
                                                "MATCH(:Void { IFCGuid: $IFC_Guid})-[*] - (voidX) -[*] - (allconnected)" +
                                                "RETURN allconnected",
                        new { IFC_Guid });
                    foreach (var r in result)
                    {
                        //Get as an INode instance to access properties.
                        var node = r["allconnected"].As<INode>();
                        //Properties are a Dictionary<string,object>, so you need to 'As' them
                        var name = node["name"].As<string>();
                        var label = node.Labels.Single().As<string>();
                        temp_key.Add(label);
                        temp_value.Add(name);
                    }
                    temp_dict = temp_key.Zip(temp_value, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
                    return temp_dict;
                });
                pp = RetrieveHistory_result;
            }
            return pp;
        }
        //------------------------------------------------------------------------------------------------------------------------------
        public static Dictionary<string, string> RetrieveVoidVersionGraphData(string uri, string user, string password, string IFC_Guid, int Version_Number)
        {
            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                return DBConnection.RetrieveVoidVersionGraphData_fn(IFC_Guid);
            }
        }

        //]]]]][[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[

        private List<int> Retrieve_IterationNoList(string IfcGUID)
        {
            List<int> ExistingVoidIterNosList = new List<int>();

            using (var session = _driver.Session())
            {
                List<int> temp_List = new List<int>();

                var Retrieve_IterationNos_result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH (:Void {IfcGUID: $IfcGUID })- [*] - (VoidIterations:Version) " +
                                                "RETURN VoidIterations.VersionNo as IterationNos ",
                        new { IfcGUID });
                    
                    foreach (var IterationNos_x in Queryresult)
                    {
                        temp_List.Add(IterationNos_x["IterationNos"].As<int>());
                    }
                    return temp_List;
                });
                ExistingVoidIterNosList = Retrieve_IterationNos_result;
            }
            return ExistingVoidIterNosList;
        }
        private void CreateIterationGraphsForApprovalVoids_fn(Dictionary<string, string> ParamDict, string ProposalDiscipline, int ProposalVersion)
        {
            string void_name = ParamDict["Void Name"];
            string IfcGUID = ParamDict["IfcGUID"];
            int VersionNo = ProposalVersion;

            //ProposalDiscipline = "MEP";           
            string ProposalDiscipline_Status = "";
            string ProposalDiscipline_Comment = "";
            string ProposalDiscipline_Approval_Date = "";
            if (ProposalDiscipline == "MEP")
            {
                ProposalDiscipline_Status = ParamDict["MEP Approval Status"].ToString();
                ProposalDiscipline_Comment = ParamDict["MEP Comment"];
                ProposalDiscipline_Approval_Date = ParamDict["MEP Approval Date"].ToString();
            }
            else if (ProposalDiscipline == "ARC")
            {
                ProposalDiscipline_Status = ParamDict["ARC Approval Status"].ToString();
                ProposalDiscipline_Comment = ParamDict["ARC Comment"];
                ProposalDiscipline_Approval_Date = ParamDict["ARC Approval Date"].ToString();
            }

            #region STR &ARC
            //string ARC = "ARC";
            //string ARC_Approval_Status = VoidParametersDict["ARC Approval Status"];
            //string ARC_Comment = VoidParametersDict["ARC Comment"];
            //string ARC_Approval_Date = VoidParametersDict["ARC Approval Date"];

            //string STR = "STR";
            //string STR_Approval_Status = VoidParametersDict["STR Approval Status"];
            //string STR_Comment = VoidParametersDict["STR Comment"];
            //string STR_Approval_Date = VoidParametersDict["STR Approval Date"];
            #endregion STR &ARC

            //Geometry parameters of the selected Voids
            string Dimensions = "Geometry";
            string VoidShape = ParamDict["Shape"].ToString();
            string VoidWidth = ParamDict["Width"].ToString();
            string VoidHeight = ParamDict["Height"].ToString();
            string VoidDiameter = ParamDict["Diameter"].ToString();
            string VoidDepth = ParamDict["Depth"].ToString();

            //Position Parameters of the selected voids

            string Position = "Position";
            //string[] VoidCentroid = GetVoidObjectData.GetCentroidOfSelectedVoidElements(); //Get Geometric centoid of the SelectedVoids
            //string X_coord = VoidCentroid[0];
            //string Y_coord = VoidCentroid[1];
            //string Z_coord = VoidCentroid[2];

            string X_coord = ParamDict["X_Coord"];
            string Y_coord = ParamDict["Y_Coord"];
            string Z_coord = ParamDict["Z_Coord"];
            string StoreyName = ParamDict["Storey No"];

            string new_Date = ParamDict["Update Date"];
            if (new_Date == null || new_Date == "" || new_Date.Length == 0)
            {
                new_Date = DateTime.Now.ToString("yyyy-MM-dd");

            }

            string void_VersionNo_Name = "V_" + VersionNo.ToString();

            using (var session = _driver.Session())
            {
                var CreateGraphData_result = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MERGE (v0:Void {IfcGUID: $IfcGUID })-[state_old:Has_State {TO: ''}] - (VrLatest:Version) " +
                                        "CREATE (vn:Version { name: $void_VersionNo_Name, VersionNo: $VersionNo }) " +
                                        "CREATE (v0)-[state_n:Has_State { FROM: date($new_Date), TO: ''}]->(vn) " +
                                        "SET state_old.TO = date($new_Date) " +
                                        "SET state_n.FROM = date($new_Date) " +
                                       //-----------------------------------------------------------------------
                                       //Create Proposal parameter nodes and relationships
                                       "CREATE (mep:Proposal_Discipline{ name: $ProposalDiscipline }) " + //create Node MEP
                                       "CREATE (vn)-[:Proposed_By]->(mep) " +       // create relationship
                                        "CREATE (sn1:Proposal_Status { name: $ProposalDiscipline_Status }) " +
                                        "CREATE (mep)-[:Status]->(sn1) " +
                                        "CREATE (cn1:Proposal_Comment { name: $ProposalDiscipline_Comment }) " +
                                        "CREATE (mep)-[:Proposal_Comment]->(cn1) " +
                                        "CREATE (apn1:Proposal_Date { name: $ProposalDiscipline_Approval_Date }) " +
                                        "CREATE (mep)-[:Proposal_Date]->(apn1) " +
                                          //-----------------------------------------------------------------------
                                        "CREATE (Pos:Position { name: $Position }) " +
                                        "CREATE (vn)-[:Has]->(Pos) " +
                                        "CREATE (srn:Storey { name: $StoreyName }) " +
                                        "CREATE (Pos)-[:Storey_No]->(srn) " +
                                        "CREATE (xcod:PosX { name: $X_coord}) " +
                                        "CREATE (Pos)-[:X_Coodinate]->(xcod) " +
                                        "CREATE (ycod:PosY { name: $Y_coord }) " +
                                        "CREATE (Pos)-[:Y_Coodinate]->(ycod) " +
                                        "CREATE (zcod:PosZ { name: $Z_coord }) " +
                                        "CREATE (Pos)-[:Z_Coodinate]->(zcod) " +

                                        //Geometric parameters - create node and assign relationship
                                        "CREATE (dim:Geometry { name: $Dimensions }) " + //dim:type (class) { name: nodename =geometry }
                                        "CREATE (vn)-[:Has]->(dim) " +

                                        "CREATE (shp:Shape { name: $VoidShape }) " +
                                        "CREATE (dim)-[:Shape]->(shp) " +

                                        "CREATE (wth:Width { name: $VoidWidth}) " +
                                        "CREATE (dim)-[:Width]->(wth) " +

                                        "CREATE (ht:Height { name: $VoidHeight }) " +
                                        "CREATE (dim)-[:Height]->(ht) " +

                                        "CREATE (dia:Diameter { name: $VoidDiameter }) " +
                                        "CREATE (dim)-[:Diameter]->(dia) " +

                                        "CREATE (dpt:Depth { name: $VoidDepth }) " +
                                        "CREATE (dim)-[:Depth]->(dpt) " +
                                        //-----------------------------------------------------------------------
                                        "RETURN * ", //Return all elements 
                                                     //-----------------------------------------------------------------------
                        new
                        {
                            IfcGUID,
                            void_VersionNo_Name,
                            VersionNo,
                            new_Date,
                            ProposalDiscipline,
                            ProposalDiscipline_Status,
                            ProposalDiscipline_Comment,
                            ProposalDiscipline_Approval_Date,
                            Position,
                            StoreyName,
                            X_coord,
                            Y_coord,
                            Z_coord,
                            Dimensions,
                            VoidShape,
                            VoidWidth,
                            VoidHeight,
                            VoidDiameter,
                            VoidDepth
                        });

                    return "Finished";
                });
            }
        }

        public static TaskDialogResult ProposeVoidsToDataBase(string uri, string user, string password, List<Dictionary<string, string>> VoidParametersDict, string ProposalDiscipline)
        {
           
            string TaskDialogMsg = "";
            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                int ProposalVersion;
                List<string> IfcGUID_List = DBConnection.IfcGUID_ListinDB_fn();

                for (int i = 0; i < VoidParametersDict.Count; i++)
                {
                    string IfcGUID = (VoidParametersDict[i])["IfcGUID"];
                    bool ExistCheckIfcGUID = IfcGUID_List.Contains(IfcGUID);
                    if(ExistCheckIfcGUID==true)
                    {
                        string LastVersion = DBConnection.LastVoidProposalVersionsInDB(IfcGUID).First();
                        ProposalVersion = Convert.ToInt32(LastVersion) + 1;
                    }
                    else
                    {
                        ProposalVersion = 0;
                    }

                    List<int> IterationNoList = (DBConnection.Retrieve_IterationNoList(IfcGUID));

                    
                    if (ExistCheckIfcGUID == false) //create only if it is not existing 
                    {
                        DBConnection.CreateGraphsForApprovalVoids_fn(VoidParametersDict[i], ProposalDiscipline, 0);
                        TaskDialogMsg = "Created proposal version V" + ProposalVersion.ToString() + " of the selected Voids in DB";
                    }
                    else if(ExistCheckIfcGUID == true && (!IterationNoList.Contains(ProposalVersion))) //Update only if it's proposal version 0 is existing and
                                                                                                       //current proposal version is not existing in DB
                    {
                        DBConnection.CreateIterationGraphsForApprovalVoids_fn(VoidParametersDict[i], ProposalDiscipline, ProposalVersion);
                        TaskDialogMsg = "Updated with the proposal version V" + ProposalVersion.ToString() + " in  the DB for the selected Voids";
                    }
                    else
                    {
                        TaskDialogMsg = "The selected Void's proposal version V" + ProposalVersion.ToString() + " is already existing in DB"; //(ifcGUID:" + IfcGUID + ")
                    }
                }
            }
            return TaskDialog.Show("Message ", TaskDialogMsg);
        }




        private List<string> NEWIfcGUID_ListinDB_fn()
        {
            List<string> IfcGUID_List = new List<string>();

            using (var session = _driver.Session())
            {
                List<string> temp_IfcGUID_List = new List<string>();

                var result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH (p:Void) " +
                                             "WHERE exists(p.IfcGUID) " +
                                             "RETURN p.IfcGUID as IfcGUIDs ",
                        new { });
                    foreach (var IfcGUID_x in Queryresult)
                    {
                        temp_IfcGUID_List.Add(IfcGUID_x["IfcGUIDs"].As<string>());
                    }
                    return temp_IfcGUID_List;
                });
                IfcGUID_List = result;
            }
            return IfcGUID_List;
        }
        private void UpdateGraphData_Void_fn(Dictionary<string, string> ParamDict, string LatestVersionNumber)
        {
            string void_name = ParamDict["Void Name"];
            string IfcGUID = ParamDict["IfcGUID"];
            int VersionNo = 1 + Convert.ToInt32(LatestVersionNumber);

            string MEP = "MEP";
            string STR = "STR";
            string ARC = "ARC";
            string MEP_Approval_Status = ParamDict["MEP Approval Status"];
            string MEP_Comment = ParamDict["MEP Comment"];
            string MEP_Approval_Date = ParamDict["MEP Approval Date"];

            string ARC_Approval_Status = ParamDict["ARC Approval Status"];
            string ARC_Comment = ParamDict["ARC Comment"];
            string ARC_Approval_Date = ParamDict["ARC Approval Date"];

            string STR_Approval_Status = ParamDict["STR Approval Status"];
            string STR_Comment = ParamDict["STR Comment"];
            string STR_Approval_Date = ParamDict["STR Approval Date"];

            string VoidShape = ParamDict["Shape"];
            string VoidWidth = ParamDict["Width"];
            string VoidHeight = ParamDict["Height"];
            string VoidDiameter = ParamDict["Diameter"];
            string VoidDepth = ParamDict["Depth"];

            //Position Parameters of the selected voids
            string Position = "Position";
            string[] VoidCentroid = GetVoidObjectData.GetCentroidOfSelectedVoidElements(); //Get Geometric centoid of the SelectedVoids
            string X_coord = VoidCentroid[0];
            string Y_coord = VoidCentroid[1];
            string Z_coord = VoidCentroid[2];
            string StoreyName = ParamDict["Storey No"];


            //string old_Date = "21.08.2021";
            string new_Date = ParamDict["Update Date"];
            if (new_Date == null || new_Date == "" || new_Date.Length == 0)
            {
                new_Date = DateTime.Now.ToString("dd.MM.yyyy");

            }
            string void_VersionNo_Name = "V" + VersionNo.ToString();
            //string void_VersionNo_Name = "V" + LatestApprovalVersionNumber;

            string Dimensions = "Geometry";

            using (var session = _driver.Session())
            {
                var UpdateGraphData_result = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MERGE (v0:Void {IfcGUID : $IfcGUID})-[state_old:Has_State {TO: ''}]-(v_old:Version) " +
                                        "CREATE (vn:Version { name: $void_VersionNo_Name, VersionNo: $VersionNo }) " +
                                         "CREATE (v0)-[state_n:Has_State { FROM: '', TO: ''}]->(vn) " +
                                         "SET state_old.TO = $new_Date " +
                                         "SET state_n.FROM = $new_Date " +
                                       //-----------------------------------------------------------------------
                                       //Create Approval parameter nodes and relationships for MEP 
                                       "CREATE (mep:MEP { name: $MEP }) " + //create Node MEP
                                       "CREATE (vn)-[:Approval_discipline]->(mep) " +       // create relationship

                                        "CREATE (sn1:MEP_Status { name: $MEP_Approval_Status }) " +
                                        "CREATE (mep)-[:MEP_Status]->(sn1) " +
                                        "CREATE (cn1:MEP_Comment { name: $MEP_Comment }) " +
                                        "CREATE (mep)-[:MEP_Comment]->(cn1) " +
                                        "CREATE (apn1:MEP_Approval_Date { name: $MEP_Approval_Date }) " +
                                        "CREATE (mep)-[:MEP_Approval_Date]->(apn1) " +
                                        //-----------------------------------------------------------------------
                                        //Create Approval parameter nodes and relationships for Architecture 
                                        "CREATE (arc:ARC { name: $ARC }) " +        //create Node MEP
                                        "CREATE (vn)-[:Approval_discipline]->(arc) " +    // create relationship

                                        "CREATE (sn2:ARC_Status { name: $ARC_Approval_Status }) " +
                                        "CREATE (arc)-[:ARC_Status]->(sn2) " +

                                        "CREATE (cn2:ARC_Comment { name: $ARC_Comment }) " +
                                        "CREATE (arc)-[:ARC_Comment]->(cn2) " +

                                        "CREATE (apn2:ARC_Approval_Date { name: $ARC_Approval_Date }) " +
                                        "CREATE (arc)-[:ARC_Approval_Date]->(apn2) " +

                                        //-----------------------------------------------------------------------
                                        //Create Approval parameter nodes and relationships  Structure
                                        "CREATE (str:STR { name: $STR }) " + //create Node MEP
                                        "CREATE (vn)-[:Approval_discipline]->(str) " +  // create relationship

                                        "CREATE (sn3:STR_Status { name: $STR_Approval_Status }) " +
                                        "CREATE (str)-[:STR_Status]->(sn3) " +

                                        "CREATE (cn3:STR_Comment { name: $STR_Comment }) " +
                                        "CREATE (str)-[:STR_Comment]->(cn3) " +

                                        "CREATE (apn3:STR_Approval_Date { name: $STR_Approval_Date }) " +
                                        "CREATE (str)-[:STR_Approval_Date]->(apn3) " +
                                        //-----------------------------------------------------------------------                                      
                                        //Create Position Parameters nodes and relationships  Structure
                                        "CREATE (Pos:Position { name: $Position }) " +
                                        "CREATE (vn)-[:Has]->(Pos) " +
                                        "CREATE (srn:Storey { name: $StoreyName }) " +
                                        "CREATE (Pos)-[:Storey_No]->(srn) " +
                                        "CREATE (xcod:PosX { name: $X_coord}) " +
                                        "CREATE (Pos)-[:X_Coodinate]->(xcod) " +
                                        "CREATE (ycod:Posy { name: $Y_coord }) " +
                                        "CREATE (Pos)-[:Y_Coodinate]->(ycod) " +
                                        "CREATE (zcod:Posz { name: $Z_coord }) " +
                                        "CREATE (Pos)-[:Z_Coodinate]->(zcod) " +

                                        //-----------------------------------------------------------------------
                                        //Geometric parameters - create node and assign relationship
                                        "CREATE (dim:Geometry { name: $Dimensions }) " + //dim:type (class) { name: nodename =geometry }
                                        "CREATE (vn)-[:Has]->(dim) " +

                                        "CREATE (shp:Shape { name: $VoidShape }) " +
                                        "CREATE (dim)-[:Shape]->(shp) " +

                                        "CREATE (wth:Width { name: $VoidWidth}) " +
                                        "CREATE (dim)-[:Width]->(wth) " +

                                        "CREATE (ht:Height { name: $VoidHeight }) " +
                                        "CREATE (dim)-[:Height]->(ht) " +

                                        "CREATE (dia:Diameter { name: $VoidDiameter }) " +
                                        "CREATE (dim)-[:Diameter]->(dia) " +

                                        "CREATE (dpt:Depth { name: $VoidDepth }) " +
                                        "CREATE (dim)-[:Depth]->(dpt) " +
                                        //-----------------------------------------------------------------------
                                        "RETURN * ", //Return all elements 
                                                     //-----------------------------------------------------------------------
                        new
                        {
                            IfcGUID,
                            void_VersionNo_Name,
                            VersionNo,
                            new_Date,
                            MEP,
                            MEP_Approval_Status,
                            MEP_Comment,
                            MEP_Approval_Date,
                            ARC,
                            ARC_Approval_Status,
                            ARC_Comment,
                            ARC_Approval_Date,
                            STR,
                            STR_Approval_Status,
                            STR_Comment,
                            STR_Approval_Date,
                            Position,
                            StoreyName,
                            X_coord,
                            Y_coord,
                            Z_coord,
                            Dimensions,
                            VoidShape,
                            VoidWidth,
                            VoidHeight,
                            VoidDiameter,
                            VoidDepth
                        });

                    return "Finished";
                });
            }
        }
        public static TaskDialogResult Update_VoidsGraphInDB(string uri, string user, string password, Dictionary<string, string> VoidParametersDict, string ApprovalDiscipline, int ApprovalVersion)
        {
            string IfcGUID = VoidParametersDict["IfcGUID"];
            string LatestVersionNo = "";
            string TaskDialogMsg;
            List<string> IfcGUID_List = new List<string>();

            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                IfcGUID_List = DBConnection.IfcGUID_ListinDB_fn();
                bool ExistCheckIfcGUID = IfcGUID_List.Contains(IfcGUID);
                if (ExistCheckIfcGUID == true)
                {
                    LatestVersionNo = DBConnection.Retrieve_VoidLastVersionNoInDB(IfcGUID);
                    DBConnection.UpdateGraphData_Void_fn(VoidParametersDict, LatestVersionNo);

                    int versionUpdated = Convert.ToInt32(LatestVersionNo) + 1;
                    TaskDialogMsg = "Updated Approval version V" + versionUpdated.ToString() + " of the selected Void in DB";
                }
                else
                {
                    TaskDialogMsg = "Cannot find the selected Void in Database" + "\n" + "Please add Version 0 in DB before Updating with Approval Information";
                }

            }
            return TaskDialog.Show("Message", TaskDialogMsg);
        }

        //----------------------------------------------------------------              

        private List<int> Retrieve_VoidApprovalVersionsInDB(string IfcGUID)
        {
            List<int> VersionNumbers = new List<int>();
            using (var session = _driver.Session())
            {
                List<int> Versions_temp = new List<int>();

                var result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID })-[:Has_State {TO: ''}]-(Vr1: Version)-[]-(VrApp:Approval_Version)" +
                     "RETURN VrApp.VersionNo as VersionNumbers",
                     new { IfcGUID });

                    foreach (var versions_x in Queryresult)
                    {
                        Versions_temp.Add(versions_x["VersionNumbers"].As<int>());
                    }
                    return Versions_temp;
                });
                VersionNumbers = result;
            }
            return VersionNumbers;
        }

        private void NEWUpdateGraphData_Void_fn(Dictionary<string, string> ParamDict, string ProposalVersion, string LatestAppVersionNo, string ApprovalDiscipline)
        {
            string void_name = ParamDict["Void Name"];
            string IfcGUID = ParamDict["IfcGUID"];
            
            int VersionNo = 1 + Convert.ToInt32(LatestAppVersionNo);

            string ApprovalDiscipline_Status = "";
            string ApprovalDiscipline_Comment = "";
            string ApprovalDiscipline_Approval_Date = "";

            if (ApprovalDiscipline == "MEP")
            {
                ApprovalDiscipline_Status = ParamDict["MEP Approval Status"];
                ApprovalDiscipline_Comment = ParamDict["MEP Comment"];
                ApprovalDiscipline_Approval_Date = ParamDict["MEP Approval Date"].ToString();
            }
            else if (ApprovalDiscipline == "ARC")
            {
                ApprovalDiscipline_Status = ParamDict["ARC Approval Status"];
                ApprovalDiscipline_Comment = ParamDict["ARC Comment"];
                ApprovalDiscipline_Approval_Date = ParamDict["ARC Approval Date"];               
            }
            else if (ApprovalDiscipline == "STR")
            {
                ApprovalDiscipline_Status = ParamDict["STR Approval Status"];
                ApprovalDiscipline_Comment = ParamDict["STR Comment"];
                ApprovalDiscipline_Approval_Date = ParamDict["STR Approval Date"].ToString();
            }

            if (ApprovalDiscipline_Status == "Yes")
            
            {
                ApprovalDiscipline_Status = "Approved";
            }
            if (ApprovalDiscipline_Status == "No")
            {
                ApprovalDiscipline_Status = "Disapproved";
            }

            string new_Date = ParamDict["Update Date"];
            if (new_Date == null || new_Date == "" || new_Date.Length == 0)
            {
                //new_Date = DateTime.Now.ToString("dd.MM.yyyy");
                new_Date = DateTime.Now.ToString("yyyy-MM-dd");
            }


            string void_VersionNo_Name = "V_" + ProposalVersion + "." + VersionNo.ToString();

            using (var session = _driver.Session())
            {
                var UpdateGraphData_result = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MERGE (v0:Void {IfcGUID : $IfcGUID})-[state_old:Has_State {TO: ''}]-(v_old:Version) " +
                                        "CREATE (vn:Approval_Version { name: $void_VersionNo_Name, VersionNo: $VersionNo }) " +                                       
                                       //-----------------------------------------------------------------------
                                       //Create Approval parameter nodes and relationships for MEP 
                                       "CREATE (v_disc:ApprovalDiscipline  { name: $ApprovalDiscipline }) " + //create Node MEP
                                       "CREATE (vn)-[:Choice_of_approval_from]->(v_disc) " +       // create relationship

                                       "CREATE (v_old)-[state_approval:Has_Approval_state { FROM: ''}]->(vn) " +
                                       "SET state_approval.FROM = date($new_Date) " +

                                        "CREATE (sts:Approval_Status{ name: $ApprovalDiscipline_Status }) " +
                                        "CREATE (vn)-[:Has_Status]->(sts) " +
                                        "CREATE (cmt:Approval_Comment { name: $ApprovalDiscipline_Comment }) " +
                                        "CREATE (vn)-[:Has_Comment]->(cmt) " +
                                        "CREATE (dte:Approval_Date { name: $ApprovalDiscipline_Approval_Date }) " +
                                        "CREATE (vn)-[:Approval_Date]->(dte) " +
                                        
                                        "RETURN * ", //Return all elements 
                                       //-----------------------------------------------------------------------
                                        new
                                        {   
                                            IfcGUID,
                                            void_VersionNo_Name,
                                            VersionNo,
                                            ApprovalDiscipline,
                                            new_Date,                            
                                            ApprovalDiscipline_Status ,
                                            ApprovalDiscipline_Comment ,
                                            ApprovalDiscipline_Approval_Date,                                                
                                        });

                    return "Finished";
                });
            }
        }

        public static TaskDialogResult NEWUpdate_VoidsGraphInDB(string uri, string user, string password, Dictionary<string, string> VoidParametersDict, string ApprovalDiscipline)
        {
            string IfcGUID = VoidParametersDict["IfcGUID"];
            string LatestAppVersionNo = "";
            string TaskDialogMsg;
            List<string> IfcGUID_List = new List<string>();
            List<int> ApprovalversionNos = new List<int>();        

            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                IfcGUID_List = DBConnection.IfcGUID_ListinDB_fn();
                bool ExistCheckIfcGUID = IfcGUID_List.Contains(IfcGUID);
                if (ExistCheckIfcGUID == true)
                {
                    string ProposalVersion = DBConnection.Retrieve_VoidLastVersionNoInDB(IfcGUID);                                  
                    ApprovalversionNos = DBConnection.Retrieve_VoidApprovalVersionsInDB(IfcGUID);

                    //List<string> LastVersions = GraphDBNeo4j.GetExistingVoidVersions(uri, user, password, IfcGUID);
                    //int LastVersion = Convert.ToInt32(LastVersions.First());

                    if (ApprovalversionNos.Count==0) //if nobody gives approval 
                    {                     
                        LatestAppVersionNo = 0.ToString(); 
                    }
                    else  //else if(ApprovalversionNos.Count> 0)
                    {                                         
                        LatestAppVersionNo = ApprovalversionNos.Max().ToString();
                    }
                    
                    DBConnection.NEWUpdateGraphData_Void_fn(VoidParametersDict, ProposalVersion, LatestAppVersionNo, ApprovalDiscipline);

                    int versionUpdated = Convert.ToInt32(LatestAppVersionNo) + 1;
                    TaskDialogMsg = "Updated Approval version V" + versionUpdated.ToString() + " of the selected Void in DB";
                }
                else
                {
                    TaskDialogMsg = "Cannot find the selected Void in Database" + "\n" + "Please add Version 0 in DB before Updating with Approval Information";
                }

            }
            return TaskDialog.Show("Message", TaskDialogMsg);
        }
        
        #endregion Write Data to Database





        #region Read data from database 


        //private Dictionary<string, string> ReadVoidProposalVersionsData_fn(string IFC_Guid, int VoidProposalVersionNo)
        //{
        //    Dictionary<string, string> pp = new Dictionary<string, string>();

        //    using (var session = _driver.Session())
        //    {
        //        List<string> temp_key = new List<string>();
        //        List<string> temp_value = new List<string>();
        //        Dictionary<string, string> temp_dict;

        //        string VersionNo = "V" + VoidProposalVersionNo.ToString();

        //        var RetrieveHistory_result = session.WriteTransaction(tx =>
        //        {
        //            var result = tx.Run("MATCH (:Void {IFCGuid: $IFC_Guid})-[*]-(voidX:Void)-[*]-(:Property {name: $VersionNo}) " +
        //                                        "MATCH(:Void { IFCGuid: $IFC_Guid})-[*] - (voidX) -[*] - (allconnected)" +
        //                                        "RETURN allconnected",
        //                new { IFC_Guid, VersionNo });
        //            foreach (var r in result)
        //            {
        //                //Get as an INode instance to access properties.
        //                var node = r["allconnected"].As<INode>();
        //                //Properties are a Dictionary<string,object>, so you need to 'As' them
        //                var name = node["name"].As<string>();
        //                var label = node.Labels.Single().As<string>();
        //                temp_key.Add(label);
        //                temp_value.Add(name);
        //            }
        //            temp_dict = temp_key.Zip(temp_value, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        //            return temp_dict;
        //        });
        //        pp = RetrieveHistory_result;
        //    }
        //    return pp;
        //}
   

        //public static Dictionary<string, string> ReadVoidVersionGraphData(string uri, string user, string password, string IFC_Guid, int VoidProposalVersionNo)
        //{
        //    using (var DBConnection = new GraphDBNeo4j(uri, user, password))
        //    {
        //        return DBConnection.ReadVoidProposalVersionsData_fn(IFC_Guid, VoidProposalVersionNo);
        //    }
        //}





        //Get latest state of approval from different disciplines 

        private Dictionary<string, string> ReadVoidApprovalVersionData_fn(string IfcGUID,string Discipline)
        {
            Dictionary<string, string> pp = new Dictionary<string, string>();

            using (var session1 = _driver.Session())
            {
                List<string> temp_key = new List<string>();
                List<string> temp_value = new List<string>();
                Dictionary<string, string> temp_dict;

                var result = session1.WriteTransaction(tx =>
                {   /*
                    var Queryresult = tx.Run("MATCH (:Void {IfcGUID: $IfcGUID })-[:Has_State {TO: ''}]-(Vr1:Version)-[*]-(all_connected)-[*]-(GetAll) " +
                                        "WHERE all_connected.name = 'STR' OR all_connected.name = 'ARC' OR all_connected.name = 'MEP' OR all_connected.name = 'Geometry' " +
                                        "RETURN GetAll",

                //  var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID })-[:Has_State {TO: ''}]-(Vr1: Version)-[]-(VrApp:Approval_Version)" +
                //"RETURN VrApp.VersionNo as VersionNumbers",
                //new { IfcGUID });
                
                    MATCH(:Void { IfcGUID: "1h2057DXYgP7c8E6kna5rUxt" })-[:Has_State { TO: ''}]-(Vr1: Version) -[] - (VrApp: Approval_Version) -[] -(Apprdisc:ApprovalDiscipline) -[]-(all) RETURN (all.name) as Approvaldata, Apprdisc.name as Discipline, VrApp.VersionNo as Approval_VrNo ,Vr1.VersionNo as IterationNo
                */
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: '09uCMKO7P6DQ8dOqHFtBbG' })-[:Has_State { TO:''}]-(Vr1: Version) -[] - (VrApp: Approval_Version) -[] - (Appdis: ApprovalDiscipline{ name:$Discipline })-[] - (all)" +
                    "RETURN(all) as STRApprovalInformation",
                    new { IfcGUID });


                    foreach (var r in Queryresult)
                    {
                        //Get as an INode instance to access properties.
                        var node = r["STRApprovalInformation"].As<INode>();
                        //Properties are a Dictionary<string,object>, so you need to 'As' them
                        var name = node["name"].As<string>();
                        var label = node.Labels.Single().As<string>();
                        temp_key.Add(label);
                        temp_value.Add(name);
                    }
                    temp_dict = temp_key.Zip(temp_value, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
                    return temp_dict;
                });
                pp = result;

                //-------------------------------------------------------------------------
            }
            return pp;
        }                
      

        //public static Dictionary<List<string>, List<int>>GetAllVoid_Versions(string uri, string user, string password)
        //public static Dictionary<string, List<string>> GetAllVoid_Versions(string uri, string user, string password,string IfcGUID,int IterationNo)
        //{
        //    using (var DBConnection = new GraphDBNeo4j(uri, user, password))
        //    {
        //        return DBConnection.NEWReadVoidApprovalVersionData_fn(IfcGUID, IterationNo);
        //    }
        //}        
        //https://stackoverflow.com/questions/51978728/how-to-pivot-a-list-in-c-sharp

        private Dictionary<List<string>, List<int>> VoidIterationNumbers()
        {
            Dictionary<List<string>, List<int>> pp = new Dictionary<List<string>, List<int>>();

            using (var session1 = _driver.Session())
            {
                List<int> IterNo = new List<int>();                           
                List<string> IfcGUIDs = new List<string>();          
                Dictionary<List<string>, List<int>> temp_dict=new Dictionary<List<string>, List<int>>();            

                var result = session1.WriteTransaction(tx =>
                {  
                    var Queryresult = tx.Run("MATCH(allVoid:Void)-[:Has_State { TO: ''}]-(Vr1: Version) -[] - (VrApp: Approval_Version) -[] -(all)"+
                        " RETURN (allVoid.IfcGUID) as IfcGUID,  Vr1.VersionNo as IterationNo",
                    new {});
                                       
                    foreach (var r in Queryresult)
                    {
                        var IterationNo = r["IterationNo"].As<int>();
                        var Approval_VrNo = r["Approval_VrNo"].As<int>();
                        IfcGUIDs.Add (r["IfcGUID"].As<string>());
                        IterNo.Add(IterationNo);                                                  
                    }
                    temp_dict.Add(IfcGUIDs, IterNo);
                    return temp_dict;
                });
                pp = result;               
            }
            return pp;
        }


        private Dictionary<string, Dictionary<string, string>> GetApprovalHistory_Selected_fn(string IfcGUID, int IterationNo)
        {

            Dictionary<string, Dictionary<string, string>> pp = new Dictionary<string, Dictionary<string, string>>();

            using (var session1 = _driver.Session())
            {
                List<string> temp_IterNo = new List<string>();
                List<string> temp_ApprVrNo = new List<string>();
                List<string> temp_Data_label = new List<string>();
                List<string> temp_Approvaldata = new List<string>();

                var result = session1.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID})-[:Has_State]-(Vr1: Version {VersionNo: $IterationNo}) -[] - (VrApp: Approval_Version) -[]-(all)" +
                                            "RETURN (all.name) as Approvaldata, labels(all) as DataLabel, VrApp.VersionNo as Approval_VrNo ,Vr1.VersionNo as IterationNo",
                    new { IfcGUID, IterationNo });

                    foreach (var r in Queryresult)
                    {
                        //var VoidIterationNo = (r["IterationNo"].As<int>()).ToString();
                        var Approval_VrNo = (r["Approval_VrNo"].As<int>()).ToString();
                        var DataLabel = (r["DataLabel"].As<List<string>>()).First();
                        var Approvaldata = r["Approvaldata"].As<string>();
                        //temp_IterNo.Add(IterationNo);
                        temp_ApprVrNo.Add(Approval_VrNo);
                        temp_Data_label.Add(DataLabel);
                        temp_Approvaldata.Add(Approvaldata);
                    }

                    List<List<string>> temptempvalue = new List<List<string>>();
                    //temptempvalue.Add(temp_IterNo);
                    temptempvalue.Add(temp_ApprVrNo);
                    temptempvalue.Add(temp_Data_label);
                    temptempvalue.Add(temp_Approvaldata);

                    List<string> temptempkey = new List<string>();
                    temptempkey.Add("Approval_VrNo");
                    temptempkey.Add("DataLabel");
                    temptempkey.Add("Approvaldata");

                    //Dictionary<string, List<string>>
                    List<string> Distinct_temp_ApprVrNo = temp_ApprVrNo.Distinct().ToList();
                    List<string> Distinct_temp_Data_label = temp_Data_label.Distinct().ToList();

                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo.Count().ToString());
                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo[0]);
                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo[1]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label.Count().ToString());
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[0]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[1]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[2]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[3]);

                    Dictionary<string, Dictionary<string, string>> tempDict2 = new Dictionary<string, Dictionary<string, string>>();

                    for (int x = 0; x < Distinct_temp_ApprVrNo.Count(); x++)
                    {
                        string Approval_VrNo1 = Distinct_temp_ApprVrNo[x];
                        //TaskDialog.Show("Message", Approval_VrNo1);
                        Dictionary<string, string> tempDict1 = new Dictionary<string, string>();

                        for (int y = 0; y < Distinct_temp_Data_label.Count(); y++)
                        {
                            string Data_label1 = Distinct_temp_Data_label[y];

                            for (int z = 0; z < temp_ApprVrNo.Count(); z++)
                            {
                                var Approval_VrNoins = temp_ApprVrNo[z];
                                var DataLabelins = temp_Data_label[z];
                                var Approvaldata = temp_Approvaldata[z];

                                if (Approval_VrNoins == Approval_VrNo1)
                                {
                                    if (DataLabelins == Data_label1)
                                    {
                                        tempDict1.Add(Data_label1, Approvaldata);
                                    }
                                }
                            }
                        }
                        tempDict2.Add(Approval_VrNo1, tempDict1);
                    }
                    return tempDict2;
                });

                pp = result;

                //-------------------------------------------------------------------------
            }
            return pp;
        }

        public static Dictionary<string, Dictionary<string, string>> GetApprovalHistory_Selected(string uri, string user, string password, string IfcGUID,int LastVersionNo)
        {

            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                //string LastVersion = DBConnection.LastVoidProposalVersionsInDB(IfcGUID).First();
                //int LastVersionNo = 0;// Convert.ToInt32(LastVersion);
                //return DBConnection.GetApprovalHistory_Selected_fn(IfcGUID, 0);
                return DBConnection.GetApprovalHistory_Selected_fn(IfcGUID, LastVersionNo);
            }
        }

        public static TaskDialogResult DialogResultApproval(Dictionary<string, Dictionary<string, string>> allOffset)
        {
            string TaskDialogMsg = "Choice of approval for the selected void " + "\n";

            var qry = from outer in allOffset
                      from inner in outer.Value
                      select inner.Key + ": " + inner.Value + "\n";// +  " ("+ outer.Key+ ")" + "/n";

            int count = 0;
            foreach (string s in qry)
            {
                if (count % 4 == 0)
                {
                    TaskDialogMsg = TaskDialogMsg + "\n" + "\n";
                }
                TaskDialogMsg = TaskDialogMsg + s;
                count++;
            }
            return TaskDialog.Show("Message", TaskDialogMsg);

        }




        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> GetPreviousVersionsPositionData_fn(string IfcGUID)
        {

            Dictionary<string, Dictionary<string, Dictionary<string, string>>> pp = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            using (var session1 = _driver.Session())
            {
                List<string> temp_IterNo = new List<string>();
                List<string> temp_PGNodedata = new List<string>();
                List<string> temp_ParaNodeLabel = new List<string>();
                List<string> temp_ParaNodedata = new List<string>();

                var result = session1.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID})-[:Has_State]-(Vr1:Version) -[:Has] - (PGNode)-[]-(ParaNode)" +
                                            "RETURN  Vr1.VersionNo as IterationNo, (PGNode.name) as PGNodedata, labels(ParaNode) as ParaNodeLabel, (ParaNode.name) as ParaNodedata",
                    new { IfcGUID});

                    foreach (var r in Queryresult)
                    {
                        var VoidIterationNo = (r["IterationNo"].As<int>()).ToString();
                        var PGNodedata = r["PGNodedata"].As<string>();
                        var ParaNodeLabel = (r["ParaNodeLabel"].As<List<string>>()).First();
                        var ParaNodedata = r["ParaNodedata"].As<string>();

                        temp_IterNo.Add(VoidIterationNo);
                        temp_PGNodedata.Add(PGNodedata);
                        temp_ParaNodeLabel.Add(ParaNodeLabel);
                        temp_ParaNodedata.Add(ParaNodedata);
                    }

                    //List<List<string>> temptempvalue = new List<List<string>>();
                    //temptempvalue.Add(temp_IterNo);
                    //temptempvalue.Add(temp_ApprVrNo);
                    //temptempvalue.Add(temp_Data_label);
                    //temptempvalue.Add(temp_Approvaldata);

                    //List<string> temptempkey = new List<string>();
                    //temptempkey.Add("Approval_VrNo");
                    //temptempkey.Add("DataLabel");
                    //temptempkey.Add("Approvaldata");

                    //Dictionary<string, List<string>>
                    List<string> Distinct_temp_IterNo = temp_IterNo.Distinct().ToList();
                    List<string> Distinct_temp_PGNodedata = temp_PGNodedata.Distinct().ToList();
                    List<string> Distinct_temp_ParaNodeLabel = temp_ParaNodeLabel.Distinct().ToList();
                    List<string> Distinct_temp_ParaNodedata = temp_ParaNodedata.Distinct().ToList();

                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo.Count().ToString());
                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo[0]);
                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo[1]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label.Count().ToString());
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[0]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[1]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[2]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[3]);

                    Dictionary<string, Dictionary<string, Dictionary<string, string>>> tempDict3 = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

                    for (int x = 0; x < Distinct_temp_IterNo.Count(); x++)
                    {
                        string IterNo1 = Distinct_temp_IterNo[x];
                        Dictionary<string, Dictionary<string, string>> tempDict2 = new Dictionary<string, Dictionary<string, string>>();

                        for (int y = 0; y < Distinct_temp_PGNodedata.Count(); y++)
                        {
                            string PGNodedata1 = Distinct_temp_PGNodedata[y];

                            Dictionary<string, string> tempDict1 = new Dictionary<string, string>();

                            for (int z = 0; z < Distinct_temp_ParaNodeLabel.Count(); z++)
                            {
                                string ParaNodeLabel1 = Distinct_temp_ParaNodeLabel[z];

                                for (int zz = 0; zz < temp_ParaNodedata.Count(); zz++)
                                {
                                    var IterNo_Check = temp_IterNo[zz];
                                    var PGNodedata_Check = temp_PGNodedata[zz];
                                    var ParaNodeLabel_Check = temp_ParaNodeLabel[zz];
                                    var ParaNodedata_Check = temp_ParaNodedata[zz];

                                    if (IterNo_Check == IterNo1)
                                    {
                                        if (PGNodedata_Check == PGNodedata1)
                                        {
                                            if (ParaNodeLabel_Check == ParaNodeLabel1)
                                            {
                                                tempDict1.Add(ParaNodeLabel_Check, ParaNodedata_Check);
                                            }
                                        }
                                    }
                                }
                            }
                            tempDict2.Add(PGNodedata1, tempDict1);
                        }
                        tempDict3.Add(IterNo1, tempDict2);
                    }
                    return tempDict3;
                });

                pp = result;

                //-------------------------------------------------------------------------
            }
            return pp;
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>> GetPreviousPositions(string uri, string user, string password, string IfcGUID)
        {
            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                return DBConnection.GetPreviousVersionsPositionData_fn(IfcGUID);
            }
        }         

        public static TaskDialogResult DialogResultGeometryPositions(Dictionary<string, Dictionary<string, string>> GetPreviousPositions)
        {
            string TaskDialogMsg = " Position and geometry history of the selected void" + "\n";

            var qry = from outer in GetPreviousPositions
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

            return TaskDialog.Show("Message", TaskDialogMsg);
        }
        #endregion Read data from database 


        //________________17,10,2021

        private List<string> GetVoidVersionsUsingDate_fn(string IfcGUID, string FromDate, string ToDate)
        {
            List<string> VersionNumbers = new List<string>();          

            using (var session = _driver.Session())
            {
                List<string> Versions_temp = new List<string>();
                var result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID})-[state: Has_State]-(vr: Version) " +
                                        "WHERE date($FromDate) <= state.FROM <= date($ToDate) " +
                                       "OR date($FromDate) <= state.TO <= date($ToDate) " + 
                                       "OR date($FromDate) > state.FROM AND state.TO > date($ToDate) " +      
                                       "OR date($FromDate) > state.FROM AND state.TO = '' " +
                                        "RETURN vr.VersionNo as VersionNumbers ", 
                                        new
                                        {
                                            IfcGUID,
                                            FromDate,
                                            ToDate,
                                        });

                    foreach (var versions_x in Queryresult)
                    {
                        Versions_temp.Add(versions_x["VersionNumbers"].As<string>());
                    }
                    return Versions_temp;
                });
                VersionNumbers = result;
            }
            return VersionNumbers;
        }

        public static List<string> GetVoidVersionsUsingDate(string uri, string user, string password, string IfcGUID, string FromDate, string ToDate)
        {
            List<string> VersionNos = new List<string>();
            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                VersionNos = DBConnection.GetVoidVersionsUsingDate_fn(IfcGUID, FromDate, ToDate);
            }
            return VersionNos;
        }



        //________________18,10,2021
        private List<List<string>> GetVoidVersionDataUsingDate_fn(string IfcGUID, string FromDate, string ToDate)
        {
            List<List<string>> Versions_Data = new List<List<string>>();

            using (var session = _driver.Session())
            {
                List<List<string>> VersionsList_temp = new List<List<string>>();
                
                //List<string> Versions_temp = new List<string>();

                var result = session.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID})-[Vstate: Has_State]-(vr: Version) " +
                                        "WHERE date($FromDate) <= Vstate.FROM <= date($ToDate) " +
                                       "OR date($FromDate) <= Vstate.TO <= date($ToDate) " +
                                       "OR date($FromDate) > Vstate.FROM AND Vstate.TO > date($ToDate) " +
                                       "OR date($FromDate) > Vstate.FROM AND Vstate.TO = '' " +
                                        "RETURN Vstate.FROM as Fromdate, Vstate.TO as Todate, vr.VersionNo as VersionNumbers ",
                                        new
                                        {
                                            IfcGUID,
                                            FromDate,
                                            ToDate,
                                        });

                    foreach (var versions_x in Queryresult)
                    {
                        List<string> Versions_temp = new List<string>();
                        Versions_temp.Add(versions_x["VersionNumbers"].As<string>());
                        string DateSpan = "From: "+ 
                                            versions_x["Fromdate"].As<string>() + " To: " + versions_x["Todate"].As<string>();
                       
                        Versions_temp.Add(DateSpan);

                        VersionsList_temp.Add(Versions_temp);
                    }
                    return VersionsList_temp;
                });
                Versions_Data = result;
            }
            return Versions_Data;
        }


        private Dictionary<string, Dictionary<string, string>> GetApprovalHistory_SelectedDate_fn(string IfcGUID, int IterationNo)
        {

            Dictionary<string, Dictionary<string, string>> pp = new Dictionary<string, Dictionary<string, string>>();

            using (var session1 = _driver.Session())
            {
                List<string> temp_IterNo = new List<string>();
                List<string> temp_ApprVrNo = new List<string>();
                List<string> temp_Data_label = new List<string>();
                List<string> temp_Approvaldata = new List<string>();

                var result = session1.WriteTransaction(tx =>
                {
                    var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID})-[:Has_State]-(Vr1: Version {VersionNo: $IterationNo}) -[] - (VrApp: Approval_Version) -[]-(all)" +
                                            "RETURN (all.name) as Approvaldata, labels(all) as DataLabel, VrApp.VersionNo as Approval_VrNo ,Vr1.VersionNo as IterationNo",
                    new { IfcGUID, IterationNo });

                    foreach (var r in Queryresult)
                    {
                        //var VoidIterationNo = (r["IterationNo"].As<int>()).ToString();
                        var Approval_VrNo = (r["Approval_VrNo"].As<int>()).ToString();
                        var DataLabel = (r["DataLabel"].As<List<string>>()).First();
                        var Approvaldata = r["Approvaldata"].As<string>();
                        //temp_IterNo.Add(IterationNo);
                        temp_ApprVrNo.Add(Approval_VrNo);
                        temp_Data_label.Add(DataLabel);
                        temp_Approvaldata.Add(Approvaldata);
                    }

                    List<List<string>> temptempvalue = new List<List<string>>();
                    //temptempvalue.Add(temp_IterNo);
                    temptempvalue.Add(temp_ApprVrNo);
                    temptempvalue.Add(temp_Data_label);
                    temptempvalue.Add(temp_Approvaldata);

                    List<string> temptempkey = new List<string>();
                    temptempkey.Add("Approval_VrNo");
                    temptempkey.Add("DataLabel");
                    temptempkey.Add("Approvaldata");

                    //Dictionary<string, List<string>>
                    List<string> Distinct_temp_ApprVrNo = temp_ApprVrNo.Distinct().ToList();
                    List<string> Distinct_temp_Data_label = temp_Data_label.Distinct().ToList();

                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo.Count().ToString());
                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo[0]);
                    //TaskDialog.Show("Message", Distinct_temp_ApprVrNo[1]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label.Count().ToString());
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[0]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[1]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[2]);
                    //TaskDialog.Show("Message", Distinct_temp_Data_label[3]);

                    Dictionary<string, Dictionary<string, string>> tempDict2 = new Dictionary<string, Dictionary<string, string>>();

                    for (int x = 0; x < Distinct_temp_ApprVrNo.Count(); x++)
                    {
                        string Approval_VrNo1 = Distinct_temp_ApprVrNo[x];
                        //TaskDialog.Show("Message", Approval_VrNo1);
                        Dictionary<string, string> tempDict1 = new Dictionary<string, string>();

                        for (int y = 0; y < Distinct_temp_Data_label.Count(); y++)
                        {
                            string Data_label1 = Distinct_temp_Data_label[y];

                            for (int z = 0; z < temp_ApprVrNo.Count(); z++)
                            {
                                var Approval_VrNoins = temp_ApprVrNo[z];
                                var DataLabelins = temp_Data_label[z];
                                var Approvaldata = temp_Approvaldata[z];

                                if (Approval_VrNoins == Approval_VrNo1)
                                {
                                    if (DataLabelins == Data_label1)
                                    {
                                        tempDict1.Add(Data_label1, Approvaldata);
                                    }
                                }
                            }
                        }
                        tempDict2.Add(Approval_VrNo1, tempDict1);
                    }
                    return tempDict2;
                });

                pp = result;

                //-------------------------------------------------------------------------
            }
            return pp;
        }


        public static Dictionary<List<string>, Dictionary<string, Dictionary<string, string>>> GetAllHistoryWithDate(string uri, string user, string password, string IfcGUID, string FromDate, string ToDate)
        {

            Dictionary<List<string>, Dictionary<string, Dictionary<string, string>>> AllHistory = new Dictionary<List<string>, Dictionary<string, Dictionary<string, string>>>();
            //Versionstate data, 
            using (var DBConnection = new GraphDBNeo4j(uri, user, password))
            {
                List<List<string>> Versions_Data= DBConnection.GetVoidVersionDataUsingDate_fn(IfcGUID, FromDate, ToDate); 
                foreach(var item in Versions_Data)
                {                   
                    int VoidVersionNo = Convert.ToInt32(item[0]);
                    Dictionary<string, Dictionary<string, string>> tempapproval = DBConnection.GetApprovalHistory_SelectedDate_fn(IfcGUID, VoidVersionNo);
                    AllHistory.Add(item, tempapproval);
                }                
            }           

            return AllHistory;
        }
       
    }
}
