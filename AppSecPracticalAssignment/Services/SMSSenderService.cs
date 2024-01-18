using AppSecPracticalAssignment.Settings;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace AppSecPracticalAssignment.Services
{
    public class SMSSenderService : ISMSSenderService
    {
        private readonly TwilioSettings _twilioSettings;

        public SMSSenderService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
        }

        public async Task SendSMSAsync(string toPhone, string message)
        {
            TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);

            await MessageResource.CreateAsync(
                to: toPhone,
                from: _twilioSettings.FromPhone,
                body: message
            );
        }
    }
}
