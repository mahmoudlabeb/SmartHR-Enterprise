// إعداد الاتصال بـ SignalR Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// 1. استقبال الإشعارات العامة (مثل طلبات الإجازات أو رسائل النظام)
connection.on("ReceiveNotification", function (message) {
    // يمكنك لاحقاً تحويلها إلى Bootstrap Toast ليكون شكلها أجمل
    alert("🔔 إشعار جديد من SmartHR:\n\n" + message);
});

// 2. استقبال إشعارات تحديث التذاكر
connection.on("TicketStatusChanged", function (ticketId, status) {
    alert("🎫 تحديث تذكرة:\n\nتم تغيير حالة التذكرة رقم " + ticketId + " إلى: " + status);

    // إذا كان المستخدم داخل صفحة التذاكر، قم بتحديث الصفحة تلقائياً
    if (window.location.href.indexOf("Tickets") > -1) {
        location.reload();
    }
});

// بدء الاتصال بالسيرفر
connection.start().then(function () {
    console.log("تم الاتصال بنظام الإشعارات اللحظية (SignalR) بنجاح!");
}).catch(function (err) {
    return console.error(err.toString());
});