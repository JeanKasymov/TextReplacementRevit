using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace TextReplacement
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TextReplacementEntry
    {
        public void Main(UIApplication app)
        {
            try
            {
                MainWindow mainWindow = new MainWindow(new ViewModel(new ModelService(app)));
                mainWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", ex.Message);
            }
        }
        public static string RussianName { get; private set; } = "Замена текста";

    }
}
