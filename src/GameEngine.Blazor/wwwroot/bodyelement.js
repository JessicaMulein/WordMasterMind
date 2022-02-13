var JsFunctions = window.JsFunctions || {};

JsFunctions = {
    setDocumentTitle: function (title) {
        document.title = title;
    },
    getBodyElement: function() {
        return document.getElementsByTagName("body")[0];
    },
    addCssClassToBody: function (cssClass) {
        let body = window.JsFunctions.getBodyElement();

        if (!body.classList.contains(cssClass)) {
            body.classList.add(cssClass);
        }
    },
    removeCssClassFromBody: function (cssClass) {
        let body = window.JsFunctions.getBodyElement();

        if (body.classList.contains(cssClass)) {
            body.classList.remove(cssClass);
        }
    },
    setNightMode: function (value) {
        let body = window.JsFunctions.getBodyElement();

        if (value) {
            window.JsFunctions.addCssClassToBody("nightmode");
        } else {
            window.JsFunctions.removeCssClassFromBody("nightmode");
        }
    },
    setLanguageOfBody: function (language) {
        let body = window.JsFunctions.getBodyElement();
        body.lang = language;
    },
    setTextDirectionOfBody: function (direction) {
        let body = window.JsFunctions.getBodyElement();
        body.dir = direction;
    },
    showAlert(message, duration = 1000) {
        const alertContainer = document.querySelector("#alert-container")
        const alert = document.createElement("div")
        alert.textContent = message
        alert.classList.add("alert")
        alertContainer.prepend(alert)
        if (duration == null) return

        setTimeout(() => {
            alert.classList.add("hide")
            alert.addEventListener("transitionend", () => {
                alert.remove()
            })
        }, duration)
    }
};
