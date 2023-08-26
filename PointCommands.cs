using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using System.Xml.Linq;
using Autodesk.Civil.DatabaseServices.Styles;

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
            CivilDocument civilDocument = CivilApplication.ActiveDocument;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Access the COGO points collection
                CogoPointCollection cogoPoints = CogoPointCollection.GetCogoPoints(db);
                PointGroupCollection pointGroups = PointGroupCollection.GetPointGroups(db);
                LabelStyleCollection pointLabelStyles = civilDocument.Styles.LabelStyles.PointLabelStyles.LabelStyles;
                PointStyleCollection pointStyles = civilDocument.Styles.PointStyles;

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
                    ObjectId newPointGroupId = CreateNewPointGroup(uniqueDescription, pointGroups);
                    ObjectId newPointLabelStyleId = CreateNewLabelStyle(uniqueDescription, purple, pointLabelStyles);
                    ObjectId newPointStyleId = CreateNewPointStyle(uniqueDescription, purple, pointStyles);
                    PointGroup pointGrp = newPointGroupId.GetObject(OpenMode.ForWrite) as PointGroup;
                    pointGrp.PointLabelStyleId = newPointLabelStyleId;
                    pointGrp.PointStyleId = newPointStyleId;
                }
                

                tr.Commit();

            }
        }

        public static ObjectId CreateNewPointGroup(string name, PointGroupCollection pointGroups)
        {
            ObjectId newPointGroupId = pointGroups.Add(name);
            return newPointGroupId;
        }

        public static ObjectId CreateNewLabelStyle(string name, Color color, LabelStyleCollection pointLabelStyles)
        {
            ObjectId pointLabelStyleId = pointLabelStyles.Add(name);
            return pointLabelStyleId;
            
        }

        public static ObjectId CreateNewPointStyle(string name, Color color, PointStyleCollection pointStyles)
        {
            ObjectId newPointStyleId = pointStyles.Add(name);
            PointStyle pointStyle = newPointStyleId.GetObject(OpenMode.ForWrite) as PointStyle;
            PointDisplayStyleType markerType = 0;
            PointDisplayStyleType labelType = 0;
            DisplayStyle displayStylePlanMarker = pointStyle.GetDisplayStylePlan(markerType); //0 for marker, `1 for label
            DisplayStyle displayStylePlanLabel = pointStyle.GetDisplayStylePlan(labelType); //0 for marker, `1 for label
            displayStylePlanMarker.Color = color;
            displayStylePlanLabel.Color = color;
            return newPointStyleId;
        }
    }
}
