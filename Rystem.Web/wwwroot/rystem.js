class Rystem {
    static instances = new Array();
    constructor(id) {
        this.id = id;
        Rystem.instances.push(this);
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

    static httpRequest(request, withLoader, query, event, obj, onSuccess, onFailure, feedback = false, feedbackNotOk = true) {
        if (withLoader)
            Rystem.showLoader();
        $.ajax({
            type: request.method,
            enctype: 'multipart/form-data',
            processData: false,
            contentType: false,
            cache: false,
            url: request.url + (query && query.length > 0 ? (request.url.indexOf("?") > -1 ? "&" : "?") + query : Rystem.stringEmpty),
            data: request.data == "null" ? Rystem.stringEmpty : request.data,
            success: function (data) {
                if (request.onSuccess) {
                    request.onSuccess = eval(request.onSuccess);
                    request.onSuccess(data, event, obj);
                }
                if (onSuccess)
                    onSuccess(data);
                if (request.selector && request.selector.length > 0)
                    $(request.selector).html(data);

                if (withLoader)
                    Rystem.hideLoader();
                if (feedback)
                    ToastRystem.ok();
            },
            error: function (data) {
                if (request.onFailure) {
                    request.onFailure = eval(request.onFailure);
                    request.onFailure(data, event, obj);
                if (onFailure)
                    onFailure(data);
                if (withLoader)
                    Rystem.hideLoader();
                if (feedbackNotOk)
                    ToastRystem.notOk(data.responseText);
            }
        });
    }

    static generateGUID() {
        let d = new Date().getTime();
        let d2 = (performance && performance.now && (performance.now() * 1000)) || 0;
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            let r = Math.random() * 16;
            if (d > 0) {
                r = (d + r) % 16 | 0;
                d = Math.floor(d / 16);
            } else {
                r = (d2 + r) % 16 | 0;
                d2 = Math.floor(d2 / 16);
            }
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
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
        if (ModalRystem.hasActive())
            $("#" + ModalRystem.active().id).css("visibility", "visible");
    }
    static forceClose() {
        $("#" + ModalRystem.active().id).modal("hide");
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
                $("#" + ModalRystem.active().id).css("visibility", "hidden");
            ModalRystem.actives.push(modal);
        });
    }
    hide() {
        if (this.update)
            Rystem.httpRequest(this.update, "true");
        $("#" + this.id).remove();
    }
}

class DropdownRystem extends Rystem {
    constructor(id, itemName, request, update) {
        super(id);
        this.selector = "." + id;
        this.itemName = itemName;
        this.request = request;
        this.update = update;
    }
    show() {
        let dropdown = this;
        $(dropdown.selector).selectpicker();
        if (dropdown.request)
            $(dropdown.selector).on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) {
                let selectedItems = $(e.target).val();
                if (!Array.isArray(selectedItems))
                    selectedItems = [selectedItems];
                let query = Rystem.stringEmpty;
                for (let i = 0; i < selectedItems.length; i++) {
                    query += dropdown.itemName + "=" + selectedItems[i];
                    if (i < selectedItems.length - 1)
                        query += "&";
                }
                Rystem.httpRequest(dropdown.request, false, query, e, e.target, function () {
                    if (dropdown.update)
                        Rystem.httpRequest(dropdown.update, false, Rystem.stringEmpty, e, e.target);
                }, null);
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
        form.request.data = new FormData(obj);
        Rystem.httpRequest(form.request, true, Rystem.stringEmpty, e, obj, function (data) {
            if (form.ifInModalCloseAfterValidSubmit)
                ModalRystem.forceClose();
            if (form.toast)
                new ToastRystem("toast-" + form.id, form.toast).show(data);
            if (form.update)
                Rystem.httpRequest(form.update, false, Rystem.stringEmpty, e, obj);
        }, function (data) {
            if (form.toast)
                new ToastRystem("toast-" + form.id, form.toast).show(data);
        });
    }
}

class AjaxButtonRystem extends Rystem {
    constructor(id, request, ifInModalCloseAfterValidSubmit, toast, update) {
        super(id);
        this.request = request;
        this.ifInModalCloseAfterValidSubmit = ifInModalCloseAfterValidSubmit;
        this.toast = toast;
        this.update = update;
    }

