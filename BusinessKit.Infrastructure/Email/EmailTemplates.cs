using System.Net;

namespace BusinessKit.Infrastructure.Email;

public static class EmailTemplates
{
    public static (string Subject, string HtmlBody) AppointmentCreatedAdmin(
        int appointmentId,
        string customerFullName,
        string? customerEmail,
        string customerPhone,
        DateTime requestedDate,
        string requestedTime,
        string? staffMemberName,
        string? businessServiceTitle,
        string? note,
        string businessName)
    {
        var subject = $"New Appointment Request — {WebUtility.HtmlEncode(customerFullName)}";

        var emailRow = customerEmail is not null
            ? $"<tr><td><strong>Email:</strong></td><td>{WebUtility.HtmlEncode(customerEmail)}</td></tr>"
            : string.Empty;

        var staffRow = staffMemberName is not null
            ? $"<tr><td><strong>Staff:</strong></td><td>{WebUtility.HtmlEncode(staffMemberName)}</td></tr>"
            : string.Empty;

        var serviceRow = businessServiceTitle is not null
            ? $"<tr><td><strong>Service:</strong></td><td>{WebUtility.HtmlEncode(businessServiceTitle)}</td></tr>"
            : string.Empty;

        var noteRow = !string.IsNullOrWhiteSpace(note)
            ? $"<tr><td><strong>Note:</strong></td><td>{WebUtility.HtmlEncode(note)}</td></tr>"
            : string.Empty;

        var html = $"""
            <html><body style="font-family:sans-serif;color:#222;">
            <h2>New Appointment Request</h2>
            <p>A new appointment has been submitted on <strong>{WebUtility.HtmlEncode(businessName)}</strong>.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;">
              <tr><td><strong>ID:</strong></td><td>#{appointmentId}</td></tr>
              <tr><td><strong>Name:</strong></td><td>{WebUtility.HtmlEncode(customerFullName)}</td></tr>
              {emailRow}
              <tr><td><strong>Phone:</strong></td><td>{WebUtility.HtmlEncode(customerPhone)}</td></tr>
              <tr><td><strong>Date:</strong></td><td>{requestedDate:yyyy-MM-dd}</td></tr>
              <tr><td><strong>Time:</strong></td><td>{WebUtility.HtmlEncode(requestedTime)}</td></tr>
              {staffRow}
              {serviceRow}
              {noteRow}
            </table>
            <p style="color:#888;font-size:12px;margin-top:24px;">This is an automated notification from {WebUtility.HtmlEncode(businessName)}.</p>
            </body></html>
            """;

        return (subject, html);
    }

    public static (string Subject, string HtmlBody) AppointmentCreatedCustomer(
        string customerFullName,
        DateTime requestedDate,
        string requestedTime,
        string? staffMemberName,
        string? businessServiceTitle,
        string businessName)
    {
        var subject = $"Your Appointment Request Was Received — {WebUtility.HtmlEncode(businessName)}";

        var staffRow = staffMemberName is not null
            ? $"<tr><td><strong>Staff:</strong></td><td>{WebUtility.HtmlEncode(staffMemberName)}</td></tr>"
            : string.Empty;

        var serviceRow = businessServiceTitle is not null
            ? $"<tr><td><strong>Service:</strong></td><td>{WebUtility.HtmlEncode(businessServiceTitle)}</td></tr>"
            : string.Empty;

        var html = $"""
            <html><body style="font-family:sans-serif;color:#222;">
            <h2>Appointment Request Received</h2>
            <p>Hi <strong>{WebUtility.HtmlEncode(customerFullName)}</strong>,</p>
            <p>Thank you for your appointment request at <strong>{WebUtility.HtmlEncode(businessName)}</strong>. We will review it shortly and confirm your booking.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;">
              <tr><td><strong>Date:</strong></td><td>{requestedDate:yyyy-MM-dd}</td></tr>
              <tr><td><strong>Time:</strong></td><td>{WebUtility.HtmlEncode(requestedTime)}</td></tr>
              {staffRow}
              {serviceRow}
            </table>
            <p>If you have any questions, please contact us.</p>
            <p style="color:#888;font-size:12px;margin-top:24px;">{WebUtility.HtmlEncode(businessName)}</p>
            </body></html>
            """;

        return (subject, html);
    }

    public static (string Subject, string HtmlBody) AppointmentConfirmedCustomer(
        string customerFullName,
        DateTime requestedDate,
        string requestedTime,
        string? staffMemberName,
        string? businessServiceTitle,
        string businessName)
    {
        var subject = $"Your Appointment Is Confirmed — {WebUtility.HtmlEncode(businessName)}";

        var staffRow = staffMemberName is not null
            ? $"<tr><td><strong>Staff:</strong></td><td>{WebUtility.HtmlEncode(staffMemberName)}</td></tr>"
            : string.Empty;

        var serviceRow = businessServiceTitle is not null
            ? $"<tr><td><strong>Service:</strong></td><td>{WebUtility.HtmlEncode(businessServiceTitle)}</td></tr>"
            : string.Empty;

        var html = $"""
            <html><body style="font-family:sans-serif;color:#222;">
            <h2>Appointment Confirmed</h2>
            <p>Hi <strong>{WebUtility.HtmlEncode(customerFullName)}</strong>,</p>
            <p>Your appointment at <strong>{WebUtility.HtmlEncode(businessName)}</strong> has been <strong>confirmed</strong>. We look forward to seeing you.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;">
              <tr><td><strong>Date:</strong></td><td>{requestedDate:yyyy-MM-dd}</td></tr>
              <tr><td><strong>Time:</strong></td><td>{WebUtility.HtmlEncode(requestedTime)}</td></tr>
              {staffRow}
              {serviceRow}
            </table>
            <p>If you need to make changes, please contact us as soon as possible.</p>
            <p style="color:#888;font-size:12px;margin-top:24px;">{WebUtility.HtmlEncode(businessName)}</p>
            </body></html>
            """;

        return (subject, html);
    }

