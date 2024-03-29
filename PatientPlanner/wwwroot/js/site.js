﻿// https://www.codeproject.com/Tips/1175658/Session-Expiration-Popup

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

function timeLineTimer(){
    updateLine = setInterval('updateTimeLine()', 1000);
}

function taskReminderTimer(){
    updateTask = setInterval('taskReminder()', 1000);
}

function taskReminder(){
    // NOTE: I dislike the number of loops but this is how the table is structured
    var tableRows = $('.tt-row');
    let currentTime = new Date().toString().slice(16, 21);
    // let currentTime = "01:00:00";
    // console.log(currentTime);
    // console.log(tableRows);
    for (let i = 0; i < tableRows.length; i++) {
        // greater than 1 means patients exist
        //console.log(tableRows[i]);
        if ($(tableRows[i]).children(".heading").length > 1){
            continue;
        }
        if ($(tableRows[i]).children("div").length > 1) {
            let time = $($(tableRows[i]).children()[0]).attr('id');
            //console.log(time);
            let minuteDiff = (currentTime.split(":")[0] * 60 - time.split(":")[0] * 60) + (currentTime.split(":")[1] - time.split(":")[1]);
            // NIGHTSHIFT
            // console.log(minuteDiff);
            if (minuteDiff < 0) {
                minuteDiff = 1440 + minuteDiff;
                // console.log("nighttime " + minuteDiff);
                // NOTE: this could break the nighttime calculation
                if (minuteDiff < 1320) {
                    break;
                }
            }
            // TODO: set each task's background to default
            if (minuteDiff > 120 && $(tableRows[i]).attr('marked') == "True") {
                let patients = $(tableRows[i]).children("div");
                for (let j = 1; j < patients.length; j++) {
                    console.log($(patients[j]).children());
                    // if there are tasks for the patient then get the time from the first td
                    let tasks = $(patients[j]).children();
                    //console.log(tasks);
                    if (tasks.length > 0) {
                        for (let k = 0; k < tasks.length; k++) {
                            $(tasks[k]).css('background-color', 'rgb(255, 255, 255)');
                        }
                    }
                }
                $(tableRows[i]).attr('marked', "False");
                continue;
            }
            // DAYTIME: if the current time is less than the cell time  
            // if the current time is greater than
            if (minuteDiff <= 120){
                let patients = $(tableRows[i]).children("div");
                // console.log(patients);
                for (let j = 1; j < patients.length; j++) {
                    // console.log($(patients[j]).children());
                    // if there are tasks for the patient then get the time from the first td
                    let tasks = $(patients[j]).children();
                    //console.log(tasks);
                    if (tasks.length > 0) {
                        for (let k = 0; k < tasks.length; k++) {
                            // check if the task is checked off
                            let taskid = $(tasks[k]).attr("taskid");
                            let checked = $(tasks[k]).attr("completed");
                            // console.log(tasks[k]);
                            if (checked == "False") {
                                // console.log(tasks[k]);
                                $(tasks[k]).css('background-color', 'rgb(227, 66, 52, 0.75)');
                            }
                            // console.log(tasks[k]);
                        }
                        $(tableRows[i]).attr('marked', "True");
                    }
                }
            }
            
            
            
        }
    }
}

