using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Reflection;

namespace PrototypeUI_MasterThesisGanga
{

    public class App : IExternalApplication
    {
        const string RIBBON_TAB = "Master Thesis GSS";
        const string RIBBON_PANEL = "Prototype";

        // Implement the OnStartup method to register events when Revit starts.       
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(RIBBON_TAB);
            }
            catch (Exception) { }
            
            RibbonPanel panel = null;
            List<RibbonPanel> panels = application.GetRibbonPanels(RIBBON_TAB);
            foreach (RibbonPanel pnl in panels)
            {
                if(pnl.Name == RIBBON_PANEL)
                {
                    panel = pnl;
                    break;
                }
            }
            if (panel == null)
            {
                panel = application.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL);
            }
            Image ButtonImage = PrototypeUI_MasterThesisGanga.Properties.Resources.PrototypeButtonIcon;
            ImageSource imageSource = GetImageSource(ButtonImage);

            PushButtonData btnData = new PushButtonData("Prototype","Slot & Opening Planning",Assembly.GetExecutingAssembly().Location,"PrototypeUI_MasterThesisGanga.Prototype_Main" )
            {
                ToolTip = "Master Thesis @CMS,TUM",
                LongDescription = "Database oriented Slot and Opening Planning",
                Image = imageSource,
                LargeImage= imageSource
            };

            PushButton button = panel.AddItem(btnData) as PushButton;
            button.Enabled = true;

            // Register related events
            ExternalEvents externalEvents = new ExternalEvents();
            externalEvents.CreateEvents(); 
            

            return Result.Succeeded;
        }

        // Implement this method to unregister the subscribed events when Revit exits.
        public Result OnShutdown(UIControlledApplication application)
        {           
            return Result.Succeeded;
        }

        private BitmapSource GetImageSource(Image image) 
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            return bmp;
        }
        

    }
}