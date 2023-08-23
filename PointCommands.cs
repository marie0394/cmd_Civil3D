using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using System.Xml.Linq;

namespace cmd_C3D
{
    public class PointCommands : IExtensionApplication
    {
        #region IExtensionApplication Members
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

        #endregion

        [CommandMethod("MULTIPOINTGROUPS")]
        public static void CreateMultiplePointGroups()
        {
            // Get the active Civil 3D document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Access the COGO points collection
                CogoPointCollection cogoPoints = CogoPointCollection.GetCogoPoints(db);
                PointGroupCollection pointGroups = PointGroupCollection.GetPointGroups(db);

                // Create a set to store unique description values
                HashSet<string> uniqueDescriptions = new HashSet<string>();

                foreach (ObjectId pointId in cogoPoints)
                {
                    CogoPoint cogoPoint = tr.GetObject(pointId, OpenMode.ForRead) as CogoPoint;
                    if (cogoPoint != null)
                    {
                        // Extract the description value
                        string rawDescription = cogoPoint.RawDescription;

                        // Add to the set (automatically filters out duplicates)
                        uniqueDescriptions.Add(rawDescription);
                        ed.WriteMessage("Unique Description: " + rawDescription + "\n");
                    }
                }

                foreach (string uniqueDescription in uniqueDescriptions)
                {
                    // ObjectId newPointGroup = pointGroups.Add(uniqueDescription);
                    Color purple = Color.FromColorIndex(ColorMethod.ByAci, 211);
                    CreateNewPointGroup(uniqueDescription, purple, pointGroups);
                }



                tr.Commit();

            }
        }

        public static void CreateNewPointGroup(string name, Color color, PointGroupCollection pointGroups)
        {
            ObjectId newPointGroup = pointGroups.Add(name);
        }
    }
}
