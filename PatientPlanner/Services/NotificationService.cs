using PatientPlanner.Models;
using WebPush;

namespace PatientPlanner.Services;
public static class NotificationService
{
    private static readonly IConfiguration _configuration;

    private static List<Device> Devices = new List<Device>();
    static NotificationService()
    {
    }

    public static List<Device> GetAllDevices() => Devices;

    public static Device GetDevice(string PushP256DH) => Devices.FirstOrDefault(d => d.PushP256DH == PushP256DH);

    public static void Send(string P256DH, string payload, IConfiguration _configuration)
    {
        try
        {
            // var payload = Request.Form["payload"];
            Device device = GetDevice(P256DH);

            string vapidPublicKey = _configuration["VapidKeys:PublicKey"];
            string vapidPrivateKey = _configuration["VapidKeys:PrivateKey"];

            var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
            var vapidDetails = new VapidDetails("", vapidPublicKey, vapidPrivateKey);

            var webPushClient = new WebPushClient();
            webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
        }
        catch (Exception exception)
        {

        }
    }
}