var iframe = document.querySelector('#iframe');
if (iframe) {
    iframe.addEventListener('load', e => {
        e.target.contentWindow.addEventListener('scroll', e => {
            if ($("#iframe").contents().find('body').scrollTop() == 0) {
                document.getElementById("module_nav_bar").style.display = "inherit";
            } else {
                document.getElementById("module_nav_bar").style.display = "none";
            }
        });
    });
}

function clickEvent() {
    $('#iframe').contents().click(function (event) {
        var clickTarget = event.target.attributes.target;
        var clickHref = event.target.attributes.href;
        if (clickTarget) {
            if (clickTarget.value == "_top") {
                event.preventDefault();
                event.target.setAttribute("target", "_self");
                event.target.click();
            }
        }
    });
}