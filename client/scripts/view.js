
let clientInfo = JSON.parse(sessionStorage.getItem('clientInfo'));


function initUI() {
    //let clientInfo = JSON.parse(sessionStorage.getItem("clientInfo"));

    // Sets names for client and friend in UI
    $('#client-accountname').html((clientInfo.accountname).toString());
    $('#friend-profile-tab > .usertag').html(clientInfo.friendsList[0]);
    $('#profile-name').html(clientInfo.accountname).toString();

    // Create friend-list bubbles
    clientInfo.friendsList.forEach(friend => {
        createFriendBubble(friend, clientInfo.friendStatus[clientInfo.friendsList.indexOf(friend)]);
    });

    // Display messages of first friend in friend list on login
    if ($('#friend-profile-tab > .usertag').html() != "") {
        displayMessages($('#friend-profile-tab > .usertag').html());
    }

    //Incoming Requests
    clientInfo.friendReqIn.forEach(element => {
        createIncomingRequestBubble(element);
    });

    //Outgoing Requests
    clientInfo.friendReqOut.forEach(element => {
        createOutgoingRequestBubble(element);
    });

    $('#online-status-dropdown').val(clientInfo.status);

}



/*
    Creates a text bubble to display a message
    @param: message - Message to be displayed
    @param: wrapperClass - "user-message-bubble" or "friend-message-bubble"
*/
function createMessageBubble(message, messageTime, wrapperClass, newMessage) {
    let chat = $('#chat-view');
    let $bubble = $(`
    <div class="${wrapperClass}">
        <div class="message-bubble">
            <p>${message}</p>
            <p class="message-time">${messageTime}</p>
        </div>
    </div>`);

    if (newMessage) chat.prepend($bubble);
    else chat.append($bubble);
}

/*
    Creates a friend bubble to display in friends list
    @param: friendName - Name of friend to be added
*/
function createFriendBubble(friendName, onlineStatus) {
    let statusColor;
    switch (onlineStatus) {
        case "online":
            statusColor = "green";
            break;
        case "away":
            statusColor = "yellow";
            break;
        default:
            statusColor = "red";
    }


    let $bubble = $(`
    <div class="friend-bubble">
        <img class="avatar" src="">
        <p class="friend-name">${friendName}</p>
        <span>
            <i class="material-icons message-notif md-18">message</i>
            <svg height="24" width="24">
                <circle class="${statusColor}" cx="12" cy="12" r="6" />
            </svg>
        </span>
    </div>`)

    $('#friends-list').append($bubble);

    $($bubble).on('click', function (e) {
        e.preventDefault();
        handleSelectFriend(this);
    })
}

function removeIncomingRequest(name) {
    $('#incoming-requests-list > li').each(function () {
        if ($(this).children('p').html().localeCompare(name) == 0) {
            $(this).remove();
        }
    });
}

function removeOutgoingRequest(name) {
    $('#outgoing-requests-list > li').each(function () {
        if ($(this).children('p').html().localeCompare(name) == 0) {
            $(this).remove();
        }
    });
}


/*
    Displays loaded messages from session storage when client clicks on friend in friends list
    @param: friendname - Name of friend whose messages are to be displayed
*/
function displayMessages(friendname) {
    $('#friend-profile-tab > .usertag').html(friendname);

    $('#chat-view').empty();
    if (JSON.parse(sessionStorage.getItem(friendname)).message.length === 0) {
        getMessages(friendname, 0);
    }

    setTimeout(() => {
        let messages = JSON.parse(sessionStorage.getItem(friendname));
        let senderlist = messages.sender;
        let times = messages.time;
        let i = 0;
        $('#friend-profile-tab > img').attr("src", JSON.parse(sessionStorage.getItem(friendname)).profilePhoto);
        for (i; i < messages.message.length; i++) {
            if (clientInfo.accountname.localeCompare(senderlist[i]) == 0) {
                createMessageBubble(messages.message[i], times[i], "user-message-bubble", true);
            } else {
                createMessageBubble(messages.message[i], times[i], "friend-message-bubble", true);
            }
        };
    }, 1000);
    
}

/*
    Displays/hides the friend-req-tab
*/
function showFriendReqTab() {
    let tab = $('#friends-view');
    let messagebar = $("#send-message-form > input");

    if (tab.css("display").localeCompare("none") == 0) {
        $('#friends-view-header').css("display", "flex");
        tab.show();
        messagebar.hide();
        $('#profile-view').hide();
        $('#profile-view-header').hide();
    } else {
        $('#friends-view-header').hide();
        tab.hide();
        messagebar.show();
    }
}



function createIncomingRequestBubble(name) {
    let bubble = $(`
        <li class="friend-request-bubble">
            <img class="avatar" src="../assets/imgs/linkedin.png">
            <p class="usertag">${name}</p>
            <div class="btn-wrapper">
                <button class="action-btn accept-req-btn">Accept</button>
                <button class="action-btn decline-req-btn">Decline</button>
            </div>
        </li    
    `);
    $('#incoming-requests-list').append(bubble);

    $(bubble).find('.accept-req-btn').on('click', function (e) {
        e.preventDefault();
        createFriendBubble($(this).parent().parent().children('p').html(), 'online');
        removeIncomingRequest($(this).parent().parent().children('p').html());
        friendRequestEvent('acceptFriendReq', clientInfo.accountname, $(this).parent().parent().children('p').html())
    });

    $(bubble).find('.decline-req-btn').on('click', function (e) {
        e.preventDefault();
        removeIncomingRequest($(this).parent().parent().children('p').html());
        friendRequestEvent('declineFriendReq', clientInfo.accountname, $(this).parent().parent().children('p').html());
    });

}

function createOutgoingRequestBubble(name) {
    let $bubble = $(`
            <li class="friend-request-bubble">
                <img class="avatar" src="../assets/imgs/linkedin.png">
                <p class="usertag">${name}</p>
                <div class="btn-wrapper">
                    <button class="action-btn cancel-req-btn">Cancel</button>
                </div>
            </li>`
    );
    $('#outgoing-requests-list').append($bubble);

    $($bubble).find('.cancel-req-btn').on('click', function (e) {
        e.preventDefault();
        removeOutgoingRequest($($bubble).find('p').html());
        friendRequestEvent('cancelFriendReq', clientInfo.accountname, $(this).parent().parent().children('p').html());
    });
}




function setFriendOnlineStatus(friend, onlineStatus) {
    let color;

    switch (onlineStatus) {
        case "online":
            color = 'green';
            break;
        case "away":
            color = 'yellow';
            break;
        default:
            color = 'red';
    }

    $(`.friend-name:contains(${friend})`).parent().find('circle').attr("class", color);
}


function messageAlert(friend) {
    let friendBubble = $(`.friend-name:contains(${friend})`).parent();
    let x = document.getElementById('audio');

    $(friendBubble).find('i').css("display", "block");
    x.play();
}


function displayProfilePhoto(name, photoBytes) {
    if (name.localeCompare(clientInfo.accountname) == 0) {
        $('#profile-pic').attr("src", photoBytes);
        $('#profile-page-btn > .avatar').attr("src", photoBytes);
    } else {
        $(`.friend-name:contains(${name})`).parent().find('img').attr("src", photoBytes);
    }
}