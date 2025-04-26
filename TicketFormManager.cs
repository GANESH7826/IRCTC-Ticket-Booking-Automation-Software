using IRCTC_APP;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

public static class TicketFormManager
{
    // Dictionary to keep track of forms by TicketName
    private static Dictionary<string, List<slotpair>> ticketForms = new Dictionary<string, List<slotpair>>();

    // Register the form with a specific TicketName
    public static void RegisterForm(slotpair form, string TicketName)
    {
        if (!ticketForms.ContainsKey(TicketName))
            ticketForms[TicketName] = new List<slotpair>();

        ticketForms[TicketName].Add(form);
    }

    // Unregister form when closed
    public static void UnregisterForm(slotpair form, string TicketName)
    {
        if (ticketForms.ContainsKey(TicketName))
        {
            ticketForms[TicketName].Remove(form);
            if (ticketForms[TicketName].Count == 0)
                ticketForms.Remove(TicketName);
        }
    }

    // Update checkbox state for all forms with the same TicketName
    public static void UpdateCheckbox(string TicketName, bool isChecked)
    {
        if (ticketForms.ContainsKey(TicketName))
        {
            foreach (var form in ticketForms[TicketName])
            {
                form.UpdateCheckboxState(isChecked);
            }
        }
    }
}
