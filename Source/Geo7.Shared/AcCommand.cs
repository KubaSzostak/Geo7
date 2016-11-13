using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Forms.Integration;


#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Bricscad.Windows;
#endif

namespace System
{
    

    public abstract class AcCommand : ICommand //,Teigha.Runtime.ICommandLineCallable
    {
        private static bool InProgress = false;
        protected bool DisplayErrormMessageBox = true;

        public string DisplayName { protected set; get; }
        public string Description { protected set; get; }

        public Bitmap SmallImage { protected set; get; }
        public Bitmap LargeImage { protected set; get; }


        public AcCommand()
        {
            DisplayName = this.GetType().Name;
            CommandManager.RequerySuggested += CanExecuteChangedRequerySuggested;
        }

        private void CanExecuteChangedRequerySuggested(object sender, EventArgs e)
        {
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            //add { Autodesk.AutoCAD.ApplicationServices.Application.Idle += value; }
            //remove { Autodesk.AutoCAD.ApplicationServices.Application.Idle -= value; }
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return
                (Ac.Doc != null)
                && (Ac.Db != null)
                && (!InProgress)
                //&& string.IsNullOrEmpty(Ac.Doc.CommandInProgress)
                && CanExecuteCore() ;
        }

        void ICommand.Execute(object parameter)
        {
            if (!(this as ICommand).CanExecute(parameter))
            {
                Ac.ShowAlertDialog("Cannot execute " + this.DisplayName + " (Possible reason: there is no drawing environment)");
                return;
            }

            try
            {
                InProgress = true;
                AppServices.Log.Add(this.GetType().FullName + ": Executing...");
                //CommandManager.InvalidateRequerySuggested();

                // Requests to modify objects or access AutoCAD can occur in any context, and coming from any number of applications. 
                // To prevent conflicts with other requests, you are responsible for locking a document before you modify it. 
                // http://help.autodesk.com/view/ACD/2015/ENU/?guid=GUID-A2CD7540-69C5-4085-BCE8-2A8ACE16BFDD
                //using (var docLock = Ac.Doc.LockDocument()) -> Lock document only when needed (Button pressed)

                //using (var uiLock = Ac.StartUserInteraction()) -> Lock user interaction only when needed (Button pressed)
                {
                    ExecuteCore();
                    //uiLock.End();
                }
                AppServices.Log.Add(this.GetType().FullName + ": Done.");
            }
            catch (Exception ex)
            {
                Ac.WriteError(ex, this.GetType().Name + ".Execute()", this.DisplayName);
                if (this.DisplayErrormMessageBox)
                    Ac.ShowAlertDialog(ex.Message);
            }
            finally
            {
                InProgress = false;
                Ac.ClearProgress();
            }

            var trans = Ac.Db.TransactionManager.TopTransaction;
            if (trans != null)
                trans.Abort();

            CommandManager.InvalidateRequerySuggested();
        }
        

        public void Execute()
        {
            (this as ICommand).Execute(null);
        }

        protected abstract void ExecuteCore();

        protected virtual bool CanExecuteCore()
        {
            return true;
        }

    }

    public class AcDelegateCommand : AcCommand
    {
        public AcDelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            _execMethod = executeMethod;
            _canExecMethod = canExecuteMethod;
        }

        private Action _execMethod;
        private Func<bool> _canExecMethod;

        protected override bool CanExecuteCore()
        {
            if (_canExecMethod != null)
                return _canExecMethod();
            else
                return true;
        }

        protected override void ExecuteCore()
        {
            _execMethod();
        }
    }


    public abstract class PaletteCommand : AcCommand
    {
        protected void AddPalette(PaletteSet paletteSet, System.Windows.Controls.Page page)
        {
            ElementHost host = new ElementHost();
            host.AutoSize = true;
            host.Dock = System.Windows.Forms.DockStyle.Fill;

            // Page może być tylko w Window lub Frame
            System.Windows.Controls.Frame pageFrame = new System.Windows.Controls.Frame();
            pageFrame.Content = page;
            host.Child = pageFrame;

            paletteSet.Add(page.Title, host);
        }
    }

}
