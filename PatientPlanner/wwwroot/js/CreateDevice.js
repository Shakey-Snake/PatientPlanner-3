function subscribe() {
    navigator.serviceWorker.ready.then(function (reg) {
        var subscribeParams = { userVisibleOnly: true };

        //Setting the public key of our VAPID key pair.
        var applicationServerKey = urlB64ToUint8Array(applicationServerPublicKey);
        subscribeParams.applicationServerKey = applicationServerKey;

        reg.pushManager.subscribe(subscribeParams)
            .then(function (subscription) {
                isSubscribed = true;

                var p256dh = base64Encode(subscription.getKey('p256dh'));
                var auth = base64Encode(subscription.getKey('auth'));

                console.log(getCookie(subscription));

                //check if cookie exists
                if (getCookie(subscription) != subscription.endpoint){
                    //check if the user is subscribed in the system
                    var isSubscribedTest = false;
                    $.ajax({
                        type: "POST",
                        // Should it go to the index? Or is there a better way?
                        url: "/Index?handler=CheckSub",
                        data: {
                            PushEndpoint: subscription.endpoint,
                            PushP256DH: p256dh,
                            PushAuth: auth
                        },
                        headers: {
                            RequestVerificationToken: 
                                $('input:hidden[name="__RequestVerificationToken"]').val()
                        },
                        success: function(data) {
                            if (data == "false"){
                                console.log("reload");
                                location.reload();
                            }
                            console.log(data);
                        },
                        error: function() {
                            alert('Error occured');
                        }
                    });
                }
                
                //Create a cookie with the string sent back
                //cookie will be used for all other data on the page



                //if the cookie does not exist (page shutdown) check if p256dh is in devices
                //if it is, find the string and send it back
                //else idk, do something

                //Subscribe a user via POST
                console.log(subscription);

                $('#PushEndpoint').val(subscription.endpoint);
                $('.PushP256DH').val(p256dh);
                $('#PushAuth').val(auth);
            })
            .catch(function (e) {
                errorHandler('[subscribe] Unable to subscribe to push', e);
            });
    });
}

function enableBrowserNotifications() {
    if (typeof applicationServerPublicKey === 'undefined') {
        errorHandler('Vapid public key is undefined.');
        return;
    }

    Notification.requestPermission().then(function (status) {
        if (status === 'denied') {
            $('#notifications-denied').show();
            $('#browser-notification-reminder').hide();
            errorHandler('[Notification.requestPermission] Browser denied permissions to notification api.');
        } else if (status === 'granted') {
            $('#browser-notification-reminder').hide();
            console.log('[Notification.requestPermission] Initializing service worker.');
            initialiseServiceWorker();
            subscribe();
        }
    });

    
}

function enableSiteNotifications() {
    subscribe();
}

function errorHandler(message, e) {
    if (typeof e == 'undefined') {
        e = null;
    }

    console.error(message, e);
    $("#errorMessage").append('<li>' + message + '</li>').parent().show();
}

function urlB64ToUint8Array(base64String) {
    var padding = '='.repeat((4 - base64String.length % 4) % 4);
    var base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    var rawData = window.atob(base64);
    var outputArray = new Uint8Array(rawData.length);

    for (var i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

function base64Encode(arrayBuffer) {
    return btoa(String.fromCharCode.apply(null, new Uint8Array(arrayBuffer)));
}

function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for(let i = 0; i <ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}