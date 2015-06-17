namespace Sandbox_DWX_EventSourcing
{
    internal struct Zahlung_wurde_geleistet
    {
        public Zahlung_wurde_geleistet(int id, int betragCents)
        {
            Id = id;
            Betrag_Cents = betragCents;
        }

        public readonly int Id;
        public readonly int Betrag_Cents;
    }
}