$(document).idle({
    onIdle: function () {
        $.post("/User/SeshCh", function (data) {
            if (data.StatusCode === -4000) {
                gigya.socialize.logout({
                    callback: function (response) {
                        location.href = '/Home/ConcurrentLogin';
                    }
                });
                location.href = '/Home/ConcurrentLogin';
            }
        });
    },
    idle: 15000
})