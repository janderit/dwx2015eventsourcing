namespace Sandbox_DWX_EventSourcing
{
    internal struct Rechnung_wurde_erstellt
    {
        public Rechnung_wurde_erstellt(int id, int betragCents)
        {
            Id = id;
            Betrag_Cents = betragCents;
        }

        public readonly int Id;
        public readonly int Betrag_Cents;
    }
}