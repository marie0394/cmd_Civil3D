using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Diagnostics;
using Autodesk.Civil.DatabaseServices;
using System.Collections.Generic;
using Autodesk.Aec.Geometry;
using static System.Collections.Specialized.BitVector32;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Reflection;
using System;

namespace cmd_AutoCAD
{
    public class TextCommands : IExtensionApplication
    {

        #region IExtensionApplication Members
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

        #endregion

        [CommandMethod("AddValueToSingleNumber")]
        public void SumValueToText()
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // Get the editor and prompt the user to select a text element
            Editor ed = doc.Editor;

            PromptDoubleOptions inputOptions = new PromptDoubleOptions("\nEnter value to be added or : ")
            {
                AllowZero = true,
                AllowNegative = true,

            };

            string keywordColor = "Color";
            // Add a keyword option to the prompt
            inputOptions.Keywords.Add(keywordColor);

            // Get the value entered by the user
            PromptDoubleResult inputResult = ed.GetDouble(inputOptions);
            if (inputResult.Status == PromptStatus.Keyword)
            {
                if (inputResult.StringResult == keywordColor)
                {
                    Application.ShowAlertDialog("Entered keyword: " + inputResult.StringResult);
                }
                
            }


            // Loop until the user cancels or ends the command
            while (true)
            {
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
                    int decimalPlaces = 0;
                    string resultText;
                    if (!double.TryParse(text.TextString, out double number))
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
                    double valueToAdd = inputResult.Value; // Change this to the value you want to add
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


        [CommandMethod("AddValueToMultipleNumbers", CommandFlags.UsePickSet)]
        public void SumValueMultipleText()
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // Get the editor and prompt the user to select a text element
            Editor ed = doc.Editor;

            // Get the previously selected objects
            PromptSelectionResult psr = ed.SelectImplied();
            if (psr.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nNo objects were previously selected.");
                return;
            }

            SelectionSet ss = psr.Value;
            ObjectId[] objIds = ss.GetObjectIds();

            PromptDoubleOptions inputOptions = new PromptDoubleOptions("\nEnter value to be added or : ")
            {
                AllowZero = true,
                AllowNegative = true,

            };

            string keywordColor = "Color";
            // Add a keyword option to the prompt
            inputOptions.Keywords.Add(keywordColor);

            // Get the value entered by the user
            PromptDoubleResult inputResult = ed.GetDouble(inputOptions);
            if (inputResult.Status == PromptStatus.Keyword)
            {
                if (inputResult.StringResult == keywordColor)
                {
                    Application.ShowAlertDialog("Entered keyword: " + inputResult.StringResult);
                }

            }

            // Open the selected text element for write
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId objId in objIds)
                {
                    DBText text = (DBText)tr.GetObject(objId, OpenMode.ForWrite);

                    // Convert the text to a number
                    int decimalPlaces = 0;
                    string resultText;
                    if (!double.TryParse(text.TextString, out double number))
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
                    double valueToAdd = inputResult.Value; // Change this to the value you want to add
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
                }
                // Commit the transaction
                tr.Commit();


            }

        }

        public static ObjectId GetDesignProfile(Alignment align)
        {
            var retval = ObjectId.Null;
            using (Transaction tr = align.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in align.GetProfileIds())
                {
                    var prof = (Autodesk.Civil.DatabaseServices.Profile)tr.GetObject(id, OpenMode.ForRead);
                    if (prof.ProfileType == ProfileType.FG)
                        retval = id;
                    prof.Dispose();
                    if (retval != ObjectId.Null)
                        break;
                }
                tr.Commit();
            }

            return retval;
        }

        public static double calculateAngleBetweenAandB(Point3d InitialPt, Point3d FinalPt)
        {
            // Calculate the angle between pointA and pointB
            double angle = Math.Atan2(FinalPt.Y - InitialPt.Y, FinalPt.X - InitialPt.X);
            return angle;
        }

        public static Point3d GetCoordsAtOffset(Point3d InitialPt, double angle, double distance)
        {
            // Calculate the coordinates of pointC
            double x = InitialPt.X + (distance * Math.Cos(angle));
            double y = InitialPt.Y + (distance * Math.Sin(angle));

            return new Point3d(x, y, 0);
        }


    }




}
 