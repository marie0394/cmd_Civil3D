using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using C3D_Entity = Autodesk.Civil.DatabaseServices.Entity;
using CAD_Entity = Autodesk.AutoCAD.DatabaseServices.Entity;
using System;
using Autodesk.AutoCAD.Windows;
using System.Collections.Generic;
using Autodesk.Civil.DatabaseServices.Styles;

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

            bool multiSelection = false;
            bool defaultColor = true;
            Color selectedColor = Color.FromColorIndex(ColorMethod.ByAci, 0);;

            bool isValueDouble = false;
            double valueToAdd = 0;


            while (!isValueDouble)
            {
                PromptDoubleOptions inputOptions = new PromptDoubleOptions("\nEnter value to be added or : ")
                {
                    AllowZero = true,
                    AllowNegative = true,

                };

                string keywordColor = "Color";
                string keywordMode = "Mode";

                // Add a keyword option to the prompt
                inputOptions.Keywords.Add(keywordColor);
                inputOptions.Keywords.Add(keywordMode);

                // Get the value entered by the user
                PromptDoubleResult inputResult = ed.GetDouble(inputOptions);
                if (inputResult.Status == PromptStatus.Keyword)
                {
                    if (inputResult.StringResult == keywordColor)
                    {
                        string keywordDefault = "Default";
                        string keywordOtherColor = "Select";
                        PromptKeywordOptions colorOptions = new PromptKeywordOptions("Select a mode:");
                        colorOptions.Keywords.Add(keywordDefault);
                        colorOptions.Keywords.Add(keywordOtherColor);
                        colorOptions.Keywords.Default = keywordDefault;
                        colorOptions.AllowNone = true;

                        PromptResult resultColor = ed.GetKeywords(colorOptions);
                        if (resultColor.Status == PromptStatus.OK)
                        {
                            string selectedKeyword = resultColor.StringResult;
                            if (selectedKeyword == keywordDefault)
                            {
                                defaultColor = true;
                            }
                            else if (selectedKeyword == keywordOtherColor)
                            {
                                
                                ed.WriteMessage("Select color from window");
                                ColorDialog cd = new ColorDialog
                                {
                                    IncludeByBlockByLayer = true
                                };
                                cd.ShowDialog();
                                selectedColor = cd.Color;
                                defaultColor = false;
                            }
                        }
                        else if (resultColor.Status == PromptStatus.Cancel)
                        {
                            ed.WriteMessage("Command canceled.");
                        }
                        // Is this else needed?
                        else
                        {
                            ed.WriteMessage("Error occurred.");
                        }
                    }
                    if (inputResult.StringResult == keywordMode)
                    {
                        string keywordSingle = "Single";
                        string keywordMulti = "Multiple";
                        PromptKeywordOptions modeOptions = new PromptKeywordOptions("Select a mode:");
                        modeOptions.Keywords.Add(keywordSingle);
                        modeOptions.Keywords.Add(keywordMulti);

                        PromptResult resultMode = ed.GetKeywords(modeOptions);
                        if (resultMode.Status == PromptStatus.OK)
                        {
                            string selectedKeyword = resultMode.StringResult;
                            if (selectedKeyword == keywordSingle)
                            {
                                multiSelection = false;
                            }
                            else if (selectedKeyword == keywordMulti)
                            {
                                multiSelection = true;
                            }
                        }
                        else if (resultMode.Status == PromptStatus.Cancel)
                        {
                            ed.WriteMessage("Command canceled.");
                        }
                        // Is this else needed?
                        else
                        {
                            ed.WriteMessage("Error occurred.");
                        }
                    }
                    

                }
                else
                {
                    isValueDouble = true;
                    valueToAdd = inputResult.Value; // Change this to the value you want to add
                }
            }

            

            // Loop until the user cancels or ends the command
            while (true)
            {
                PromptEntityOptions options = new PromptEntityOptions("\nSelect a text element: ");
                options.SetRejectMessage("\nInvalid selection.");
                options.AddAllowedClass(typeof(DBText), true);
                options.AddAllowedClass(typeof(MText), true);
                PromptEntityResult result = ed.GetEntity(options);
                if (result.Status != PromptStatus.OK)
                    return;

                string textString = "";
                
                // Open the selected text element for write
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    CAD_Entity textEntity = (CAD_Entity)tr.GetObject(result.ObjectId, OpenMode.ForWrite);
                    if (textEntity is DBText dbText)
                    {
                        textString = dbText.TextString;
                    }
                    else if (textEntity is MText mText)
                    {
                        textString = mText.Contents;
                    }


                    // Convert the text to a number
                    int decimalPlaces = 0;
                    string newText;
                    if (!double.TryParse(textString, out double textNumber))
                    {
                        ed.WriteMessage("\nThe selected text element does not contain a number.");
                        return; // Esto causa que el método termine
                    }

                    // Count the number of decimal places
                    int decimalIndex = textString.IndexOf(".");
                    if (decimalIndex >= 0)
                    {
                        decimalPlaces = textString.Length - decimalIndex - 1;
                    }


                    // Add a value to the number
                    
                    double newValue = textNumber + valueToAdd;

                    //Convert number to text
                    newText = newValue.ToString("0." + new string('0', decimalPlaces));

                    if (textEntity is DBText dbTextEntity)
                    {
                        dbTextEntity.TextString = newText;
                    }
                    else if (textEntity is MText mTextEntity)
                    {
                        mTextEntity.Contents = newText;
                    }

                    // Set the color of the text entity to purple.
                    if (!defaultColor)
                    {
                        textEntity.Color = selectedColor;
                    }

                    // Save the changes made to the text entity.
                    textEntity.RecordGraphicsModified(true);

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

        [CommandMethod("GetPointCoordinates")]
        public void GetPointCoordinates()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // List to store selected points
            List<Point3d> selectedPoints = new List<Point3d>();

            // Prompt user to click points
            PromptPointOptions ppo = new PromptPointOptions("\nClick points in the model space. Press Enter to finish: ");
            ppo.AllowNone = true;

            while (true)
            {
                PromptPointResult ppr = ed.GetPoint(ppo);
                if (ppr.Status == PromptStatus.OK)
                {
                    selectedPoints.Add(ppr.Value);
                }
                else
                {
                    break; // Exit loop if Enter key is pressed
                }
            }

            if (selectedPoints.Count > 0)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // Prompt user to select insertion point for the table
                    PromptPointOptions opts = new PromptPointOptions("\nSelect insertion point for the table: ");
                    PromptPointResult ptRes = ed.GetPoint(opts);
                    if (ptRes.Status == PromptStatus.OK)
                    {
                        Point3d insertPoint = ptRes.Value;

                        // Create a new table
                        BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        // Calculate the size of the table based on the number of selected points
                        int numRows = selectedPoints.Count; 
                        int numCols = 2; // Two columns for X and Y coordinates
                        Autodesk.AutoCAD.DatabaseServices.Table table = new Autodesk.AutoCAD.DatabaseServices.Table();
                        table.SetSize(numRows, numCols);
                        table.SetRowHeight(2);
                        table.SetColumnWidth(5);
                        btr.AppendEntity(table);
                        tr.AddNewlyCreatedDBObject(table, true);

                        // Set the insertion point for the table
                        table.Position = insertPoint;

                        // Setting table Style. This is needed so that the row style line works.
                        Autodesk.AutoCAD.DatabaseServices.TableStyle ts = new Autodesk.AutoCAD.DatabaseServices.TableStyle();
                        table.TableStyle = ts.ObjectId;

                        // Add the selected points to the table
                        for (int i = 0; i < selectedPoints.Count; i++)
                        {
                            table.Rows[i].Style = "Data";
                            table.Cells[i, 0].TextString = Math.Round(selectedPoints[i].Y, 3).ToString();
                            table.Cells[i, 1].TextString = Math.Round(selectedPoints[i].X, 3).ToString();
                        }

                        // Commit the transaction
                        tr.Commit();

                        ed.WriteMessage("\nSelected points added as a table.");
                    }
                }
            }
            else
            {
                ed.WriteMessage("\nNo points selected.");
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
 