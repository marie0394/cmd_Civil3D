using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commands_C3D
{
    public class MaterialCommands : IExtensionApplication
    {
        #region IExtensionApplication Members
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }
        #endregion
    }
}
