namespace Domain.Enums
{
    public enum ResponderStatus
    {
        Available = 1,     // ✅ Ready for assignment
        OnDuty,         // 🚨 Actively responding to an incident
        OffDuty,        // ❌ Not working (rest time or end of shift)
        Busy,            // ⚠️ Temporarily occupied (e.g., in a meeting)
        Unreachable      // ❌ Can't be contacted (e.g., no signal, emergency blackout)
    }
}
