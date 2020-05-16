class Rystem {
    constructor(id) {
        this.id = id;
    }
    getName() {
        return "Rystem class";
    }
}

class Modal extends Rystem {
    static actives = new Array();
    static standardHtml = '<div class="modal fade" id="{modalid}" tabindex="-1" style="z-index:{zindex};" role="dialog" aria-hidden="true">' +
        '<div class="modal-dialog" role="document">' +
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
        console.log(this.update);
    }
    static hasActive() {
        return Modal.actives.length > 0;
    }
    static active() {
        return Modal.actives[Modal.actives.length - 1];
    }
    static close() {
        Modal.actives.pop().hide();
    }
    show(event, obj, request) {
        let modal = this;
        $("body").append(Modal.standardHtml.replace("{modalid}", this.id).replace("{zindex}", (1050 + Modal.actives.length)));
        $(this.id).on("hide.bs.modal", function () {
            Modal.close();
        });
        $("#loader").show();
        $.ajax({
            type: request.method,
            url: request.url,
            data: request.data == "null" ? "" : request.data,
            success: function (data) {
                $("#" + modal.id + " .modal-body").html(data);
                $("#" + modal.id).modal('show');
                if (Modal.hasActive())
                    $(Modal.active().id).css("visibility", "hidden");
                Modal.actives.push(modal);
                if (request.onSuccess)
                    request.onSuccess(data);
            },
            error: function (data) {
                $("#loader").hide();
                if (request.onFailure)
                    request.onFailure(data);
            }
        });
    }
    hide() {
        if (this.update && this.update.selector && this.update.selector.length > 0) {
            let request = this.update;
            $.ajax({
                type: this.update.method,
                url: this.update.url,
                data: this.update.data == "null" ? "" : this.update.data,
                success: function (data) {
                    $(request.selector).html(data);
                    if (request.onSuccess)
                        request.onSuccess(data);
                },
                error: function (data) {
                    $("#loader").hide();
                    if (request.onFailure)
                        request.onFailure(data);
                }
            });
        }
        $("#" + this.id).remove();
    }
}