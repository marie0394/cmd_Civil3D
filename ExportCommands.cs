using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Entity = Autodesk.Civil.DatabaseServices.Entity;

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

        [CommandMethod("EXPORTLASTTWOCOLUMNVALUES")]
        public static void SelectTotalVolumeTable()
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
    }


}