using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Entity = Autodesk.Civil.DatabaseServices.Entity;
using System.Runtime.ConstrainedExecution;

namespace cmd_AutoCAD   
{
    public class ExportCommands : IExtensionApplication
    {
        #region IExtensionApplication Members
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }
        #endregion

        [CommandMethod("FINDENTITYTYPE")]
        public static void FindTheTypeOfAEntity()
        {
            // Get the active Civil 3D document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // Get the editor and prompt the user to select an object
            Editor ed = doc.Editor;
            PromptEntityOptions options = new PromptEntityOptions("\nSelect a Civil 3D object: ");
            options.SetRejectMessage("\nInvalid selection.");
            PromptEntityResult result = ed.GetEntity(options);

            // If the user selects an object, get its type
            if (result.Status == PromptStatus.OK)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Entity entity = (Entity)tr.GetObject(result.ObjectId, OpenMode.ForRead);

                    // Get the type of the object
                    string objectType = entity.GetType().ToString();

                    ed.WriteMessage("\nSelected object type: " + objectType);

                    tr.Commit();
                }
            }
        }

        [CommandMethod("EXPORTLASTTWOVALUES")]
        public static void ExporLastTwoRows()
        {
            // Get the active Civil 3D document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // Get the editor and prompt the user to select an object
            Editor ed = doc.Editor;
            PromptEntityOptions options = new PromptEntityOptions("\nSelect a Civil 3D object: ");

            // False here means that it does not need to be an exact match. It allows derived types.
            options.AddAllowedClass(typeof(Autodesk.Civil.DatabaseServices.Table), false);
            options.SetRejectMessage("\nInvalid selection.");
            PromptEntityResult result = ed.GetEntity(options);

            //9df7e2a70615a7966dfda2d41c21ebd8

            // If the user selects an object, get its type
            if (result.Status != PromptStatus.OK)
            {
                return;
            }

            ObjectId tableId = result.ObjectId;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Autodesk.Civil.DatabaseServices.Table tableToExport = tr.GetObject(tableId, OpenMode.ForRead) as Autodesk.Civil.DatabaseServices.Table;
                // object[,] content = tableToExport.RowCount;
                // int srcLastRow = tableToExport.Rows.Count - 1;
               

                tr.Commit();
            }
            
        }



    }


}