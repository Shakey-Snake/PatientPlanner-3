window.onload = (event) =>{
    // if cookie exists, show all otherwise it will show the warning message
    if (!findCookie("subscription")){
        console.log("dontshow");
        $('#Warning').show();
        $('.MainPage').hide();
    }
};

function findCookie(cname){
    let name = cname;
    let decodedCookie = decodeURIComponent(document.cookie);
    let splitCookies = decodedCookie.split(';');
    // console.log(decodedCookie);
    for(let i = 0; i <splitCookies.length; i++) {
        let cookie = splitCookies[i].trim();
        let count = 0;
        // console.log(cookie);
        while (cookie.substring(0, count) != name) {
            // console.log(cookie.substring(0, count));
            count++;
            if (count >= cookie.length || cookie.substring(count - 1, count) == "="){
                break;
            }
        }
        if (cookie.substring(0, count) == name) {
            return true;
        }
    }
    return false;
}