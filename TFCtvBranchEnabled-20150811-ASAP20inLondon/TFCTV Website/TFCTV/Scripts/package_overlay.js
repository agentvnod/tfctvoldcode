$(".subscribe_btn").tooltip({ opacity: 100
            , relative: true
            , onBeforeShow: function () {
                if (this.getTip().children().size() > 5) {
                    this.getTip().addClass("tooltip_tall");
                }
            }
});

$(".addwishlist_btn").tooltip({ opacity: 100
            , relative: true
            , onBeforeShow: function () {
                if (this.getTip().children().size() > 5) {
                    this.getTip().addClass("tooltip_tall");
                }
            }
});

$('.buyproduct').overlay({
    mask: 'black',
    fixed: false,
    closeOnClick: false,
    onBeforeLoad: function () {
        $(".apple_overlay").width(640);
        var wrap = this.getOverlay().find(".contentWrap");
        wrap.load(this.getTrigger().attr("href"));
    },
    onClose: function () {
        $('#bs_overlay .contentWrap').empty();
        if ($('#bought').val() == 1) {
            //location.reload();
            window.location.href = document.URL;
        }
    }
});

$(".addwishlist").overlay({
    // some mask tweaks suitable for modal dialogs
    mask: 'black',
    fixed: false,
    closeOnClick: false,
    onBeforeLoad: function () {
        $(".apple_overlay").width(250);
        var wrap = this.getOverlay().find(".contentWrap");
        jQuery.ajax({
            url: "/wishlist/add",
            data: "id=" + this.getTrigger().attr('prodid'),
            dataType: "json",
            type: "Post",
            beforeSend: function () {
                wrap.html("<h3>Processing your request...</h3>");
            },
            success: function (data) {
                switch (data.errorCode) {
                    case 0:
                        wrap.html("<h3>Successfully Added to your Wishlist</h3>");
                        break;
                    case -400:
                        wrap.html("<h3>Please Signin first to continue...</h3>");
                        break;
                    case -901:
                        wrap.html("<h3>This Item already exist in your wishlist</h3>");
                        break;
                    default:
                        wrap.html("<h3>Sorry Error had Occurred</h3><i> ErrorCode: " + data.errorCode + " </i>");
                        break;
                }
            }
        });
    },
    onClose: function () {
        $('#wl_overlay .contentWrap').empty();
    }
});