using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public interface ISmsSender
{
    Task SendSmsAsync(string phoneNumber, string message);
}
public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(ILogger<SmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        // TODO: Integrate with a real SMS provider (e.g., Twilio, Nexmo)
        _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}