//hide every checkbox and unhide the delete buttons
function switchMode(mode){
    if (mode == 'delete'){
        $('.checkbox').hide();
        $('.deletebutton').show();
        $('#toggleCheckbox').show();
        $('#toggleDelete').hide();
   }
   else if(mode == 'checkbox'){
        console.log(mode);
        $('.checkbox').show();
        $('.deletebutton').hide();
        $('#toggleCheckbox').hide();
        $('#toggleDelete').show();
   }
}

function deleteTask(element){
    let _time = element.getAttribute('time');
    let _patient = element.getAttribute('patient');
    let _task = element.getAttribute('task');
    console.log(_time + " " + _patient + " " + _task + " ");
    $.ajax({
           type: "POST",
           url: "/Patient?handler=DeletePatientTask",
           data: {
                time: _time,
                patient: _patient,
                task: _task
           },
           headers: {
            RequestVerificationToken: 
                $('input:hidden[name="__RequestVerificationToken"]').val()
           },
           success: function(data) {
               console.log("Deleted");
           },
           error: function() {
               alert('Error occured');
           }
       });
}

function updateTask(element){
    let _time = element.getAttribute('time');
    let _patient = element.getAttribute('patient');
    let _task = element.getAttribute('task');
    let _checkboxValue = element.getAttribute('checked');
    // let checkboxValue = $("input[name=patient+task+time]:checked");
   console.log(_time + " " + _patient + " " + _task + " " + _checkboxValue + " ");
   $.ajax({
           type: "POST",
           url: "/Patient?handler=UpdateTask",
           data: {
               time: _time,
               patient: _patient,
               task: _task,
               checkboxValue : _checkboxValue
           },
           headers: {
           RequestVerificationToken: 
               $('input:hidden[name="__RequestVerificationToken"]').val()
           },
           success: function(data) {
           },
           error: function() {
               alert('Error occured');
           }
       });
}