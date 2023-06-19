// https://www.codeproject.com/Tips/1175658/Session-Expiration-Popup

var sessServerAliveTime = 15 * 60 * 1000; // 15 minutes
var sessionTimeout = 5 * 60 * 1000; // 20 minutes
// var sessServerAliveTime = 500 * 60; // 1 minute
// var sessionTimeout = 500 * 60; // 1 minute
var sessLastActivity;
var idleTimer, remainingTimer;
var isTimout = false;

var sess_intervalID, idleIntervalID;
var sess_lastActivity;
var timer;
var isIdleTimerOn = false;
localStorage.setItem('sessionSlide', 'isStarted');

// After 20 minutes the session is expired, make a call to the server to check if the session is still alive

function initSessionMonitor() {
    startIdleTime();
}

function startIdleTime() {
    console.log('startIdleTime');
    stopIdleTime();
    localStorage.setItem("sessIdleTimeCounter", $.now());
    idleIntervalID = setInterval('checkIdleTimeout()', 1000);
    isIdleTimerOn = false;
}

// sessionExpired.addEventListener("click", sessionExpiredClicked, false);
function stopIdleTime() {
    clearInterval(idleIntervalID);
    clearInterval(remainingTimer);
}

function checkIdleTimeout() {
     // $('#sessionValue').val() * 60000;
    var idleTime = (parseInt(localStorage.getItem('sessIdleTimeCounter')) + (sessServerAliveTime));
    console.log('check idle timeout');
    // if the timeout is reached, then the session is expired and the user will be logged out.
    // TODO: change this to non interuptive behavior
    if ($.now() > idleTime + sessionTimeout) {
        $("#session-warning").hide();
        $("#session-timeout").show();
        clearInterval(sess_intervalID);
        clearInterval(idleIntervalID);

        isTimout = true;
    }
    else if ($.now() > idleTime) {
        if (!isIdleTimerOn) {
            console.log('send warning');
            $("#session-warning").show();
            $("#session-timeout").hide();
            isIdleTimerOn = true;
        }
    }
}

function refreshPage() {
    location.reload();
}