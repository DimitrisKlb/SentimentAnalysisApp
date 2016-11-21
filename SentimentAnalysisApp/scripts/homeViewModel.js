var SRequestsState = {
    Open: 0,
    Pending: 1,
    Fulfilled: 2
}

var ViewModel = function () {
    var self = this;
    self.SRequestsOpen = ko.observableArray();
    self.SRequestsPending = ko.observableArray();
    self.SRequestsFulfilled = ko.observableArray();
    self.error = ko.observable();

    var SRequestsURI = '/api/SearchRequests/';
    var SRequestsByStatusURI = SRequestsURI + 'status/';

    function ajaxHelper(uri, method, data) {
        self.error(''); // Clear error message
        return $.ajax({
            type: method,
            url: uri,
            dataType: 'json',
            contentType: 'application/json',
            data: data ? JSON.stringify(data) : null
        }).fail(function (jqXHR, textStatus, errorThrown) {
            self.error(errorThrown);
        });
    }

    function getSRequests(status, SRequestsByStatus) {
        ajaxHelper(SRequestsByStatusURI + status, 'GET').done(function (data) {
            SRequestsByStatus(data);
        });
    }

    // Fetch the initial data.
    getSRequests(SRequestsState.Open, self.SRequestsOpen);
    getSRequests(SRequestsState.Pending, self.SRequestsPending);
    getSRequests(SRequestsState.Fulfilled, self.SRequestsFulfilled);    
};

ko.applyBindings(new ViewModel());
