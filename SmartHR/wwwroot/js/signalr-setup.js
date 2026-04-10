"use strict";

// Setup Toastr options
if (typeof toastr !== 'undefined') {
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-bottom-right",
        "timeOut": "5000"
    };
}

function fetchNotifications() {
    fetch(/api/Notifications/unread)
        .then(response => response.json())
        .then(data => {
            updateNotificationUI(data.count, data.data);
        })
        .catch(err => console.error("Failed to fetch notifications", err));
}

function updateNotificationUI(count, items) {
    let countBadge = document.getElementById("notificationCount");
    let itemsList = document.getElementById("notificationItems");

    if (countBadge) {
        countBadge.innerText = count > 0 ? count : "";
        countBadge.style.display = count > 0 ? "inline-block" : "none";
    }

    if (itemsList) {
        itemsList.innerHTML = "";
        
        if (count === 0 || !items || items.length === 0) {
            itemsList.innerHTML = '<li class="p-3 text-center text-muted small"><i class="fas fa-bell-slash fa-2x mb-2 opacity-50"></i><br>·«  ÊÃœ ≈‘⁄«—«  ÃœÌœ…</li>';
            return;
        }

        items.forEach(n => {
            let li = document.createElement("li");
            let targetUrl = n.targetUrl ? n.targetUrl : "#";
            
            li.innerHTML = \
                <a class="dropdown-item d-flex align-items-center mark-read-link" href="\" data-id="\">
                    <div class="bg-primary bg-opacity-10 text-primary rounded-circle p-2 me-3">
                        <i class="fas fa-bell"></i>
                    </div>
                    <div>
                        <div class="small fw-bold text-wrap">\</div>
                        <div class="small text-muted text-wrap">\</div>
                        <div class="small text-muted" style="font-size: 0.70rem;">\</div>
                    </div>
                </a>
            \;
            itemsList.appendChild(li);
        });
        
        itemsList.innerHTML += \
            <li><hr class="dropdown-divider"></li>
            <li><a class="dropdown-item text-center small text-primary fw-bold" href="#" id="markAllReadBtn"> ÕœÌœ «·ﬂ· ﬂ„ﬁ—Ê¡</a></li>
        \;
        
        document.querySelectorAll('.mark-read-link').forEach(link => {
            link.addEventListener('click', function(e) {
                let id = this.getAttribute('data-id');
                fetch(\/api/Notifications/mark-read/\\, { method: 'POST' });
            });
        });
        
        let markAllBtn = document.getElementById('markAllReadBtn');
        if(markAllBtn) {
            markAllBtn.addEventListener('click', function(e) {
                e.preventDefault();
                fetch(\/api/Notifications/mark-all-read\, { method: 'POST' })
                    .then(() => fetchNotifications());
            });
        }
    }
}

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// 2. Notifications general
connection.on("ReceiveNotification", function (message) {
    try {
        if (typeof toastr !== 'undefined') {
            toastr.info(message, "≈‘⁄«— ÃœÌœ");
        }
        fetchNotifications();
    } catch (e) {
        console.error("Notification UI update failed", e);
    }
});

// Ticket status changes
connection.on("TicketStatusChanged", function (ticketId, status) {
    let statusBadge = document.getElementById("status-badge-" + ticketId);
    if (statusBadge) {
        statusBadge.innerText = status;
        statusBadge.className = status === "Closed" ? "badge bg-danger" : "badge bg-success";
    }
    if (typeof toastr !== 'undefined') {
        toastr.success(\Õ«·… «· –ﬂ—… #\  €Ì—  ≈·Ï: \\, " ÕœÌÀ  –ﬂ—…");
    }
});

// Chat Messages
connection.on("ReceiveMessage", function (senderName, messageContent, targetUrl) {
    if (typeof toastr !== 'undefined') {
        toastr.success(messageContent, \—”«·… „‰ \\, {
            onclick: function() { window.location.href = targetUrl; }
        });
    }
});

connection.start().then(function () {
    fetchNotifications();
}).catch(function (err) {
    return console.error(err.toString());
});
