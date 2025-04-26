using System.Collections.Generic;

public class PaymentStatusTracker
{
    private static readonly Dictionary<string, bool> _paymentStatuses = new Dictionary<string, bool>();

    public static bool IsAnyFormInPaymentMode(string ticketName)
    {
        return _paymentStatuses.ContainsKey(ticketName) && _paymentStatuses[ticketName];
    }

    public static void SetPaymentMode(string ticketName, bool isInPaymentMode)
    {
        _paymentStatuses[ticketName] = isInPaymentMode;
    }
}
