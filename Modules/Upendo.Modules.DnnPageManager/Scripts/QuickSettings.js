var dnnspamodule = dnnspamodule || {};

dnnspamodule.quickSettings = function(root, moduleId) {
    console.log(moduleId);
    var utils = new common.Utils();
    var alert = new common.Alert();
    var parentSelector = "[id='" + root + "']";
    // Setup your settings service endpoint
    /*
    var service = {
        path: "DnnSpaModule",
        framework: $.ServicesFramework(moduleId)
    }
    service.baseUrl = service.framework.getServiceRoot(service.path) + "Settings/";
    */
    var service = {
        path: "Upendo.Modules.DnnPageManager",
        framework: $.ServicesFramework(moduleId),
        controller: "Settings"
    }
    service.baseUrl = service.framework.getServiceRoot(service.path);

    var FirstTimeSaveSettings = function () {
        var deferred = $.Deferred();
        var params = {
            title: true,
            description: true,
            keywords: false
        };
        utils.get("POST", "save", service, params,
            function (data) {
                deferred.resolve();
                location.reload();
            },
            function (error, exception) {
                // fail
                var deferred = $.Deferred();
                deferred.reject();
                alert.danger({
                    selector: parentSelector,
                    text: error.responseText,
                    status: error.status
                });
            },
            function () {
            });
        return deferred.promise();
    };
    var SaveSettings = function () {
        
        var title = $('#Title').is(":checked");
        var description = $('#Description').is(":checked");
        var keywords = $('#Keywords').is(":checked");
        var deferred = $.Deferred();
        var params = {
            title: title,
            description: description,
            keywords: keywords
        };

        utils.get("POST", "save", service, params,
            function (data) {
                
                deferred.resolve();
                location.reload();
            },
            function (error, exception) {
                // fail
                var deferred = $.Deferred();
                deferred.reject();
                alert.danger({
                    selector: parentSelector,
                    text: error.responseText,
                    status: error.status
                });
            },
            function () {
            });

        return deferred.promise();        
    };

    var CancelSettings = function () {
        var deferred = $.Deferred();
        deferred.resolve();
        return deferred.promise();
    };

    var LoadSettings = function () {
        var params = {};
        
        utils.get("GET", "LoadSettings", service, params,
            function (data) {                
                //$('#Title').prop('checked', true);
                //$('#Description').prop('checked', false);
                //$('#Keywords').prop('checked', true);               
                $('#Title').prop('checked', data.title == "true");
                $('#Description').prop('checked', data.description == "true");
                $('#Keywords').prop('checked', data.keywords == "true");
                //$('.myCheckbox')[0].checked = true;
                var stateModule = localStorage.getItem('stateModule')
                if (data.title == null && stateModule === "inPage" && $('#loadPageMod').text() === "Loading....") {
                    window.location.reload();
                }
                if (data.title == null && $('#loadPageMod').text() !== "Loading....") {
                    FirstTimeSaveSettings();
                }
                localStorage.setItem('stateModule', "inPage");
                if ($('#loadPageMod').text() !== "Loading....") {
                    localStorage.removeItem('stateModule');
                }
            },
            function (error, exception) {
                // fail
                console.log("12345657897");
                console.log(error);
                alert.danger({
                    selector: parentSelector,
                    text: error.responseText,
                    status: error.status
                });
            },
            function () {
            });
    };

    var init = function () {
        // Wire up the default save and cancel buttons
        $(root).dnnQuickSettings({
            moduleId: moduleId,
            onSave: SaveSettings,
            onCancel: CancelSettings
        });
        LoadSettings();
    }

    return {
        init: init
    }
};
