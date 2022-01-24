var JsFunctions = window.JsFunctions || {};

JsFunctions = {
    setDocumentTitle: function (title) {
        document.title = title;
    },
    getBodyElement: function() {
        return document.getElementsByTagName("body")[0];
    },
    addCssClassToBody: function (cssClass) {
        var body = window.JsFunctions.getBodyElement();

        if (!body.classList.contains(cssClass)) {
            body.classList.add(cssClass);
        }
    },
    setLanguageOfBody: function (language) {
        var body = window.JsFunctions.getBodyElement();
        body.lang = language;
    },
    setTextDirectionOfBody: function (direction) {
        var body = window.JsFunctions.getBodyElement();
        body.dir = direction;
    }
};
