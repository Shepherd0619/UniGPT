mergeInto(LibraryManager.library, {

    ScheduleNotification: function (title, message, delayInSeconds) {
        if (typeof Notification !== 'undefined') {
            if (Notification.permission !== 'granted') {
                Notification.requestPermission().then(function(permission) {
                    if (permission === 'granted') {
                        var notification = new Notification(title, {
                            body: message,
                            icon: '' // 设置通知图标URL
                            });
                    }
                });
            } else {
                var notification = new Notification(title, {
                    body: message,
                    icon: '' // 设置通知图标URL
                });
            }
            setTimeout(function() {
                notification.close();
            }, delayInSeconds * 1000);
        }
    }
});