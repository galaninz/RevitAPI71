using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI71
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private Document _doc;

        public List<FamilySymbol> TitleBlockTypes { get; } = new List<FamilySymbol>();
        public List<FamilySymbol> ViewLabales { get; } = new List<FamilySymbol>();
        public List<FamilySymbol> Viewports { get; } = new List<FamilySymbol>();
        public int SheetsQuantity { get; set; }
        public string Designer { get; set; }   
        public List<View> Views { get; } = new List<View>();
        public DelegateCommand SaveCommand { get; }
        public FamilySymbol SelectedTitleBlock { get; set; }
        public View SelectedView { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;

            TitleBlockTypes = SheetUtils.GetTitleBlocks(commandData); /////
            ViewLabales = SheetUtils.GetViewTitles(commandData);
            Viewports = SheetUtils.GetViewports(commandData);
            SheetsQuantity = 1;
            Designer = "Galanin";
            Views = ViewsUtils.GetViews(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (var ts = new Transaction(doc, "Create sheet"))
            {
                try
                {
                    ts.Start();
                    //if (TitleBlockTypes != null)
                    //{
                    //    LoadTitleBlock loadTitleBlock = new LoadTitleBlock();
                    //    loadTitleBlock.LoadFamily(_commandData);
                    //}
                   
                    for (int i = 0; i < SheetsQuantity; i++)
                    {
                        ViewSheet viewSheet = ViewSheet.Create(doc, SelectedTitleBlock.Id);

                        viewSheet.get_Parameter(BuiltInParameter.SHEET_DESIGNED_BY).Set(Designer);

                        if (viewSheet == null)
                        {
                            throw new Exception("Error");
                        }

                        ElementId duplicatedPlanId = SelectedView.Duplicate(ViewDuplicateOption.Duplicate);

                        UV location = new UV(viewSheet.Outline.Max.U - viewSheet.Outline.Min.U / 2, (viewSheet.Outline.Max.V - viewSheet.Outline.Min.V) / 2);

                        Viewport viewport = Viewport.Create(doc, viewSheet.Id, duplicatedPlanId, new XYZ(location.U, location.V, 0));

                }
                    ts.Commit();
                }
                catch
                {

                }
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;

        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
