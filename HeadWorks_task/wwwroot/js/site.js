$(function () {
    let tokenKey = "accessToken";

    $("#form-login").submit(function (e) {
        e.preventDefault();
        let formAction = "/home/login";
        let cookiHoursTime = 24;//время жизни куки
        let fdata = new FormData();
        let username = $(".input-login").val();

        fdata.append("username", username);

        $(".btn-login > input").attr("disabled", "disabled");

        $.ajax({
            type: 'post',
            url: formAction,
            data: fdata,
            processData: false,
            contentType: false
        }).done(function (result) {
            //console.log(result);
            switch (result.status.toLowerCase()) {
                case "ok": {
                    //sessionStorage.setItem(tokenKey, result.message);
                    var expire = new Date();
                    expire.setHours(expire.getHours() + cookiHoursTime);
                    document.cookie = tokenKey + "=" + result.message + ";expires=" + expire.toUTCString() + ";";
                    $(location).attr('href', '/dragons');
                    break;
                }
                case "error": {
                    alert(result.message);
                    $(".btn-login > input").removeAttr("disabled");
                    break;
                }
            }
        });
    });
    $(".attack-btn").click(function (e) {
        e.preventDefault();

        let formAction = "/home/attackDragon";
        let fdata = new FormData();
        let dragonId = $(".dragon-id").text();

        fdata.append("dragonId", Number(dragonId));
        $(".attack-btn").attr("disabled", "disabled");

        $.ajax({
            type: 'post',
            url: formAction,
            data: fdata,
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {
                var token = getCookie(tokenKey);
                //console.log(token);
                //var token = sessionStorage.getItem(tokenKey);
                //console.log(token);
                xhr.setRequestHeader("Authorization", "Bearer " + token);
            }
        }).done(function (result) {
            console.log(result);
            switch (result.status.toLowerCase()) {
                case "ok": {
                    let p = result.dragon.currentHealth / result.dragon.allHealth * 100;
                    $("div.progress-bar.progress-bar-danger").text(result.dragon.currentHealth + " HP")
                    $("div.progress-bar.progress-bar-danger").css("width", p + '%');
                    $(".current-damage").text("-" + result.damage + " HP")
                    $(".current-damage").removeClass("hide");
                    if (result.view !== null) {
                        $(".wrap").html(result.view);
                    }
                    $(".attack-btn").removeAttr("disabled");
                    break;
                }
                case "error": {
                    alert(result.message);
                    break;
                }
            }
        });
    });
    // возвращает cookie с именем name, если есть, если нет, то undefined
    function getCookie(name) {
        var matches = document.cookie.match(new RegExp(
            "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
        ));
        return matches ? decodeURIComponent(matches[1]) : undefined;
    }
})