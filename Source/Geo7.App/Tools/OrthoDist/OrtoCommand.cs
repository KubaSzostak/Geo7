using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Geo7.Tools
{
    public class OrtoCommand : AcCommand
    {
        public OrtoCommand()
        {
            this.DisplayName = AppServices.Strings.Orto;
            this.Description = AppServices.Strings.OrtoDsc;
            //this.SmallImage = Resources.Orto;
        }

        protected override void ExecuteCore()
        {
            var lastPolarMode = (Int16)Ac.GetSystemVariable("POLARMODE");
            var lastAutoSnap = (Int16)Ac.GetSystemVariable("AUTOSNAP");
            var angDir = Ac.GetSystemVariable("ANGDIR");
            var angBase = Ac.GetSystemVariable("ANGBASE");

            try
            {
                // Gdy jest ustawiony CLOCKWISE AC uważa, że 100g=>300g, więc ustaw wartości zerowe
                Ac.SetSystemVariable("ANGDIR", Convert.ToInt16(0));
                Ac.SetSystemVariable("ANGBASE", 0.0);

                Ac.SetSystemVariable("POLARANG", OrtoAngle); // Set polar angle value
                Ac.SetSystemVariable("POLARMODE", lastPolarMode | 1); //Turns on relative tracking
                Ac.SetSystemVariable("AUTOSNAP", 8 | lastAutoSnap); //Turns on polar tracking
            }
            finally 
            {
                // A potem wróć do wartości poprzednich
                Ac.SetSystemVariable("ANGDIR", angDir);
                Ac.SetSystemVariable("ANGBASE", angBase);
            }

            Ac.ExecuteCommand("_pline ");
        }


        private double OrtoAngle = Math.PI / 2.0;
    }

}
