using System.Globalization;
using System.Net;

namespace NotificationService.Application.UsesCases;

// Small HTML email templates. Every value that comes from users is HTML-encoded.
public static class EmailTemplates
{
    private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");

    public static string When(DateTime date, TimeOnly time) =>
        $"{date.ToString("dddd, MMMM d, yyyy", En)} at {time.ToString("h:mm tt", En)}";

    public static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    public static string Layout(string title, string greetingName, string bodyHtml) => $@"
<div style=""font-family:Arial,Helvetica,sans-serif;max-width:560px;margin:0 auto;color:#1f2937"">
  <h2 style=""color:#0f766e"">{Encode(title)}</h2>
  <p>Hello{(string.IsNullOrWhiteSpace(greetingName) ? "" : " " + Encode(greetingName))},</p>
  {bodyHtml}
  <p style=""color:#6b7280;font-size:12px;margin-top:32px"">
    This is an automatic message from Medical Appointment. Please do not reply to this email.
  </p>
</div>";

    public static string Detail(string label, string value) =>
        $@"<p style=""margin:4px 0""><strong>{Encode(label)}:</strong> {Encode(value)}</p>";
}
