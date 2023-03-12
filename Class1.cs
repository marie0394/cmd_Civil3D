using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace cmd_AutoCAD.text
{
    public class Class1 : IExtensionApplication
    {
        #region IExtensionApplication Members
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

        #endregion

        [CommandMethod("HelloWorld")]
        public void AddValueToSelectedTextElement()
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // Get the editor and prompt the user to select a text element
            Editor ed = doc.Editor;
            PromptEntityOptions options = new PromptEntityOptions("\nSelect a text element: ");
            options.SetRejectMessage("\nInvalid selection.");
            options.AddAllowedClass(typeof(DBText), true);
            PromptEntityResult result = ed.GetEntity(options);
            if (result.Status != PromptStatus.OK)
                return;

            // Open the selected text element for write
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBText text = (DBText)tr.GetObject(result.ObjectId, OpenMode.ForWrite);

                // Convert the text to a number
                double number;
                if (!double.TryParse(text.TextString, out number))
                {
                    ed.WriteMessage("\nThe selected text element does not contain a number.");
                    return;
                }

                // Add a value to the number
                double valueToAdd = 0.15; // Change this to the value you want to add
                double resultValue = number + valueToAdd;


                // Add a value to the text
                text.TextString = resultValue.ToString();


                // Commit the transaction
                tr.Commit();

            }

            // Refresh the display
            ed.Regen();
        }

        [CommandMethod("AddValueToTextNumber")]
        public void ChangeTextColor()
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // Get the editor and prompt the user to select a text element
            Editor ed = doc.Editor;
            PromptEntityOptions options = new PromptEntityOptions("\nSelect a text element: ");
            options.SetRejectMessage("\nInvalid selection.");
            options.AddAllowedClass(typeof(DBText), true);
            PromptEntityResult result = ed.GetEntity(options);
            if (result.Status != PromptStatus.OK)
                return;

            // Open the selected text element for write
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBText text = (DBText)tr.GetObject(result.ObjectId, OpenMode.ForWrite);

                // Convert the text to a number
                double number;
                int decimalPlaces = 0;
                string resultText;
                if (!double.TryParse(text.TextString, out number))
                {
                    ed.WriteMessage("\nThe selected text element does not contain a number.");
                    return; // Esto causa que el método termine
                }
                
                // Count the number of decimal places
                int decimalIndex = text.TextString.IndexOf(".");
                if (decimalIndex >= 0)
                {
                    decimalPlaces = text.TextString.Length - decimalIndex - 1;
                }
                

                // Add a value to the number
                double valueToAdd = 0.15; // Change this to the value you want to add
                double resultValue = number + valueToAdd;

                //Convert number to text
                resultText = resultValue.ToString("0." + new string('0', decimalPlaces));

                // Add a value to the text
                text.TextString = resultText;

                // Set the color of the text entity to purple.
                Color purple = Color.FromColorIndex(ColorMethod.ByAci, 211);
                text.Color = purple;

                // Save the changes made to the text entity.
                text.RecordGraphicsModified(true);

                // Commit the transaction
                tr.Commit();

            }

        }

    }
}
 