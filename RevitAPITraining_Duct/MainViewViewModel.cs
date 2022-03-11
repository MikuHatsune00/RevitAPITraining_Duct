using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitAPITrainingLibrary;
using Prism.Commands;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Mechanical;

namespace RevitAPITraining_Duct
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public List<MEPSystemType> MEPSystemTypes { get; } = new List<MEPSystemType>();
        public List<Level> DuctLevels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public DuctType SelectedDuctType { get; set; }
        public MEPSystemType SelectedMEPSystemType { get; set; }
        public Level SelectedLevel { get; set; }
        public double DuctOffset { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public Element SelectElement (UIDocument uidoc, Document doc)
        { Reference reference = uidoc.Selection.PickObject(ObjectType.Element);
            Element element = uidoc.Document.GetElement(reference);
            return element;
        }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctsUtils.GetDuctTypes(commandData);
            DuctLevels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            MEPSystemTypes = DuctsUtils.GetMEPSystemTypes(commandData);
            DuctOffset = 100;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || SelectedDuctType == null || SelectedLevel == null || SelectedMEPSystemType == null)
                return;

            var points = new List<Point>();
            for (int i=0; i<Points.Count; i++)
            {
                if (i == 0)
                    continue;
                var prevPoint = Points[0];
                var currentPoint = Points[1];
               

            }
                

             

            using (var ts=new Transaction(doc,"Create duct"))
            { ts.Start();
                
                    Duct.Create(doc,SelectedMEPSystemType.Id, SelectedDuctType.Id, SelectedLevel.Id,
                      Points[0], Points[1]);
                
                ts.Commit();
            }
            Element e = SelectElement(uidoc, doc);
            using (var ts1=new Transaction(doc,"Select duct for offset"))
            if (e is Duct)
            {
                    ts1.Start();
                Parameter OffsetParameter = e.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
                OffsetParameter.Set(UnitUtils.ConvertToInternalUnits(DuctOffset, UnitTypeId.Millimeters));
                    ts1.Commit();
            }

            RaiseCloseRequest();
        }


        public event EventHandler CloseRequest;
        public void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
