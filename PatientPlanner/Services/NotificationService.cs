using PatientPlanner.Models;
using FirebaseAdmin;
using PatientPlanner.Data;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Messaging;

namespace PatientPlanner.Services;
public static class NotificationService
{
    private static readonly IConfiguration _configuration;
    static NotificationService()
    {
    }
    public static void Send(Device device, Dictionary<string, string> _Data)
    {
        try
        {
            Console.WriteLine(FirebaseMessaging.DefaultInstance.ToString());
            // This registration token comes from the client FCM SDKs.
            var registrationToken = device.Token;

            // See documentation on defining a message payload.
            var message = new Message()
            {
                Data = _Data,
                Token = registrationToken,
            };

            FirebaseMessaging.DefaultInstance.SendAsync(message);

        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}