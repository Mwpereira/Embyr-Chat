// Event Listeners
$(document).ready(function () {
    initUI();

    // Client sends a message
    $('#send-message-form').on('submit', (e) => {
        e.preventDefault();
        handleSendMessage();
    });

    // Client clicks view friend request button in nav bar
    $('#friend-req-btn').on("click", (e) => {
        e.preventDefault();
        $('#request-exists-message').hide();
        showFriendReqTab();
    });

    // Client wants to send friend request
    $('#add-friend-form').on('submit', (e) => {
        e.preventDefault();
        friendRequestEvent('addFriendReq', clientInfo.accountname, $('#add-friend-form > input').val());
        $('#add-friend-form > input').val("");
    });

    // Load older messages when client scrolls to top
    $('#chat-view').on('scroll', (e) => {
        if ($('#chat-view').scrollTop() == 0) {
            messages = JSON.parse(sessionStorage.getItem($('#friend-profile-tab > .usertag').html())).message;
            if (messages.length != 0) {
                getMessages($('#friend-profile-tab > .usertag').html(), messages.length);
            }
        };
    });

    // Client clicks logout button
    $('#logout-btn').on('click', (e) => {
        e.preventDefault();
        if (confirm("OK?")) {
            sessionStorage.clear();
            window.location.href = "./index.html";
        }
    });

    // Client clicks profile page button
    $('#profile-page-btn').on('click', (e) => {
        e.preventDefault();
        let tab = $('#profile-view');
        let messagebar = $("#send-message-form > input");

        if (tab.css("display").localeCompare("none") == 0) {
            $('#profile-view-header').css("display", "flex");
            tab.show();
            messagebar.hide();
            $('#friends-view').hide();
            $('#friends-view-header').hide();
        } else {
            $('#profile-view-header').hide();
            tab.hide();
            messagebar.show();
        }
    });

    // Client changes online status
    $('#online-status-dropdown').on('change', (e) => {
        e.preventDefault();
        setOnlineStatus($('#online-status-dropdown').val());
    });

    // Client clicks on profile picture
    $('#profile-pic').on('click', e => {
        $('#profile-pic-uploader').click();
    });

    // Client selects a new profile picture
    $('#profile-pic-uploader').on('change', e => {
        e.preventDefault();
        let img = document.getElementById('profile-pic-uploader').files[0];
        let formats = ['image/gif', 'image/jpeg', 'image/png'];

        if (formats.includes(img['type'])) {
            let info = JSON.parse(sessionStorage.getItem('clientInfo'));
            let reader = new FileReader();

            reader.onload = () => {
                displayProfilePhoto(clientInfo.accountname, reader.result);
                info.profilePhoto = reader.result;
                sessionStorage.setItem('clientInfo', JSON.stringify(info));
                setNewProfilePhoto(reader.result);
            }
            reader.readAsDataURL(img);

        } else {
            console.log('Not an image');
        }
    });

    // Client clicks ghost mode button
    $("#ghost-mode").on('click', e => {
        e.preventDefault();
        handleGhostMode();
    });


    $('#password-change-form').on('submit', (e) => {
        e.preventDefault();
        let form = document.getElementById('password-change-form');
        handlePasswordChange(form.getElementsByTagName('input')[0].value, form.getElementsByTagName('input')[1].value, form.getElementsByTagName('input')[2].value);
    });

    $('#delete-account-form').on('submit', (e) => {
        e.preventDefault();
        if (document.getElementById('delete-account-pw').value != '') {
            if (confirm("Are you sure you want to delete your account?")) {
                if (confirm("This is your last chance to change your mind. Your account will be unable to be recovered. ARE YOU SURE?")) {
                    deleteAccount($('#delete-account-pw').val());
                }
            }
        }
    })


    $('#delete-friend-btn').on('click', (e) => {
        e.preventDefault();
        if (confirm("Are you sure? All messages will be deleted!")) {
            $(`.friend-name:contains(${$('#friend-profile-tab > p').html()})`).parent().remove();
            deleteFriend($('#friend-profile-tab > p').html());
        }
    });

});


function handlePasswordChange(oldPW, newPW, confirmPW) {

    if (oldPW != '' && newPW != '' && confirmPW != '') {
        if (newPW.localeCompare(confirmPW) == 0) {
            changePassword(oldPW, newPW);
        } else {
            console.log("new passwords do not match")
        }
    } else {
        console.log('fields cannot be empty')
    }
}

// Handles enabling/disabling ghost mode
function handleGhostMode() {
    let friendname = $('#friend-profile-tab > .usertag').html();

    let info = JSON.parse(sessionStorage.getItem(friendname));

    if (!info.ghostMode) {
        $('#ghost-mode').css("color", "rgb(43, 40, 37)")
    } else {
        $('#ghost-mode').css("color", "white");
    }

    info.ghostMode = !info.ghostMode;
    sessionStorage.setItem(friendname, JSON.stringify(info));
}


/*
    Handles actions when client sends message
*/
function handleSendMessage() {
    let message = $('#send-message-form > input').val();
    $('#send-message-form > input').val("");

    if ($.trim(message) != '') {
        let friendname = $('#friend-profile-tab > .usertag').html();
        let options = { month: "short", day: "numeric", year: "numeric", hour: "numeric", minute: "numeric" };
        let messageTime = new Date().toLocaleTimeString([], options);
        createMessageBubble(message, messageTime, "user-message-bubble", true);
        sendMessage(clientInfo.accountname, friendname, message, JSON.parse(sessionStorage.getItem(friendname)).ghostMode);
        updateLocalMessages(friendname, clientInfo.accountname, message, messageTime, true);
    }
}

/*
    Handles actions when client clicks on a friend in friends-list
*/
function handleSelectFriend(bubble) {
    displayMessages(bubble.querySelector('p').innerHTML);
    if ($('#friends-view').css("display").localeCompare("none") != 0) showFriendReqTab();
    if ($('#profile-view').css("display").localeCompare("none") != 0) {
        $('#profile-view').css("display", "none");
        $('#profile-view-header').css("display", "none");
        $("#send-message-form > input").show();
    }
    $(bubble).find('i').css("display", "none");
}

/*
    Stores newly sent/received messages and corresponding sender into session storage.
    @param: friendname - Username of friend whose conversation is to be updated
    @param: sender - Who sent the the message, client or friend
    @param: message - The message that was sent/received
    @parem: messageTime - Time of message (sent/received)
*/
function updateLocalMessages(friendname, sender, message, messageTime, newMessage) {
    let friendInfo = JSON.parse(sessionStorage.getItem(friendname));

    if (newMessage) {
        friendInfo.message.push(message);
        friendInfo.sender.push(sender);
        friendInfo.time.push(messageTime);
    } else {
        friendInfo.message.unshift(message);
        friendInfo.sender.unshift(sender);
        friendInfo.time.unshift(messageTime);
    }

    sessionStorage.setItem(friendname, JSON.stringify(friendInfo));
}
