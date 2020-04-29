var ws = new WebSocket("ws://99.227.48.179:8001");

ws.onopen = function (e) {
    console.log("Connected To WebSocket!");
    ws.send(clientInfo.accountname);
}

ws.onmessage = function (e) {
    let eventData = JSON.parse(e.data);

    switch (eventData.eventV) {
        case "receiveMessage": // Client received message
            if ($('#friend-profile-tab > .usertag').html().localeCompare(eventData.sender) == 0) {
                createMessageBubble(eventData.message, eventData.time, "friend-message-bubble", true);
            }
            messageAlert(eventData.sender);
            updateLocalMessages(eventData.sender, eventData.sender, eventData.message, eventData.time, true);
            break;

        case "incomingRequest": // Add to incoming (Client receives friend request)
            createIncomingRequestBubble(eventData.sender);
            break;

        case "cancelIncomingRequest": // Remove from incoming (Remote client cancels their friends request to client)
            $('#incoming-requests-list > li').each(function () {
                if ($(this).children('p').html().localeCompare(eventData.sender) == 0) {
                    $(this).remove();
                }
            });
            break;

        case "addFriend": // Add to friends list/Remove from outgoing (Remote client accepts friend request from client)
            createFriendBubble(eventData.sender, 'online');
            $('#outgoing-requests-list > li').each(function () {
                if ($(this).children('p').html().localeCompare(eventData.sender) == 0) {
                    $(this).remove();
                }
            });
            break;

        case "declineFriendRequest": // Remove from outgoing (Remote client declines friend request from client)
            $('#outgoing-requests-list > li').each(function () {
                if ($(this).children('p').html().localeCompare(eventData.sender) == 0) {
                    $(this).remove();
                    break;
                }
            });
            break;

        case "setProfilePhoto":
            displayProfilePhoto(eventData.user, eventData.photoBytes);
            if (eventData.user.localeCompare(clientInfo.accountname) == 0) {
                let info = JSON.parse(sessionStorage.getItem('clientInfo'));
                info.profilePhoto = eventData.photoBytes;
                sessionStorage.setItem('clientInfo', JSON.stringify(info));
            } else {
                let info = JSON.parse(sessionStorage.getItem(eventData.user));
                info.profilePhoto = eventData.photoBytes;
                sessionStorage.setItem(eventData.user, JSON.stringify(info));
            }
            break;

        case "userExists": // Checks if person receiving request exists
            if (eventData.existsV) {
                createOutgoingRequestBubble(eventData.user);
                $('#request-exists-message').hide();
            } else {
                $('#request-exists-message').show();
            }
            break;

        case "setStatus":
            setFriendOnlineStatus(eventData.friend, eventData.status);
            break;


        default:
            console.log("ERROR: No Event Handler Exists -> " + eventData);
    }
}

// Sends message
function sendMessage(sender, receiver, message, ghostMode) {
    var messageData = {
        "job": "sendMessage",
        "sender": sender,
        "receiver": receiver,
        "message": message,
        "ghostMode": ghostMode
    }
    ws.send(JSON.stringify(messageData));
}


// Sends an event related to friend requests(send, accept, decline, cancel)
function friendRequestEvent(job, sender, receiver) {
    var data = {
        "job": job,
        "sender": sender,
        "receiver": receiver
    }
    ws.send(JSON.stringify(data));
}

// Sets online status of client
function setOnlineStatus(status) {
    var data = {
        "job": "setStatus",
        "sender": clientInfo.accountname,
        "status": status
    }
    ws.send(JSON.stringify(data));
}

function setNewProfilePhoto(photoBytes) {
    var data = {
        "job": "setProfilePhoto",
        "sender": clientInfo.accountname,
        "photoBytes": photoBytes
    }
    ws.send(JSON.stringify(data));
}


ws.onclose = function (e) {
    console.log("Connection Closed");
}