    submit(e, obj) {
        e.preventDefault();
        let button = this;
        Rystem.httpRequest(button.request, true, Rystem.stringEmpty, e, obj, function (data) {
            if (button.ifInModalCloseAfterValidSubmit)
                ModalRystem.forceClose();
            if (button.toast)
                new ToastRystem("toast-" + button.id, button.toast).show(data);
            if (button.update)
                Rystem.httpRequest(button.update, false, Rystem.stringEmpty, e, obj);
        }, function (data) {
            if (button.toast)
                new ToastRystem("toast-" + button.id, button.toast).show(data);
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
    static defaultContainer = '<div class="toast-container"></div>';
    show(message, header = Rystem.stringEmpty) {
        let toast = this;
        if ($(".toast-container").length == 0)
            $("body").append(ToastRystem.defaultContainer);
        let htmlMessage = ToastRystem.defaultHtml
            .replace("{id}", toast.id)
            .replace("{message}", message)
            .replace("{header}", header)
            .replace("{cssClass}", toast.options ? toast.options.cssClass : Rystem.stringEmpty);
        $(".toast-container").append(htmlMessage);
        let $id = $('#' + toast.id);
        $id.toast(toast.options);
        $id.toast('show');
        $id.on('hidden.bs.toast', function () {
            $id.remove();
        })
    }
    static ok(message = Rystem.stringEmpty) {
        let toast = new ToastRystem("OK-" + Rystem.generateGUID(),
            {
                delay: 5000,
                cssClass: "toast-message-ok"
            });
        if (message.length > 0)
            toast.show(message);
        else
            toast.show("OK");
    }
    static notOk(message = Rystem.stringEmpty) {
        let toast = new ToastRystem("NOTOK-" + Rystem.generateGUID(), {
            delay: 5000,
            cssClass: "toast-message-notok"
        });
        if (message.length > 0)
            toast.show(message);
        else
            toast.show("NOT OK");
    }
}


class CarouselRystem extends Rystem {
    constructor(id, container, elements, options) {
        super(id);
        this.container = container;
        this.$container = $("#" + this.container);
        if (this.$container.length == 0) {
            $("body").append("<div id='" + container.substr(1) + "'></div>");
            this.$container = $("#" + this.container);
        }
        this.elements = elements;
        let elementList = "";
        for (let i = 0; i < this.elements.length; i++) {
            const element = this.elements[i];
            if (element.link && element.link.length > 0)
                elementList += "<a href='" + element.link + "'>";
            if (element.content.indexOf("http") == 0)
                elementList += "<img class='swiper-image' src='" + element.content + "' />";
            else
                elementList += element.content;
            if (element.link && element.link.length > 0)
                elementList += "</a>";
        }
        this.options = options;
        this.$container.html(CarouselRystem.defaultXml.replace("{id}", this.id).replace("{contents}", elementList));
        if (!this.options.pagination)
            $("#" + this.id + " .swiper-pagination").remove();
        if (!this.options.navigation) {
            $("#" + this.id + " .swiper-button-next").remove();
            $("#" + this.id + " .swiper-button-prev").remove();
        }
        this.$id = $("#" + id);
        this.that = new Swiper(this.$id, this.options);
    }
    static defaultXml = "<div id='{id}' class='swiper-container' style='display:none;'>" +
        "<div class='swiper-wrapper'>{contents}</div>" +
        "<div class='swiper-pagination'></div>" +
        "<div class='swiper-button-next'></div>" +
        "<div class='swiper-button-prev'></div>" +
        "</div>";
    show() {
        this.$id.css("display", "block");
    }
}

class ChartRystem extends Rystem {
    constructor(id, data) {
        super(id);
        this.data = data;
        this.$id = $("#" + id);
    }
    show() {
        let ctx = this.$id[0].getContext('2d');
        this.chart = new Chart(ctx, this.data);
    }
}

class TableRystem extends Rystem {
    constructor(id, language) {
        super(id);
        this.language = language;
    }
    show() {
        if (this.language)
            $("#" + this.id).DataTable({
                language: { url: this.language }
            });
        else
            $("#" + this.id).DataTable();
    }
}