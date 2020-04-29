const serverIP = 'http://99.227.48.179:8000';

/*
    Requests server for messages for specific friend
*/
function getMessages(friendname, numLoaded) {
    let data = `sender=${clientInfo.accountname}&receiver=${friendname}&n=${numLoaded}`;

    $.post(serverIP + '/loadmessages', data, function (messages) {

        if (/*sessionStorage.getItem(friendname) === null*/ numLoaded == 0) {
            //sessionStorage.setItem(`${friendname}`, JSON.stringify(messages));
            let json = JSON.parse(sessionStorage.getItem(`${friendname}`));
            json.message = messages.message;
            json.sender = messages.sender;
            json.time = messages.time;
            sessionStorage.setItem(`${friendname}`, JSON.stringify(json));
        } else {
            if (messages.message != null) {
                let i = messages.message.length - 1;

                for (i; i >= 0; i--) {
                    updateLocalMessages(friendname, messages.sender[i], messages.message[i], messages.time[i], false);
                    if (clientInfo.accountname.localeCompare(messages.sender[i]) == 0) {
                        createMessageBubble(messages.message[i], messages.time[i], "user-message-bubble", false);
                    } else {
                        createMessageBubble(messages.message[i], messages.time[i], "friend-message-bubble", false);
                    }
                }
            }
        }

    });
}


function changePassword(oldPW, newPW) {
    $.ajax({
        url: serverIP + '/account',
        type: 'PUT',
        data: `username=${clientInfo.accountname}&password=${oldPW}&newPassword=${newPW}`,
        success: console.log('password changed'),
        statusCode: {
            403: () => console.log('The current password you entered is incorrect')
        }
    })
}


function deleteAccount(password) {
    $.ajax({
        url: serverIP + '/account',
        type: 'DELETE',
        data: `username=${clientInfo.accountname}&password=${password}`,
        success: () => {
            window.location.href = './index.html';
            sessionStorage.clear();
        },
        statusCode: {
            403: () => console.log('The password entered is incorrect')
        }
    })
}

function deleteFriend(friendname) {
    $.ajax({
        url: serverIP + '/friend',
        type: 'DELETE',
        data: `username=${clientInfo.accountname}&friend=${friendname}`,
    })
}