// creates a line that will show what the time is now
// NOTE: this function can be improved by using localStorage
function updateTimeLine(){
    // get the current time
    let currentTime = new Date().toString().slice(16, 25);
    // get the current interval by getting the id of the top two tr and finding the minute diff
    let tableRows = $('.tt-row');
    //console.log(tableRows);
    let tr1 = $($(tableRows[1]).children()[0]).attr('id');
    //console.log(tr1);
    let tr2 = $($(tableRows[2]).children()[0]).attr('id');
    //console.log(tr2);
    //could be an issue if tr1 is larger than tr2
    let interval = (tr2.split(":")[0]*60 - tr1.split(":")[0]*60) + (tr2.split(":")[1] - tr1.split(":")[1]);
    if (interval < 0){
        console.log("Not supported yet");
    }
    
    // might need 2 for loops, 1 to check the hour and 1 to check the minute
    if (interval > 60){
        console.log(interval)
    }
    else {
        let tableRow = null;
        //find the table row that has the matching hour
    loopHour:
        for (let i = 1; i < tableRows.length; i++){
            //console.log($($(tableRows[i]).children()[0]).attr('id'));
            if ($($(tableRows[i]).children()[0]).attr('id').split(":")[0] == currentTime.split(":")[0]){
                let rounded = Math.floor(currentTime.split(":")[1] / interval) * interval;
                // console.log(rounded);
                let currRow = $(tableRows[i]);
                //console.log(currRow);
                for (let j = 0; j < 60/interval; j += interval){
                    if($(currRow.children("div")[0]).attr("id").split(":")[1] == rounded){
                        // console.log("THIS " + rounded + " " + currRow.attr("id").split(":")[1]);
                        tableRow = currRow;
                        break loopHour;
                    }
                    currRow = currRow.next();
                }
            }
        }
        if (tableRow == null){
            return;
        }
        // set the blue line
        //console.log($(tableRow.children("td")[0]));
        let currentMinutes = currentTime.split(":")[1];
        let currentSeconds = currentTime.split(":")[2];

        let width = $(tableRow).outerWidth() - $(tableRow.children("div")[0]).outerWidth();
        let height = $(tableRow.children("div")[0]).outerHeight();

        // TODO: adjust by seconds also
        let heightAjustSeconds = (height / interval) * currentSeconds / 60;
        // console.log(heightAjustSeconds);
        let heightAdjust = (height / interval) * (currentMinutes - (Math.floor(currentMinutes / interval) * interval)) + heightAjustSeconds;

        let _top = $(tableRow.children("div")[0]).offset().top + heightAdjust;
        let _left = $(tableRow.children("div")[0]).offset().left + $(tableRow.children("div")[0]).outerWidth();
        //console.log(_top);
        $("#time-bar").remove();
        jQuery('<div>', {
            id: "time-bar",
            style: 'width: ' + width + 'px; height: ' + height + 'px; border-top: 3px solid; position: absolute; border-top-color: rgb(227, 66, 52, 0.9)'
        }).appendTo('body').offset({ top: _top, left: _left });
        // console.log(tableRow);
    }
}

