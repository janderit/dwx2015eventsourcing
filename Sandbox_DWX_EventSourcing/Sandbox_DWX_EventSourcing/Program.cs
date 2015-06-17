using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox_DWX_EventSourcing
{
    class Program
    {
        private const int Tausend = 1000;
        private const int Million = Tausend*Tausend;
        private const int Millionen = Million;

        private static void Main(string[] args)
        {

            Console.Out.WriteLine("Creating dummy data...");

            var historie = new List<object>();

            var timer = Timer();
            const int anzahl_demo_Rechnungen = 1*Million;

            foreach (var id in Enumerable.Range(0, anzahl_demo_Rechnungen))
            {
                historie.Add(new Rechnung_wurde_erstellt(id, 18000));
                historie.Add(new Zahlung_wurde_geleistet(id, 14000));
                historie.Add(new Zahlung_wurde_geleistet(id, 4000 - 1));
            }

            Console.Out.WriteLine("History ready, " + timer());
            Console.Out.WriteLine("Press <enter>...");
            Console.ReadLine();



            Console.Out.WriteLine("");
            Console.Out.WriteLine("Naive lookup:");

            var random = new Random();

            for (int i = 0; i < 2; i++)
            {
                timer = Timer();
                var test_id = random.Next(anzahl_demo_Rechnungen);
                var betrag = Offener_Betrag(historie, test_id);
                Console.Out.WriteLine(
                    "Offener Betrag für Rechnung {0}: {1} ({2})",
                    test_id,
                    betrag,
                    timer());
            }

            Console.Out.WriteLine("Press <enter>...");
            Console.ReadLine();


            Console.Out.WriteLine("");
            Console.Out.WriteLine("Generic lookup:");
            for (int i = 0; i < 2; i++)
            {
                timer = Timer();
                var test_id = random.Next(anzahl_demo_Rechnungen);

                var offener_Betrag = new Projection<int>()
                    .Fuer<Rechnung_wurde_erstellt>((summe, e) => (e.Id == test_id) ? summe + e.Betrag_Cents : summe)
                    .Fuer<Zahlung_wurde_geleistet>((summe, e) => (e.Id == test_id) ? summe - e.Betrag_Cents : summe);

                var betrag = offener_Betrag.Eval(0, historie);
                Console.Out.WriteLine(
                    "Offener Betrag für Rechnung {0}: {1} ({2})",
                    test_id,
                    betrag,
                    timer());
            }

            Console.Out.WriteLine("Press <enter>...");
            Console.ReadLine();




            Console.Out.WriteLine("");
            Console.Out.WriteLine("Creating ID index");
            timer = Timer();

            var rechnungs_id = new Projection<int>()
                .Fuer<Rechnung_wurde_erstellt>((summe, e) => e.Id)
                .Fuer<Zahlung_wurde_geleistet>((summe, e) => e.Id);

            var sorted = Create_index(historie, rechnungs_id);
            Console.Out.WriteLine("ID index ready (" + timer() + ")");

            Console.Out.WriteLine("Press <enter>...");
            Console.ReadLine();




            Console.Out.WriteLine("");
            Console.Out.WriteLine("Lookup with index:");
            for (int i = 0; i < 2; i++)
            {
                timer = Timer();
                var test_id = random.Next(anzahl_demo_Rechnungen);

                var offener_Betrag = new Projection<int>()
                    .Fuer<Rechnung_wurde_erstellt>((summe, e) => summe + e.Betrag_Cents)
                    .Fuer<Zahlung_wurde_geleistet>((summe, e) => summe - e.Betrag_Cents);

                var betrag = offener_Betrag.Eval(0, sorted[test_id]);
                Console.Out.WriteLine(
                    "Offener Betrag für Rechnung {0}: {1} ({2})",
                    test_id,
                    betrag,
                    timer());
            }

            Console.Out.WriteLine("Press <enter>...");
            Console.ReadLine();

            {
                var lookups = 200000;
                Console.Out.WriteLine("");
                Console.Out.WriteLine("Lookup with index: {0} lookups:", lookups);

                timer = Timer();

                var offener_Betrag = new Projection<int>()
                    .Fuer<Rechnung_wurde_erstellt>((summe, e) => summe + e.Betrag_Cents)
                    .Fuer<Zahlung_wurde_geleistet>((summe, e) => summe - e.Betrag_Cents);

                var sum = 0;
                for (int i = 0; i < lookups; i++)
                {
                    var test_id = random.Next(anzahl_demo_Rechnungen);

                    var betrag = offener_Betrag.Eval(0, sorted[test_id]);
                    sum += betrag;
                    //Console.Out.WriteLine("Offener Betrag für Rechnung {0}: {1}", test_id, betrag);
                }
                Console.Out.WriteLine("Sum: " + sum);

                Console.Out.WriteLine(timer());
                Console.Out.WriteLine("Press <enter>...");
                Console.ReadLine();
            }


            {

                Console.Out.WriteLine("");
                Console.Out.WriteLine("Live-Projektion");

                var offener_Betrag = new Projection<int>()
                    .Fuer<Rechnung_wurde_erstellt>((summe, e) => summe + e.Betrag_Cents)
                    .Fuer<Zahlung_wurde_geleistet>((summe, e) => summe - e.Betrag_Cents);

                var offener_Betrag_live
                    = new LiveProjection<int, int>(
                        0,
                        offener_Betrag,
                        _ => _);

                offener_Betrag_live.Next += betrag => Console.WriteLine(" Rechnung 4711 hat neuen offenen Betrag: " + betrag);


                offener_Betrag_live.Handle(new object[] {new Rechnung_wurde_erstellt(4711, 18000)});
                offener_Betrag_live.Handle(new object[] {new Zahlung_wurde_geleistet(4711, 14000)});
                offener_Betrag_live.Handle(new object[] {new Zahlung_wurde_geleistet(4711, 3999)});
                Console.Out.WriteLine("Press <enter>...");
                Console.ReadLine();
            }

        }



        private static Dictionary<int, List<object>> Create_index(List<object> historie, Projection<int> sorter)
        {
            var result = new Dictionary<int, List<object>>();
            foreach (var e in historie)
            {
                var key = sorter.Eval(0, e);
                if (!result.ContainsKey(key)) result.Add(key, new List<object>());
                result[key].Add(e);

            }
            return result;
        }




        private static int Offener_Betrag(List<object> historie, int testId)
        {
            var result = 0;
            foreach (var e in historie)
            {
                if (e is Rechnung_wurde_erstellt)
                {
                    var ev = (Rechnung_wurde_erstellt)e;
                    if (ev.Id == testId) result += ev.Betrag_Cents;
                }

                if (e is Zahlung_wurde_geleistet)
                {
                    var ev = (Zahlung_wurde_geleistet)e;
                    if (ev.Id == testId) result -= ev.Betrag_Cents;
                }
            }
            return result;
        }





        static Func<string> Timer()
        {
            var start = Environment.TickCount;
            return () => String.Format("Dauer: {0} ms", Environment.TickCount - start);
        }

    }
}
