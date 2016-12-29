$(function () {
    var pathname = window.location.pathname;
    if (pathname.includes("logs.html")){
        addDatePicker();
        getevents();

    } else {
        getdoors();
        getlatestevents();
    }

    $(".nav-item a").on("click", function (e) {
        e.preventDefault(); // --> if this handle didn't run first, this wouldn't work
        $("#trigger").trigger('click');
        document.location.href = this.href;
        return false;
    });


    $("#refresh-date-button").click(function () {
        var from = $("input[class*='datepicker-input min-date']").val();
        var to = $("input[class*='datepicker-input max-date']").val();
        getevents(from, to, null, null);
    });

    $("#btnDoorStatus").click(function () {
        $.ajax({
            url: "api/doors",
            success: function (data) {
                var str = '';
                for (var i = 0; i < data.length; i++) {
                    str = str + data[i]["name"] + ": " + getDoorStatus(data[i]["id"]);
                }
                alert(str);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.readyState == 4) {
                    showMessage(jqXHR.responseText);
                }
                else if (jqXHR.readyState == 0) {
                    showMessage("Oops! Connection error!");
                }
                else {
                    showMessage("Oops! Something went wrong!");
                }
                Success = false;
            }
        })




        
    });


});

function getdoors() {
    $.ajax({
        url: "api/doors",
        success: function (data) {
            $("#doorlist").empty();
            for (var i = 0; i < data.length; i++) {
                var id = data[i]["id"];
                var name = data[i]["name"];
                var status = data[i]["status"];
                var time = data[i]["laststatetime"];
                var li = '<li id="' + id + '" data-icon="false">';
                li = li + '<a href="javascript:click(\'' + id + '\',\'' + status + '\');">';
                li = li + '<img src="img/' + status + '.gif" />';
                li = li + '<h3>' + name + '</h3>';
                li = li + '<p>' + formatState(status, time, false) + '</p>';
                li = li + '</a>';
                li = li + '<a data-icon="info" href="javascript:showDoorStatus(\'' + id + '\');">';
                li = li + '</a></li>';
                $("#doorlist").append(li);
                $("#doorlist").listview('refresh');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.readyState == 4) {
                showMessage(jqXHR.responseText);
            }
            else if (jqXHR.readyState == 0) {
                showMessage("Oops! Connection error!");
            }
            else {
                showMessage("Oops! Something went wrong!");
            }
            Success = false;
        }
    })
};

