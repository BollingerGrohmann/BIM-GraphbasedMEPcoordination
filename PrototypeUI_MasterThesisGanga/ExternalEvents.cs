using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PrototypeUI_MasterThesisGanga
{
    public class ExternalEvents
    {
        public MA_EventHandler MY_EventHandler { get; private set; }
        public static ExternalEvent MyExEv { get; private set; }
        public void CreateEvents()
        {
            MY_EventHandler = new MA_EventHandler();
            MyExEv = ExternalEvent.Create(MY_EventHandler);
        } 
    }
}
