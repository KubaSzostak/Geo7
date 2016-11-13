using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;

using AcApp = Bricscad.ApplicationServices.Application;
#endif

//[assembly: CommandClass(typeof(Geo7.AutoCAD.Commands.PromptsTest))]
//[assembly: CommandClass(typeof(Geo7.AutoCAD.Commands.TestEntityJig))]

namespace Geo7.Tools
{
    public class OrtoCommand : AcCommand
    {
        public OrtoCommand()
        {
            this.DisplayName = AcConsts.Orto;
            this.Description = AcConsts.OrtoDsc;
            //this.SmallImage = Resources.Orto;
        }

        protected override void ExecuteCore()
        {
            var lastPolarMode = (Int16)AcApp.GetSystemVariable("POLARMODE");
            var lastAutoSnap = (Int16)AcApp.GetSystemVariable("AUTOSNAP");
            var angDir = AcApp.GetSystemVariable("ANGDIR");
            var angBase = AcApp.GetSystemVariable("ANGBASE");

            try
            {
                // Gdy jest ustawiony CLOCKWISE AC uważa, że 100g=>300g, więc ustaw wartości zerowe
                AcApp.SetSystemVariable("ANGDIR", Convert.ToInt16(0));
                AcApp.SetSystemVariable("ANGBASE", 0.0);

                AcApp.SetSystemVariable("POLARANG", OrtoAngle); // Set polar angle value
                AcApp.SetSystemVariable("POLARMODE", lastPolarMode | 1); //Turns on relative tracking
                AcApp.SetSystemVariable("AUTOSNAP", 8 | lastAutoSnap); //Turns on polar tracking
            }
            finally 
            {
                // A potem wróć do wartości poprzednich
                AcApp.SetSystemVariable("ANGDIR", angDir);
                AcApp.SetSystemVariable("ANGBASE", angBase);
            }

            Ac.ExecuteCommand("_pline ");
        }


        private double OrtoAngle = Math.PI / 2.0;
    }

}
