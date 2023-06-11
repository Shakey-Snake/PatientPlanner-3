using PatientPlanner.Models;
using WebPush;
using PatientPlanner.Data;

namespace PatientPlanner.Services;
public static class NotificationService
{
    private static readonly IConfiguration _configuration;
    static NotificationService()
    {
    }
    public static void Send(Device device, string payload, IConfiguration _configuration)
    {
        try
        {
            string vapidPublicKey = _configuration["VapidKeys:PublicKey"];
            string vapidPrivateKey = _configuration["VapidKeys:PrivateKey"];

            var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
            var vapidDetails = new VapidDetails("mailto:example@example.com", vapidPublicKey, vapidPrivateKey);

            var webPushClient = new WebPushClient();
            webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
        }
        catch (Exception exception)
        {

        }
    }
}