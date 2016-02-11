using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kea
{

    public interface IStepProgress : IProgress<double>
    {
        /// <summary>
        /// Obtiene el último progreso reportado
        /// </summary>
        double LastValue { get; }

        /// <summary>
        /// Obtiene el número de pasos de este progreso
        /// </summary>
        double Steps { get; }

        /// <summary>
        /// Informa de n pasos completados a este progreso
        /// </summary>
        void Step(double Steps);

        /// <summary>
        /// Resetea todo el progreso de este objeto 
        /// </summary>
        /// <param name="Value"></param>
        void Reset();

        event EventHandler<ProgressEventArgs> OnReport;

    }

    public interface IAggregateProgress : IStepProgress
    {
        /// <summary>
        /// Agrega un hijo a este progreso
        /// </summary>
        void AddChild(IStepProgress Prog);
    }



    public static class ProgressExtension
    {


        public static IAggregateProgress Child(this IProgress<double> Prog, double Current, double Steps)
        {
            AggregateProgress P = new AggregateProgress(Steps);
            P.OnReport += delegate
            {
                Prog?.Report(P.LastValue / P.Steps);
            };
            P.Step(Current);
            return P;
        }

        public static IAggregateProgress Child(this IProgress<double> Prog, double Steps)
        {
            return Prog.Child(0, Steps);
        }

        /// <summary>
        /// Devuelve un objeto que representa el progreso de un paso de esta tarea
        /// </summary>
        /// <param name="Prog">Progreso total</param>
        /// <param name="Steps">Numero de pasos de la tarea que representa el paso actual</param>
        public static IAggregateProgress Child(this IAggregateProgress Prog, double Steps)
        {
            AggregateProgress P = new AggregateProgress(Steps);
            Prog?.AddChild(P);
            return P;
        }

        /// <summary>
        /// Avanza un paso en el progreso de esta tarea
        /// </summary>
        public static void Step(this IStepProgress Prog)
        {
            Prog.Step(1);
        }

    }

    class AggregateProgress : IAggregateProgress
    {
        public AggregateProgress(double Steps)
        {
            this.steps = Steps;
        }


        readonly double steps;
        public event EventHandler<ProgressEventArgs> OnReport;

        public double Steps
        {
            get
            {
                return steps;
            }
        }

        double stepAcum;
        public double LastValue
        {
            get
            {
                return Childs.Sum(x => x.LastValue / x.Steps) + stepAcum;
            }
        }


        ConcurrentBag<IStepProgress> Childs = new ConcurrentBag<IStepProgress>();


        public void AddChild(IStepProgress Child)
        {
            Childs.Add(Child);
            OnReport?.Invoke(this, new ProgressEventArgs(LastValue));
            Child.OnReport += Child_OnReport;
        }

        private void Child_OnReport(object sender, ProgressEventArgs e)
        {
            OnReport?.Invoke(this, new ProgressEventArgs(LastValue));
        }

        public void Reset()
        {
            stepAcum = 0;
            var newBag = new ConcurrentBag<IStepProgress>();
            Interlocked.Exchange(ref Childs, newBag);
        }

        public void Step(double Steps)
        {
            stepAcum += Steps;
            OnReport?.Invoke(this, new ProgressEventArgs(LastValue));
        }
        public void Report(double Value)
        {
            Reset();
            Step(Value);
        }
    }

    /// <summary>
    /// Define un progreso de una tarea
    /// </summary>
    class StepProgress : IStepProgress
    {
        /// <summary>
        /// Crea un progreso con un conjunto de pasos conocido
        /// </summary>
        /// <param name="Steps">El numero de pasos total de la tarea</param>
        public StepProgress(double Steps)
        {
            this.steps = Steps;
        }


        double steps;
        double currentStep = 0;

        public double Steps
        {
            get
            {
                return steps;
            }
        }

        public event EventHandler<ProgressEventArgs> OnReport;

        public double LastValue
        {
            get
            {
                return currentStep;
            }
        }


        public double RelativeProgress
        {
            get
            {
                return currentStep / steps;
            }
        }


        public void Step(double Steps)
        {
            currentStep += Steps;
            OnReport?.Invoke(this, new ProgressEventArgs(LastValue));
        }

        public void Reset()
        {
            currentStep = 0;
            OnReport?.Invoke(this, new ProgressEventArgs(LastValue));
        }

        public void Report(double Value)
        {
            Reset();
            Step(Value);
        }
    }

}
