// Import the functions you need from the SDKs you need
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries
import firebase from 'firebase/compat/app';
// const messaging = require('firebase/compat/messaging');

// Your web app's Firebase configuration
const firebaseConfig = {
  apiKey: "AIzaSyDWIxMplVoH-3GGQ7YQWU76DIUhikVCtjo",
  authDomain: "patientplanner-1d025.firebaseapp.com",
  projectId: "patientplanner-1d025",
  storageBucket: "patientplanner-1d025.appspot.com",
  messagingSenderId: "162531664373",
  appId: "1:162531664373:web:ddbe00353d9be2396c80bc"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);

// Initialize Firebase Cloud Messaging and get a reference to the service
const messaging = firebase.messaging();

var applicationServerPublicKey = 'BKeF6SQHFZiUGYx2EO7VrkVYACUeYoWD3dkazCfXSS0cIfwRQqiCHXs5F-CilG7wY9JG2sjjFtTdWJmJ5-b2Pt8';
var serviceWorker = '/sw.js';
var isSubscribed = false;

window.onload = (event) =>{
    // Application Server Public Key defined in Views/Device/Create.cshtml
    if (typeof applicationServerPublicKey === 'undefined') {
        errorHandler('Vapid public key is undefined.');
        return;
    }

    if (Notification.permission == "default"){
        $('#browser-notification-reminder').show();
        errorHandler('[Notification.requestPermission] Browser denied permissions to notification api.');
    }
    else if (Notification.permission == "denied"){
        $('#browser-notification-reminder').show();
        errorHandler('[Notification.requestPermission] Browser denied permissions to notification api.');
    }
    else if (Notification.permission == "granted") {
        // Add the public key generated from the console here.
        getToken(messaging, {vapidKey: "BMjn0sGVG_L4BzjK9TdSlpXQGEv92Ccr6SQdsA-fDbNieO-hQb-hqdaVa6wvIdQOAcF2rWPNXeQDNOuNUhapx10" }).then((currentToken) => {
            if (currentToken) {
            // Send the token to your server and update the UI if necessary
            console.log('[Notification.requestPermission] Initializing service worker.');
            initialiseServiceWorker();
            subscribe();
            }
            else {
            // Show permission request UI
            console.log('No registration token available. Request permission to generate one.');
            }
        }).catch((err) => {
            console.log('An error occurred while retrieving token. ', err);
        });
    }
};

function initialiseServiceWorker() {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register(serviceWorker).then(handleSWRegistration);
    } else {
        errorHandler('[initialiseServiceWorker] Service workers are not supported in this browser.');
    }
};

function handleSWRegistration(reg) {
    if (reg.installing) {
        console.log('Service worker installing');
    } else if (reg.waiting) {
        console.log('Service worker installed');
    } else if (reg.active) {
        console.log('Service worker active');
    }

    initialiseState(reg);
}

// Once the service worker is registered set the initial state
function initialiseState(reg) {
    // Are Notifications supported in the service worker?
    if (!(reg.showNotification)) {
        $('.no-support').show();
        errorHandler('[initialiseState] Notifications aren\'t supported on service workers.');
        return;
    }

    // Check if push messaging is supported
    if (!('PushManager' in window)) {
        $('.no-support').show();
        errorHandler('[initialiseState] Push messaging isn\'t supported.');
        return;
    }

    // We need the service worker registration to check for a subscription
    navigator.serviceWorker.ready.then(function (reg) {
        // Do we already have a push message subscription?
        reg.pushManager.getSubscription()
            .then(function (subscription) {
                isSubscribed = subscription;
                if (isSubscribed) {
                    console.log('User is already subscribed to push notifications');
                } else {
                    console.log('User is not yet subscribed to push notifications');
                }
            })
            .catch(function (err) {
                console.log('[req.pushManager.getSubscription] Unable to get subscription details.', err);
            });
    });
}

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