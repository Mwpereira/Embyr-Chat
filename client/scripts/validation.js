const serverIP = 'http://99.227.48.179:8000';

let form = document.getElementsByTagName('form')[0];

// Events
form.addEventListener('submit', function (e) {
    e.preventDefault();
    if (form.id == "login-form") login();
    else if (form.id == "signup-form") signup();
});


// Validation
function login() {
    let username = form.children[0].value;
    let password = form.children[1].value;

    if (username == '') {
        $('input:first-child').attr("placeholder", "Cannot Be Empty");
    }
    if (password == '') {
        $('input:nth-child(2)').attr("placeholder", "Cannot Be Empty");
    }
    if (username != '' && password != '') {
        $.getJSON("http://jsonip.com?callback=?", function (res) {
            let info = `username=${username}&password=${password}&ip=${res.ip}`;

            $.ajax({
                type: 'POST',
                url: serverIP + '/login',
                data: info,
                success: function (data) {
                    sessionStorage.setItem("clientInfo", JSON.stringify(data));
                    data.friendsList.forEach(friend => {
                        sessionStorage.setItem(friend, `{"message": [], "sender": [], "time": [], "ghostMode": false}`);
                    })
                    window.location.href = './chatx.html';
                },
                statusCode: {
                    403: function () {
                        $('#login-error-message').css("display", "block");
                    },
                }
            });
        });
    }
}


function signup() {
    let username = form.children[0].value;
    let password = form.children[1].value;
    let confirmpassword = form.children[2].value;

    if (username != '' && password != '' && confirmpassword != '') {
        if (password.localeCompare(confirmpassword) == 0) {
            $.getJSON("http://jsonip.com?callback=?", function (res, status) {
                let data = `username=${username}&password=${password}&ip=${res.ip}`;

                $.post(serverIP + '/signup', data, (res, status) => {
                    window.location.href = './login.html';
                });
            });
        } else if(password.localeCompare(confirmpassword) != 0){
            $('#password-match-error').css("display", "block");

            $('#signup-error-message').css("display", "none");
            $('#field-empty').css('display', 'none');
        }else{
            $('#password-match-error').css("display", "none");
            $('#field-empty').css('display', 'none');

            $('#signup-error-message').css("display", "block");
        }
    } else {
        $('#field-empty').css('display', 'block');

        $('#password-match-error').css("display", "none");
        $('#signup-error-message').css("display", "none");
    }
}