    public static (string Subject, string HtmlBody) AppointmentCancelledCustomer(
        string customerFullName,
        DateTime requestedDate,
        string requestedTime,
        string businessName)
    {
        var subject = $"Your Appointment Has Been Cancelled — {WebUtility.HtmlEncode(businessName)}";

        var html = $"""
            <html><body style="font-family:sans-serif;color:#222;">
            <h2>Appointment Cancelled</h2>
            <p>Hi <strong>{WebUtility.HtmlEncode(customerFullName)}</strong>,</p>
            <p>We're sorry to inform you that your appointment at <strong>{WebUtility.HtmlEncode(businessName)}</strong> has been <strong>cancelled</strong>.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;">
              <tr><td><strong>Date:</strong></td><td>{requestedDate:yyyy-MM-dd}</td></tr>
              <tr><td><strong>Time:</strong></td><td>{WebUtility.HtmlEncode(requestedTime)}</td></tr>
            </table>
            <p>Please contact us if you would like to reschedule.</p>
            <p style="color:#888;font-size:12px;margin-top:24px;">{WebUtility.HtmlEncode(businessName)}</p>
            </body></html>
            """;

        return (subject, html);
    }

    public static (string Subject, string HtmlBody) ContactMessageAdmin(
        string fullName,
        string email,
        string? phone,
        string? subject,
        string message,
        string businessName)
    {
        var emailSubject = $"New Contact Message — {WebUtility.HtmlEncode(fullName)}";

        var phoneRow = !string.IsNullOrWhiteSpace(phone)
            ? $"<tr><td><strong>Phone:</strong></td><td>{WebUtility.HtmlEncode(phone)}</td></tr>"
            : string.Empty;

        var subjectRow = !string.IsNullOrWhiteSpace(subject)
            ? $"<tr><td><strong>Subject:</strong></td><td>{WebUtility.HtmlEncode(subject)}</td></tr>"
            : string.Empty;

        var html = $"""
            <html><body style="font-family:sans-serif;color:#222;">
            <h2>New Contact Message</h2>
            <p>A new contact message was submitted on <strong>{WebUtility.HtmlEncode(businessName)}</strong>.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;">
              <tr><td><strong>Name:</strong></td><td>{WebUtility.HtmlEncode(fullName)}</td></tr>
              <tr><td><strong>Email:</strong></td><td>{WebUtility.HtmlEncode(email)}</td></tr>
              {phoneRow}
              {subjectRow}
            </table>
            <h3>Message:</h3>
            <p style="background:#f5f5f5;padding:12px;border-radius:4px;">{WebUtility.HtmlEncode(message)}</p>
            <p style="color:#888;font-size:12px;margin-top:24px;">This is an automated notification from {WebUtility.HtmlEncode(businessName)}.</p>
            </body></html>
            """;

        return (emailSubject, html);
    }

    public static (string Subject, string HtmlBody) PaymentConfirmedCustomer(
        string customerFullName,
        decimal amount,
        string currency,
        int appointmentId,
        DateTime requestedDate,
        string requestedTime,
        string? businessServiceTitle,
        string businessName)
    {
        var subject = $"Payment Confirmed — {WebUtility.HtmlEncode(businessName)}";

        var serviceRow = businessServiceTitle is not null
            ? $"<tr><td><strong>Service:</strong></td><td>{WebUtility.HtmlEncode(businessServiceTitle)}</td></tr>"
            : string.Empty;

        var html = $"""
            <html><body style="font-family:sans-serif;color:#222;">
            <h2>Payment Confirmed</h2>
            <p>Hi <strong>{WebUtility.HtmlEncode(customerFullName)}</strong>,</p>
            <p>Your payment has been confirmed for your appointment at <strong>{WebUtility.HtmlEncode(businessName)}</strong>.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;">
              <tr><td><strong>Appointment:</strong></td><td>#{appointmentId}</td></tr>
              <tr><td><strong>Date:</strong></td><td>{requestedDate:yyyy-MM-dd}</td></tr>
              <tr><td><strong>Time:</strong></td><td>{WebUtility.HtmlEncode(requestedTime)}</td></tr>
              {serviceRow}
              <tr><td><strong>Amount:</strong></td><td>{WebUtility.HtmlEncode(currency)} {amount:0.00}</td></tr>
            </table>
            <p>Thank you for your payment. We look forward to seeing you.</p>
            <p style="color:#888;font-size:12px;margin-top:24px;">{WebUtility.HtmlEncode(businessName)}</p>
            </body></html>
            """;

        return (subject, html);
    }
}
