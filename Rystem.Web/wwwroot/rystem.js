class Rystem {
    constructor(id) {
        this.id = id;
    }
    static stringEmpty = "";
    getName() {
        return "Rystem class";
    }
    static standardLoader = '<svg id="rystem-loader" viewBox="0 0 120 120"><g class="g1"><rect class="r1" x="30" y="30" width="60" height="60" /><rect class="big" x="81" y="81" width="8" height="8" /><rect class="r_ol" x="31" y="31" width="8" height="8" /><rect class="r_or" x="81" y="31" width="8" height="8" /><rect class="r_ul" x="31" y="81" width="8" height="8" /><xrect class="r_ur" x="81" y="81" width="8" height="8" /></g></svg>';
    static loaderId = "#rystem-loader";
    static showLoader() {
        if ($(ModalRystem.loaderId).length == 0)
            $("body").append(ModalRystem.standardLoader);
        else
            $(ModalRystem.loaderId).show();
    }
    static hideLoader() {
        $(ModalRystem.loaderId).hide();
    }

    static httpRequest(request, withLoader, query, event, obj, onSuccess, onFailure) {
        if (withLoader)
            Rystem.showLoader();
        $.ajax({
            type: request.method,
            url: request.url + (query && query.length > 0 ? (request.url.indexOf("?") > -1 ? "&" : "?") + query : Rystem.stringEmpty),
            data: request.data == "null" ? Rystem.stringEmpty : request.data,
            success: function (data) {
                if (onSuccess)
                    onSuccess(data);
                else if (request.selector && request.selector.length > 0)
                    $(request.selector).html(data);
                if (request.onSuccess)
                    request.onSuccess(data, event, obj);
                if (withLoader)
                    Rystem.hideLoader();
            },
            error: function (data) {
                if (onFailure)
                    onFailure(data);
                if (request.onFailure)
                    request.onFailure(data, event, obj);
                if (withLoader)
                    Rystem.hideLoader();
            }
        });
    }
}

class ModalRystem extends Rystem {
    static actives = new Array();
    static standardHtml = '<div class="modal fade" id="{modalid}" tabindex="-1" style="z-index:{zindex};" role="dialog" aria-hidden="true">' +
        '<div class="modal-dialog {modalsize}" role="document">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span>' +
        '</button>' +
        '</div>' +
        '<div class="modal-body">' +
        '</div>' +
        '</div>' +
        '</div>';
    constructor(id, update) {
        super(id);
        this.update = update;
    }
    static hasActive() {
        return ModalRystem.actives.length > 0;
    }
    static active() {
        return ModalRystem.actives[ModalRystem.actives.length - 1];
    }
    static close() {
        ModalRystem.actives.pop().hide();
    }
    static attachEvent(id) {
        $(id).on("hide.bs.modal", function () {
            ModalRystem.close();
        });
        const aligner = function () {
            let modalContent = $("#" + ModalRystem.active().id + " .modal-content");
            const windowHeight = $(window).height();
            const modalContentHeight = modalContent.height();
            if (modalContentHeight <= windowHeight)
                modalContent.css("margin-top", Math.max(0, (windowHeight - modalContentHeight) / 2));
        };
        $(id).on("shown.bs.modal", aligner);
        $(window).on("resize", aligner);
    }
    show(event, obj, request, size) {
        let modal = this;
        $("body").append(ModalRystem.standardHtml.replace("{modalid}", this.id).replace("{zindex}", (1050 + ModalRystem.actives.length)).replace("{modalsize}", size));
        ModalRystem.attachEvent("#" + this.id);
        Rystem.httpRequest(request, true, Rystem.stringEmpty, event, obj, function (data) {
            $("#" + modal.id + " .modal-body").html(data);
            $("#" + modal.id).modal('show');
            if (ModalRystem.hasActive())
                $(ModalRystem.active().id).css("visibility", "hidden");
            ModalRystem.actives.push(modal);
        }, event, obj);
    }
    hide() {
        if (this.update)
            Rystem.httpRequest(this.update, "true");
        $("#" + this.id).remove();
    }
}

class DropdownRystem extends Rystem {
    constructor(id, request, update) {
        super(id);
        this.request = request;
        this.update = update;
    }
    show() {
        let dropdown = this;
        $('#' + dropdown.id).selectpicker();
        $('#' + dropdown.id).parent().attr("id", "dropdown-container-" + dropdown.id);
        if (dropdown.request)
            $('#' + dropdown.id).on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) {
                const selectedItems = $(e.target).val();
                let query = Rystem.stringEmpty;
                for (let i = 0; i < selectedItems.length; i++) {
                    query += "selecteditems=" + selectedItems[i];
                    if (i < selectedItems.length - 1)
                        query += "&";
                }
                Rystem.httpRequest(dropdown.request, false, query, e, e.target, function () {
                    if (dropdown.update)
                        Rystem.httpRequest(dropdown.update, false, Rystem.stringEmpty, e, e.target);
                });
            });
    }
}

class FormRystem extends Rystem {
    constructor(id, request, ifInModalCloseAfterValidSubmit, toast, update) {
        super(id);
        this.request = request;
        this.ifInModalCloseAfterValidSubmit = ifInModalCloseAfterValidSubmit;
        this.toast = toast;
        this.update = update;
    }

    submit(e, obj) {
        e.preventDefault();
        let form = this;
        form.request.data = $(obj).serialize();
        Rystem.httpRequest(form.request, true, Rystem.stringEmpty, e, obj, function (data) {
            if (form.ifInModalCloseAfterValidSubmit)
                ModalRystem.close();
            if (form.toast)
                new ToastRystem("toast-" + form.id, form.toast).show(data);
            if (form.request.selector)
                $(form.request.selector).html(data);
            if (form.update)
                Rystem.httpRequest(form.update, false, Rystem.stringEmpty, e, obj);
        }, function (data) {
            if (form.toast)
                new ToastRystem("toast-" + form.id, form.toast).show(data);
        });
    }
}

class ToastRystem extends Rystem {
    constructor(id, options) {
        super(id);
        this.options = options;
    }
    static defaultHtml = '<div id="{id}" class="toast {cssclass}" role="alert" aria-live="assertive" aria-atomic="true">' +
        '<div class="toast-header">' +
        '{header}' +
        '<button type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span>' +
        '</button>' +
        '</div>' +
        '<div class="toast-body">{message}</div>' +
        '</div>';
    static defaultContainer = '<div id="toast-container" style="position:absolute;top:20px;right:20px;"></div>';
    show(message, header = Rystem.stringEmpty) {
        let toast = this;
        if ($("#toast-container").length == 0)
            $("body").append(ToastRystem.defaultContainer);
        let htmlMessage = ToastRystem.defaultHtml
            .replace("{id}", toast.id)
            .replace("{message}", message)
            .replace("{header}", header)
            .replace("{cssClass}", toast.options.cssClass);
        $("#toast-container").append(htmlMessage);
        let $id = $('#' + toast.id);
        $id.toast(toast.options);
        $id.toast('show');
        $id.on('hidden.bs.toast', function () {
            $id.remove();
        })
    }
}