function checkIdleTimeout() {
     // $('#sessionValue').val() * 60000;
    let idleTime = (parseInt(localStorage.getItem('sessIdleTimeCounter')) + (sessServerAliveTime));
    // console.log('check idle timeout');
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

function checkTasks() {
    // ajax the task list and the settings
    const startTime = $("#start-time").attr("value");
    const endTime = $("#end-time").attr("value"); 
    const interval = $("#interval").attr("value");
    console.log(startTime, endTime, interval);
    // check if its a night shift
    let tasks = [];
    $.ajax({
        type: "GET",
        // Should it go to the index? Or is there a better way?
        url: "/Index?handler=Tasks",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (data) {
            // NOTE: COULD BE BROKEN DOWN INTO SEPARATE FUNCTIONS
            if (data != "false") {
                showGhostTaskNotification();
                // NOTE: Could have cross compatibility difficulties if the order changes
                let patients = data.patients;
                tasks = data.taskList;
                //console.log(data);
                //console.log(startTime);
                let row = $(".tt-rows").children()[0];
                if (startTime < endTime) {
                    console.log(tasks);
                    console.log('check tasks');
                    for (let i = 0; i < tasks.length; i++) {
                        //console.log(tasks[i].dueTime);
                        if (tasks[i].dueTime < startTime){
                            //check if a class has already been created
                            //console.log(row);
                            //console.log($("#06:00:00"));
                            if ($(row).hasClass("ghost") && $($(row).children()[0]).attr("id") == tasks[i].dueTime ){
                                console.log("check");
                                ghostRow(false, tasks[i], patients, row);
                            }
                            else {
                                let newRow = ghostRow(true, tasks[i], patients);
                                if ($(row).hasClass("ghost")){
                                    $(row).after(newRow);
                                }
                                else {
                                    $(row).before(newRow);
                                }
                                row = newRow;
                            }
                            // create the row using a custom function
                        }
                        else if (tasks[i].dueTime > endTime){
                            row = $(".tt-rows").children().last();
                            // console.log(row);
                            if ($(row).hasClass("ghost") && $($(row).children()[0]).attr("id") == tasks[i].dueTime ){
                                //console.log("check");
                                ghostRow(false, tasks[i], patients, row);
                            }
                            else {
                                //create the row using a custom function
                                let newRow = ghostRow(true, tasks[i], patients);
                                $(row).after(newRow);
                            }
                        }
                        else if (tasks[i].dueTime > startTime && tasks[i].dueTime < endTime){
                            let tableBodyRows = $(".tt-rows").children();
                            //check for intervals
                            let minutes = tasks[i].dueTime.split(":")[1];
                            if (minutes > 0){
                                let modulo = 0;
                                if (minutes > interval){
                                    modulo = minutes / interval;
                                }
                                else {
                                    modulo = interval / minutes;
                                }
                                
                                if (modulo % 1 != 0){
                                    console.log(modulo);
                                    console.log("DUE TIME: " + tasks[i].dueTime);
                                    //set the row to the time closest to the current time by finding the
                                    for (let j = 0; j < tableBodyRows.length; j++){
                                        let id = $($(tableBodyRows[j]).children()[0]).attr("id");
                                        console.log("ID: " + id);
                                        console.log("DUE TIME: " + tasks[i].dueTime);
                                        if (id.split(":")[0] == tasks[i].dueTime.split(":")[0] && id.split(":")[1] == "00"){
                                            let row = $(tableBodyRows[j]);
                                            while ($($(row).children()[0]).attr("id") < tasks[i].dueTime){
                                                console.log("ROW");
                                                console.log($(row).children()[0]);
                                                row = row.next();
                                            }
                                            console.log("FINAL ROW:");
                                            
                                            console.log($(row).children()[0]);
                                            //checks if the row already exists
                                            if ($($(row).children()[0]).attr("id") == tasks[i].dueTime){
                                                ghostRow(false, tasks[i], patients, row);
                                            }
                                            else {
                                                // insert the new ghost row above the final row
                                                let newRow = ghostRow(true, tasks[i], patients);
                                                $(row).before(newRow);
                                            }
                                            break;
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }
                }
                // TODO: Compatibility for night time
                else {

                }
            }
            
        },
        error: function () {
            alert('Error occured');
        }
    });
}

function ghostRow(newRow, task, patients, row) {
    if (newRow == true) {
        //console.log(task);
        let divRow = $("<div>", {"class": "ghost tt-row", style: "opacity: 0.5; background-color: grey"});
        let divTime = $("<div>", {id: task.dueTime, "class": "time entry"});
        divTime.appendTo(divRow);
        let time = $("<time>").text(task.dueTime);
        time.appendTo(divTime);
        for (let i = 0;  i < patients.length; i++){
            let divCell = $("<div>", {id: patients[i].roomNumber, "class": "entry"});
            divCell.appendTo(divRow);
            //console.log(patients[i])
            if (task.patientID == patients[i].patientID){
                // NOTE: Need to capitalize the first letter for red box to work
                let completed = task.completed.toString();
                let completedString = completed.charAt(0).toUpperCase() + completed.slice(1);
                let divDetails = $("<div>", {"class": "details", completed: completedString, style: "border-left-color:" + task.taskColour});
                divDetails.appendTo(divCell);
                let pItem = $("<p>").text(" " + task.taskName);
                pItem.appendTo(divDetails);
            }
        }
        return divRow;
    }
    else {
        let completed = task.completed.toString();
        let completedString = completed.charAt(0).toUpperCase() + completed.slice(1);
        let patient = patients.find(p => p.patientID === task.patientID)
        let divCell = row.children("#"+patient.roomNumber+"");
        let divDetails = $("<div>", {"class": "details", completed: completedString, style: "border-left-color:" + task.taskColour});
        divDetails.appendTo(divCell);
        let pItem = $("<p>").text(" " + task.taskName);
        pItem.appendTo(divDetails);
    }
}

function showGhostTaskNotification(){
    $("#ghost-task").show();
}