function getlatestevents() {
    $.ajax({
        url: "api/doors/latestevents",
        success: function (data) {
            $("#eventlist").empty();
            for (var i = 0; i < data.length; i++) {
                var name = data[i]["name"];
                var status = data[i]["status"];
                var time = data[i]["date"];
                var client = data[i]["client"];
                var li = '<li id="event' + i + '" data-icon="false">';
                li = li + '<p><h5>' + name + '<span style="font-weight:normal">: ' + formatState(status, time, false) + ' from ' + client + '</span></h5></p>';
                li = li + '</li>';
                $("#eventlist").append(li);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showMessage("There was an error loading the latest events");
        }

    })
};

function getevents(from, to, count, page) {
    from = from != null && from.length > 0 ? new Date(from).getTime() : "";
    to = to != null && to.length > 0 ? new Date(to).getTime() : new Date().getTime();
    count = count == null ? 6 : count;
    page = page == null ? 1 : page;

    $.ajax({
        url: "api/doors/events?from="+from+"&to="+to+"&count="+count+"&page="+page,
        type: "get",
        success: function (data) {
            $("#eventlist").empty();
            for (var i = 0; i < data.logs.length; i++) {
                var name = data.logs[i]["name"];
                var status = data.logs[i]["status"];
                var time = data.logs[i]["date"];
                var client = data.logs[i]["client"];
                var li = '<li id="event' + i + '" data-icon="false">';
                li = li + '<p><h5>' + name + '<span style="font-weight:normal">: ' + formatState(status, time, false) + ' from ' + client + '</span></h5></p>';
                li = li + '</li>';
                $("#eventlist").append(li);
            }

            doPagination(data.count, data.page, data.total);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showMessage("There was an error loading the events");
        }

    })
};


var lastupdate = 0;

function formatState(state, time, utc) {
    //dateStr = dateFormat(new Date(parseInt(time) * 1000), "mmm dS, yyyy, h:MM TT");
    dateStr = dateFormat(time, "mmm dS, yyyy, h:MM TT", utc==null ? true : utc);
    return state.charAt(0).toUpperCase() + state.slice(1) + " as of " + dateStr;
};


function click(id, status) {
    updateSatus(id, getNextStatus(status));
    $.ajax({
        url: "api/toggle/" + id,
        type: 'PUT',
        contentType: 'application/json',
        data: { 'id': id },
        timeout: 60000,
        success: function (data) {
            var status = data["status"];
            var inconsistent = data["inconsistent"];
            updateSatus(id, status);
            if (inconsistent) {
                showMessage("Oops... Door status inconsistent!")
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.readyState == 4) {       // HTTP error (can be checked by XMLHttpRequest.status and XMLHttpRequest.statusText)
                showMessage(jqXHR.responseText);
            }
            else if (jqXHR.readyState == 0) {  // Network error
                showMessage("Oops! Connection error!");
            }
            else {
                showMessage("Oops! Something went wrong!");
            }            
            updateSatus(id, status);
        },
        complete: function (data) {
            getdoors();
            getlatestevents();
        }

    })
};

function showDoorStatus(id) {
    $.ajax({
        url: "api/doorstatus/" + id,
        type: 'GET',
        contentType: 'application/json',
        data: { 'id': id },
        success: function (data) {
            showMessage("Door is " + data, 'info');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showMessage("Oops! Something went wrong!");
        }
    })
};

function updateSatus(id, status) {
    //status = getNextStatus(status);
    var now = new Date();
    $("#" + id + " p").html(formatState(status, now, false));
    $("#" + id + " img").attr("src", "img/" + status + ".gif")
    $("#doorlist").listview('refresh');
}

function getNextStatus(status) {
    if (status == 'open') {
        status = 'closing';
    } else if (status == 'closed') {
        status = 'opening';
    } else if (status == 'closing') {
        status = 'closed';
    } else if (status == 'opening') {
        status = 'open';
    }
    return status;
}

function showMessage(message, type) {
    if (type == 'info') {
        $('.alert').css('background-color', '#cccccc');
    } else {
        $('.alert').css('background-color', '#c4453c');
    }
    $("#trigger").hide("slow");
    $("#alert div").text(message);
    $("#alert").slideDown();
    setTimeout(function () {
        $("#alert").slideUp();
        $("#trigger").show("slow");
    }, 5000);
}

function addDatePicker() {
    var picker = $("input[type='text']", this);
    picker.mobipick({
        dateFormat: "MM-dd-yyyy"
    });

    var mpFrom = $(".min-date").mobipick();
    var mpTo = $(".max-date").mobipick();
    mpFrom.on("change", function () {
        mpTo.mobipick("option", "minDate", mpFrom.mobipick("option", "date"));
    });
    mpTo.on("change", function () {
        mpFrom.mobipick("option", "maxDate", mpTo.mobipick("option", "date"));
    });
}

function doPagination(count, page, total) {
    var totalpages = Math.ceil(total / count);
    var lowpage = 1;
    var toppage = (totalpages < toppage) ? totalpages : 7;
    if (page > Math.ceil(toppage / 2)) {
        lowpage = page - 3;
        toppage = page + 3;
        if (totalpages < toppage) {
            toppage = totalpages;
            lowpage = toppage - 7;
        }

    }
    $(".pagination").empty();
    var low_class = page == 1 ? "class=disabled" : "";
    var li = '<li class="pag-item"><a href="#" data-id="low"' + low_class + '>«</a></li>';
    for (var i = lowpage; i <= toppage; i++) {
        var txtclass = i==page ? "class=active " : "";
        li = li + '<li class="pag-item"><a ' + txtclass + ' href="#" data-id="' + i + '">' + i + '</a></li>';
    }
    var high_class = page == totalpages ? "class=disabled" : "";
    li = li + '<li class="pag-item"><a href="#" data-id="high"' + high_class + '>»</a></li>';
    $(".pagination").append(li);

    // Add listener
    $(".pag-item a").click(function () {
        var from = $("input[class*='datepicker-input min-date']").val();
        var to = $("input[class*='datepicker-input max-date']").val();
        var activepage = $(".pag-item a.active").attr("data-id");
        var page = $(this).attr("data-id")
        if (page == 'low' && activepage > 1) {
            page =  parseInt(activepage) - 1;
        }
        if (page == 'high') {
            page =  parseInt(activepage) + 1;
        }
        getevents(from, to, null, page);
    });
};


//$(document).live('pageinit', poll);