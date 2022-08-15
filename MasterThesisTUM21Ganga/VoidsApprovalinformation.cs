namespace MasterThesisTUM21Ganga
{
//    internal class VoidsApprovalinformation
//    {

//    }
}

//private Dictionary<string, Dictionary<string, string>> NEWReadVoidApprovalVersionData_fn(string IfcGUID, int IterationNo)
//{
//    //Dictionary<string, string> pp = new Dictionary<string, string>();
//    Dictionary<string, List<string>> pp = new Dictionary<string, List<string>>();

//    using (var session1 = _driver.Session())
//    {
//        List<string> temp_IterNo = new List<string>();
//        List<string> temp_ApprVrNo = new List<string>();
//        List<string> temp_Data_label = new List<string>();
//        List<string> temp_Approvaldata = new List<string>();
//        //Dictionary<string, List<string>> temp_dict;
//        //Dictionary<string, Dictionary<string, List<string>>> data_dict;

//        List<string> temp_key = new List<string>();
//        List<string> temp_value = new List<string>();
//        //List<VoidApprovalinformation> VoidsApprovalinfo= new List<VoidApprovalinformation>();
//        Dictionary<string, List<string>> temp_dict = new Dictionary<string, List<string>>();


//        var result = session1.WriteTransaction(tx =>
//        {   /*
//                    var Queryresult = tx.Run("MATCH (:Void {IfcGUID: $IfcGUID })-[:Has_State {TO: ''}]-(Vr1:Version)-[*]-(all_connected)-[*]-(GetAll) " +
//                                        "WHERE all_connected.name = 'STR' OR all_connected.name = 'ARC' OR all_connected.name = 'MEP' OR all_connected.name = 'Geometry' " +
//                                        "RETURN GetAll",

//                    //  var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID })-[:Has_State {TO: ''}]-(Vr1: Version)-[]-(VrApp:Approval_Version)" +
//                    //"RETURN VrApp.VersionNo as VersionNumbers",
//                    //new { IfcGUID });


//                    MATCH(:Void { IfcGUID: "1h2057DXYgP7c8E6kna5rUxt" })-[:Has_State { TO: ''}]-(Vr1: Version) -[] - (VrApp: Approval_Version) -[] -(Apprdisc:ApprovalDiscipline) -[]-(all) 
//                    RETURN (all.name) as Approvaldata, Apprdisc.name as Discipline, VrApp.VersionNo as Approval_VrNo ,Vr1.VersionNo as IterationNo


//                    MATCH(allVoid:Void)-[:Has_State { TO: ''}]-(Vr1: Version) -[] - (VrApp: Approval_Version) -[] -(all)                     
//                    RETURN (allVoid.IfcGUID) as IfcGUID,  (allVoid.name) as VoidName, Vr1.VersionNo as IterationNo, VrApp.VersionNo as Approval_VrNo, labels(all) as DataLabel, (all.name) as Approvaldata

//                     */
//            var Queryresult = tx.Run("MATCH(:Void { IfcGUID: $IfcGUID})-[:Has_State { TO: ''}]-(Vr1: Version { VersionNo: $IterationNo}) -[] - (VrApp: Approval_Version) -[]-(all)" +
//            "RETURN (all.name) as Approvaldata, labels(all) as DataLabel, VrApp.VersionNo as Approval_VrNo ,Vr1.VersionNo as IterationNo",
//            new { IfcGUID, IterationNo });

//            foreach (var r in Queryresult)
//            {
//                //Get as an INode instance to access properties.
//                //var node = r["STRApprovalInformation"].As<INode>();
//                //Properties are a Dictionary<string,object>, so you need to 'As' them
//                //var name = node["name"].As<string>();
//                //var label = node.Labels.Single().As<string>();

//                //var VoidIterationNo = (r["IterationNo"].As<int>()).ToString();
//                var Approval_VrNo = (r["Approval_VrNo"].As<int>()).ToString();
//                var DataLabel = (r["DataLabel"].As<List<string>>()).First();
//                var Approvaldata = r["Approvaldata"].As<string>();
//                //temp_IterNo.Add(IterationNo);
//                temp_ApprVrNo.Add(Approval_VrNo);
//                temp_Data_label.Add(DataLabel);
//                temp_Approvaldata.Add(Approvaldata);

//                //temp_key.Add(i.ToString());
//                //temp_value.Add(DataLabel);
//                //i = i + 1;                    
//            }

//            List<List<string>> temptempvalue = new List<List<string>>();
//            //temptempvalue.Add(temp_IterNo);
//            temptempvalue.Add(temp_ApprVrNo);
//            temptempvalue.Add(temp_Data_label);
//            temptempvalue.Add(temp_Approvaldata);

//            List<string> temptempkey = new List<string>();
//            temptempkey.Add("Approval_VrNo");
//            temptempkey.Add("DataLabel");
//            temptempkey.Add("Approvaldata");

//            temp_dict = temptempkey.Zip(temptempvalue, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

//            //Dictionary<string, List<string>>
//            List<string> Distinct_temp_ApprVrNo = temp_ApprVrNo.Distinct().ToList();
//            List<string> Distinct_temp_Data_label = temp_Data_label.Distinct().ToList();

//            //string Approvaldata1 = Distinct_temp_ApprVrNo[y];
//            //temp_dict["Approvaldata"]

//            Dictionary<string, Dictionary<string, string>> tempDict2 = new Dictionary<string, Dictionary<string, string>>();

//            foreach (var r in Queryresult)
//            {
//                for (int x = 0; x < Distinct_temp_ApprVrNo.Count(); x++)
//                {
//                    string Approval_VrNo1 = Distinct_temp_ApprVrNo[x];
//                    Dictionary<string, string> tempDict1 = new Dictionary<string, string>();

//                    if ((r["Approval_VrNo"].As<int>()).ToString() == Approval_VrNo1)
//                    {


//                        for (int y = 0; y < Distinct_temp_Data_label.Count(); y++)
//                        {
//                            string Data_label1 = Distinct_temp_ApprVrNo[y];

//                            if ((r["DataLabel"].As<int>()).ToString() == Data_label1)
//                            {
//                                tempDict1.Add(Data_label1, r["Approvaldata"].As<string>());
//                            }
//                        }
//                    }
//                    tempDict2.Add(Approval_VrNo1, tempDict1);
//                }
//            }
//            return tempDict2;
//        });

//        pp = result;

//        //-------------------------------------------------------------------------
//    }
//    return pp;
//}

//for (int x = 0; x< Distinct_temp_ApprVrNo.Count(); x++)
//{
//    string Approval_VrNo1 = Distinct_temp_ApprVrNo[x];

//    for (int y = 0; y < Distinct_temp_Data_label.Count(); y++)
//    {
//        Dictionary<string, string> tempDict1 = new Dictionary<string, string>();                           
//        string Data_label1 = Distinct_temp_ApprVrNo[y];
//        string Approvaldata1 = Distinct_temp_ApprVrNo[y];
//        temp_dict["Approvaldata"]
//    }
//}
//TaskDialog.Show("Message", Data_label1);
//foreach (var rr in Queryresult)
//{
//    var Approval_VrNoins = (rr["Approval_VrNo"].As<int>()).ToString();
//    var DataLabelins = (rr["DataLabel"].As<List<string>>()).First();
//    TaskDialog.Show("Message", DataLabelins);

//    if (Approval_VrNoins == Approval_VrNo1)
//    {
//        if (DataLabelins == Data_label1)
//        {
//            tempDict1.Add(Data_label1, (rr["DataLabel"].As<List<string>>()));
//        }
//    }
//}