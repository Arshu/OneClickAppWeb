/* spin.js/MIT license */
!function (t, e, i) { var o = ["webkit", "Moz", "ms", "O"], r = {}, n; function a(t, i) { var o = e.createElement(t || "div"), r; for (r in i) o[r] = i[r]; return o } function s(t) { for (var e = 1, i = arguments.length; e < i; e++) t.appendChild(arguments[e]); return t } var f = function () { var t = a("style", { type: "text/css" }); s(e.getElementsByTagName("head")[0], t); return t.sheet || t.styleSheet }(); function l(t, e, i, o) { var a = ["opacity", e, ~~(t * 100), i, o].join("-"), s = .01 + i / o * 100, l = Math.max(1 - (1 - t) / e * (100 - s), t), p = n.substring(0, n.indexOf("Animation")).toLowerCase(), u = p && "-" + p + "-" || ""; if (!r[a]) { f.insertRule("@" + u + "keyframes " + a + "{" + "0%{opacity:" + l + "}" + s + "%{opacity:" + t + "}" + (s + .01) + "%{opacity:1}" + (s + e) % 100 + "%{opacity:" + t + "}" + "100%{opacity:" + l + "}" + "}", f.cssRules.length); r[a] = 1 } return a } function p(t, e) { var r = t.style, n, a; if (r[e] !== i) return e; e = e.charAt(0).toUpperCase() + e.slice(1); for (a = 0; a < o.length; a++) { n = o[a] + e; if (r[n] !== i) return n } } function u(t, e) { for (var i in e) t.style[p(t, i) || i] = e[i]; return t } function c(t) { for (var e = 1; e < arguments.length; e++) { var o = arguments[e]; for (var r in o) if (t[r] === i) t[r] = o[r] } return t } function d(t) { var e = { x: t.offsetLeft, y: t.offsetTop }; while (t = t.offsetParent) e.x += t.offsetLeft, e.y += t.offsetTop; return e } var h = { lines: 12, length: 7, width: 5, radius: 10, rotate: 0, corners: 1, color: "#000", speed: 1, trail: 100, opacity: 1 / 4, fps: 20, zIndex: 2e9, className: "spinner", top: "auto", left: "auto", position: "relative" }; function m(t) { if (!this.spin) return new m(t); this.opts = c(t || {}, m.defaults, h) } m.defaults = {}; c(m.prototype, { spin: function (t) { this.stop(); var e = this, i = e.opts, o = e.el = u(a(0, { className: i.className }), { position: i.position, width: 0, zIndex: i.zIndex }), r = i.radius + i.length + i.width, s, f; if (t) { t.insertBefore(o, t.firstChild || null); f = d(t); s = d(o); u(o, { left: (i.left == "auto" ? f.x - s.x + (t.offsetWidth >> 1) : parseInt(i.left, 10) + r) + "px", top: (i.top == "auto" ? f.y - s.y + (t.offsetHeight >> 1) : parseInt(i.top, 10) + r) + "px" }) } o.setAttribute("aria-role", "progressbar"); e.lines(o, e.opts); if (!n) { var l = 0, p = i.fps, c = p / i.speed, h = (1 - i.opacity) / (c * i.trail / 100), m = c / i.lines; (function y() { l++; for (var t = i.lines; t; t--) { var r = Math.max(1 - (l + t * m) % c * h, i.opacity); e.opacity(o, i.lines - t, r, i) } e.timeout = e.el && setTimeout(y, ~~(1e3 / p)) })() } return e }, stop: function () { var t = this.el; if (t) { clearTimeout(this.timeout); if (t.parentNode) t.parentNode.removeChild(t); this.el = i } return this }, lines: function (t, e) { var i = 0, o; function r(t, o) { return u(a(), { position: "absolute", width: e.length + e.width + "px", height: e.width + "px", background: t, boxShadow: o, transformOrigin: "left", transform: "rotate(" + ~~(360 / e.lines * i + e.rotate) + "deg) translate(" + e.radius + "px" + ",0)", borderRadius: (e.corners * e.width >> 1) + "px" }) } for (; i < e.lines; i++) { o = u(a(), { position: "absolute", top: 1 + ~(e.width / 2) + "px", transform: e.hwaccel ? "translate3d(0,0,0)" : "", opacity: e.opacity, animation: n && l(e.opacity, e.trail, i, e.lines) + " " + 1 / e.speed + "s linear infinite" }); if (e.shadow) s(o, u(r("#000", "0 0 4px " + "#000"), { top: 2 + "px" })); s(t, s(o, r(e.color, "0 0 1px rgba(0,0,0,.1)"))) } return t }, opacity: function (t, e, i) { if (e < t.childNodes.length) t.childNodes[e].style.opacity = i } }); (function () { function t(t, e) { return a("<" + t + ' xmlns="urn:schemas-microsoft.com:vml" class="spin-vml">', e) } var e = u(a("group"), { behavior: "url(#default#VML)" }); if (!p(e, "transform") && e.adj) { f.addRule(".spin-vml", "behavior:url(#default#VML)"); m.prototype.lines = function (e, i) { var o = i.length + i.width, r = 2 * o; function n() { return u(t("group", { coordsize: r + " " + r, coordorigin: -o + " " + -o }), { width: r, height: r }) } var a = -(i.width + i.length) * 2 + "px", f = u(n(), { position: "absolute", top: a, left: a }), l; function p(e, r, a) { s(f, s(u(n(), { rotation: 360 / i.lines * e + "deg", left: ~~r }), s(u(t("roundrect", { arcsize: i.corners }), { width: o, height: i.width, left: i.radius, top: -i.width >> 1, filter: a }), t("fill", { color: i.color, opacity: i.opacity }), t("stroke", { opacity: 0 })))) } if (i.shadow) for (l = 1; l <= i.lines; l++) p(l, -2, "progid:DXImageTransform.Microsoft.Blur(pixelradius=2,makeshadow=1,shadowopacity=.3)"); for (l = 1; l <= i.lines; l++) p(l); return s(e, f) }; m.prototype.opacity = function (t, e, i, o) { var r = t.firstChild; o = o.shadow && o.lines || 0; if (r && e + o < r.childNodes.length) { r = r.childNodes[e + o]; r = r && r.firstChild; r = r && r.firstChild; if (r) r.opacity = i } } } else n = p(e, "animation") })(); if (typeof define == "function" && define.amd) define(function () { return m }); else t.Spinner = m }(window, document);

/******************************************************************************************************************/

function JsAction(pageName, actionName, actionParam) {
    var orgLocation = window.location.href;

    try {

        var actionHref = 'Action=' + actionName;
        if ((actionParam) && (actionParam.length > 0)) {
            actionHref += '&Param=' + actionParam;
        }
        if ((pageName) && (pageName.length > 0)) {
            actionHref = 'Page=' + pageName + '&' + actionHref;
        }
        window.location.replace("?" + actionHref);
    } catch (e) {
        showError("JsAction Error [" + e.get_Message + "]");
    }
}

function setFlipType(flipSelect) {
    var flipType = flipSelect.options[flipSelect.selectedIndex].value;
    if (flipType != "0") {
        JsAction("Hello", flipType, "")
    }
}

function getRootUrl(url) {
    return url.toString().replace(/^(.*\/\/[^\/?#]*).*$/, "$1");
}

/******************************************************************************************************************/

// Creating spinner see <a href="http://fgnass.github.com/spin.js/">http://fgnass.github.com/spin.js/</a> for configuration wizzard
var opts = {
    lines: 13, // The number of lines to draw
    length: 7, // The length of each line
    width: 4, // The line thickness
    radius: 10, // The radius of the inner circle
    rotate: 0, // The rotation offset
    color: '#efefef', // #rgb or #rrggbb
    speed: 0.75, // Rounds per second
    trail: 50, // Afterglow percentage
    shadow: true, // Whether to render a shadow
    hwaccel: true, // Whether to use hardware acceleration
    className: 'spinner', // The CSS class to assign to the spinner
    zIndex: 2e9, // The z-index (defaults to 2000000000)
    top: 'auto', // Top position relative to parent in px
    left: 'auto' // Left position relative to parent in px
};

var spinner = new Spinner(opts);
var ajax_cnt = 0; // Support for parallel AJAX requests

function ajaxStart(progressAnchorElmId) {

    var spinnerCenter = document.createElement("div");
    spinnerCenter.id = "spinner_center";
    //spinnerCenter.style = "display:block;position:absolute;top:15px;right:50%;";
    spinnerCenter.style.display = "block";
    spinnerCenter.style.position = "absolute";
    spinnerCenter.style.top = "15px";
    spinnerCenter.style.right = "50%";

    var anchorElm = document.getElementById(progressAnchorElmId);
    if (anchorElm) {
        anchorElm.parentNode.insertBefore(spinnerCenter, anchorElm);
    } else {
        var _body = document.getElementsByTagName('body')[0];
        _body.insertBefore(spinnerCenter, _body.firstChild);
    }

    spinner.spin(spinnerCenter);
    ajax_cnt++;
}

function ajaxStop(progressAnchorElmId) {

    ajax_cnt--;
    if (ajax_cnt <= 0) {
        spinner.stop();
        var spinnerCenter = document.getElementById('spinner_center');
        if (spinnerCenter) spinnerCenter.parentNode.removeChild(spinnerCenter);
        ajax_cnt = 0;

        //var anchorElm = document.getElementById(progressAnchorElmId);
        //if (anchorElm) {
        //    anchorElm.style.display = "none";
        //}
    }
}

function showAjaxTiming(returnAjax, clear) {
    if (returnAjax.hasOwnProperty('time') === true) {
        var ajaxTimeElm = document.getElementById('ajaxTime');
        if (ajaxTimeElm) {
            if (clear === true) ajaxTimeElm.innerHTML = "";

            var ajaxTime = "(" + returnAjax.time + ")";
            if (returnAjax.hasOwnProperty('serviceInfo') === true) {
                ajaxTime = "(<span title='" + returnAjax.serviceInfo + "'>" + returnAjax.time + "</span>)"
            }

            var ajaxTimeElmText = ajaxTimeElm.innerHTML.trim();
            if (ajaxTimeElmText.length == 0) {
                ajaxTimeElm.innerHTML = ajaxTime;
            }
            else {
                if (ajaxTimeElmText.indexOf(",") == -1) {
                    ajaxTimeElm.innerHTML = ajaxTimeElmText + "," + ajaxTime;
                } else {
                    var ajaxTimeArr = ajaxTimeElmText.split(",", 4);
                    ajaxTimeElm.innerHTML = ajaxTimeArr.join(",") + "," + ajaxTime;
                }
            }
        }
    }
}

function asyncJsonRequest(postRequestPath, request, progressElmId, endCallback) {

    ajaxStart(progressElmId);

    var http = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');
    http.open('POST', postRequestPath, true);
    http.setRequestHeader('Content-Type', 'application/json;charset=utf-8');
    //http.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    http.setRequestHeader('X-JSON-RPC', request.method);
    if (request.params) {
        http.send('{"id":' + request.id + ',"method":"' + request.method + '","params":' + request.params + '}');
    } else if (request.paramsArray) {
        http.send('{"id":' + request.id + ',"method":"' + request.method + '","paramsArray":' + request.paramsArray + '}');
    }


    http.onreadystatechange = function () {
        if (http.readyState == 4) {
            ajaxStop(progressElmId);
            var response = "";
            if (http.status == 200) {
                if (http.responseText.length > 0) {
                    response = JSON.parse(http.responseText);
                    response.text = http.responseText;
                    response.http = { text: http.responseText, headers: http.getAllResponseHeaders() };
                    showAjaxTiming(response, false);
                }
            }
            if (typeof (endCallback) === "function") endCallback(response);
        }
    }
}

/******************************************************************************************************************************************/

function addLoadEvent(func) {
    var oldonload = window.onload;
    if (typeof window.onload != 'function') {
        window.onload = func;
    } else {
        window.onload = function () {
            if (oldonload) {
                oldonload();
            }
            func();
        }
    }
}

var addEvent;
if (document.addEventListener) {
    addEvent = function (obj, type, fn) {
        obj.addEventListener(type, fn, false);
    };
}
else if (document.attachEvent) {
    addEvent = function (obj, type, fn) {
        obj['e' + type + fn] = fn;
        obj[type + fn] = function () { obj['e' + type + fn](window.event); };
        obj.attachEvent('on' + type, obj[type + fn]);
    };
}

function stopPropagation(e) {
    e = e || event; /* get IE event ( not passed ) */
    e.cancelBubble = e.stopPropagation ? e.stopPropagation() : true;
}

function getStyle(el, prop) {
    var doc = el.ownerDocument, view = doc.defaultView;
    if (view && view.getComputedStyle) {
        return view.getComputedStyle(el, '')[prop];
    }
    return el.currentStyle[prop];
}

function getElementsByClassName(node, classname) {
    if (node.getElementsByClassName) { // use native implementation if available
        return node.getElementsByClassName(classname);
    } else {
        return (function getElementsByClass(searchClass, node) {
            if (node === null)
                node = document;
            var classElements = [],
                els = node.getElementsByTagName("*"),
                elsLen = els.length,
                pattern = new RegExp("(^|\\s)" + searchClass + "(\\s|$)"), i, j;

            for (i = 0, j = 0; i < elsLen; i++) {
                if (pattern.test(els[i].className)) {
                    classElements[j] = els[i];
                    j++;
                }
            }
            return classElements;
        })(classname, node);
    }
}

if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, "");
    };
}

if (typeof String.prototype.ltrim !== 'function') {
    String.prototype.ltrim = function () {
        return this.replace(/^\s+/, "");
    };
}

if (typeof String.prototype.rtrim !== 'function') {
    String.prototype.rtrim = function () {
        return this.replace(/\s+$/, "");
    };
}

var isInteger_re = /^\s*(\+|-)?\d+\s*$/;
function isNumber(s) {
    return String(s).search(isInteger_re) !== -1;
    //return !isNaN(parseFloat(n)) && isFinite(n);
}

// checks that an input string looks like a valid email address.
var isEmail_re = /^\s*[\w\-\+_]+(\.[\w\-\+_]+)*\@[\w\-\+_]+\.[\w\-\+_]+(\.[\w\-\+_]+)*\s*$/;
function isEmail(s) {
    return String(s).search(isEmail_re) !== -1;
}

function haveJQuery() {
    return ('undefined' != typeof window.jQuery)
}

function showSelectedSection(selectElm, showElmId, hideElmIdList) {

    if (hideElmIdList) {
        for (var i = 0; i < hideElmIdList.length; i++) {
            var hideElm = document.getElementById(hideElmIdList[i]);
            if (hideElm) {
                hideElm.style.display = "none";
            }
        }
    }

    var showElm = document.getElementById(showElmId);
    if ((selectElm) && (showElm)) {
        if (selectElm.checked === true) {
            showElm.style.display = "block";
        }
    }
}

function setSelectedValue(selectElm, resultElmId) {
    var resultElm = document.getElementById(resultElmId);
    if (selectElm.options.length > 0) {
        var selectValue = selectElm.options[selectElm.selectedIndex].value;
        if (selectValue === "0") {
            resultElm.value = "";
        } else {
            resultElm.value = selectValue;
        }
    }
}

function appendSelectedValue(selectElm, resultElmId, delimiter) {
    var resultElm = document.getElementById(resultElmId);
    if (selectElm.options.length > 0) {
        var selectValue = selectElm.options[selectElm.selectedIndex].value;
        if (selectValue === "0") {
            resultElm.value = "";
        } else {
            if (resultElm.value.indexOf(selectValue) == -1) {
                if (resultElm.value.trim().length > 0) {
                    resultElm.value = resultElm.value.trim() + delimiter + selectValue.trim();
                } else {
                    resultElm.value = selectValue.trim();
                }
            }
        }
    }
}

function toggleDisplay(showHideElmId, actionElmId) {
    var showHideElm = document.getElementById(showHideElmId);
    if (showHideElm) {
        if ((showHideElm.style.display.trim() === 'block')
            || (showHideElm.style.display.trim().length == 0)) {
            showHideElm.style.display = 'none';
            if (actionElmId) {
                var actionElm = document.getElementById(actionElmId);
                if (actionElm) {
                    actionElm.style.backgroundPosition = 'right top'
                }
            }
        }
        else if (showHideElm) {
            showHideElm.style.display = 'block';
            if (actionElmId) {
                var actionElm = document.getElementById(actionElmId);
                if (actionElm) {
                    actionElm.style.backgroundPosition = 'left top'
                }
            }
        }
    } else {
        alert("Element is Null[" + showHideElmId + "]");
    }

}

function showObj(objId, objIdList) {
    var objElm = document.getElementById(objId);
    if (objElm) {
        objElm.style.display = "block";
    }
    if (objIdList != null) {
        for (var i = 0; i < objIdList.length; i++) {
            if (objIdList[i].length > 0) {
                showObj(objIdList[i], null);
            }
        }
    }
}

function hideObj(objId, objIdList) {
    var objElm = document.getElementById(objId);
    if (objElm) {
        objElm.style.display = "none";
    }
    if (objIdList != null) {
        for (var i = 0; i < objIdList.length; i++) {
            if (objIdList[i].length > 0) {
                hideObj(objIdList[i], null);
            }
        }
    }
}

function disableObj(objId) {
    var objElm = document.getElementById(objId);
    if (objElm) {
        objElm.setAttribute("disabled", "disabled");
    }
}

function enableObj(objId) {
    var objElm = document.getElementById(objId);
    if (objElm) {
        objElm.removeAttribute("disabled");
    }
}

function toggleObjClass(objId, objClassName) {
    var objElm = document.getElementById(objId);
    if (objElm) {
        if (objElm.className.indexOf(objClassName, 0) > -1) {
            objElm.className = objElm.className.replace(objClassName, "");
        } else {
            objElm.className = objElm.className + " " + objClassName;
        }
    }
}

function isVisibleObj(objId) {
    var objVisible = false;
    var objElm = document.getElementById(objId);
    if (objElm) {
        if (objElm.style.display === 'block') {
            objVisible = true;
        }
    }
    return objVisible;
}

function isDisabledObj(objId) {
    var objDisabled = false;
    var objElm = document.getElementById(objId);
    if (objElm) {
        if (objElm.getAttribute("disabled") !== null) {
            objDisabled = true;
        }
    }
    return objDisabled;
}

function hideMessage(msgLabelId, hideMsgIdList) {
    var msgLabelElm = document.getElementById(msgLabelId);
    if (msgLabelElm) {
        msgLabelElm.innerHTML = "";
        msgLabelElm.style.display = "none";
        msgLabelElm.className = msgLabelElm.className.replace("success", "");
        msgLabelElm.className = msgLabelElm.className.replace("error", "");
        msgLabelElm.className = msgLabelElm.className.replace("warning", "");
        msgLabelElm.className = msgLabelElm.className.replace("info", "");
    }
    if (hideMsgIdList != null) {
        for (var i = 0; i < hideMsgIdList.length; i++) {
            if (hideMsgIdList[i].length > 0) {
                hideMessage(hideMsgIdList[i], null);
            }
        }
    }
}

function showMessage(message, msgLabelId) {
    var msgLabelElm = document.getElementById(msgLabelId);
    if (msgLabelElm) {
        msgLabelElm.style.display = "block";
        msgLabelElm.className = msgLabelElm.className.replace("success", "");
        msgLabelElm.className = msgLabelElm.className.replace("error", "");
        msgLabelElm.className = msgLabelElm.className.replace("warning", "");
        msgLabelElm.className = msgLabelElm.className.replace("info", "");
        msgLabelElm.innerHTML = "<span style='word-space:normal'>" + message + "</span>";
        window.setTimeout("hideMessage('" + msgLabelId + "')", 5000);
    }
}

function showSuccess(message, msgLabelId) {
    var msgLabelElm = document.getElementById(msgLabelId);
    if (msgLabelElm) {
        msgLabelElm.style.display = "block";
        msgLabelElm.className = "success"
        msgLabelElm.innerHTML = "<span style='word-space:normal'>" + message + "</span>";
        window.setTimeout("hideMessage('" + msgLabelId + "')", 5000);
    }
}

function showError(message, msgLabelId) {
    var msgLabelElm = document.getElementById(msgLabelId);
    if (msgLabelElm) {
        msgLabelElm.style.display = "block";
        msgLabelElm.className = "error"
        msgLabelElm.innerHTML = "<span style='word-space:normal'>" + message + "</span>";
        window.setTimeout("hideMessage('" + msgLabelId + "')", 5000);
    }
}

function showWarning(message, msgLabelId) {
    var msgLabelElm = document.getElementById(msgLabelId);
    if (msgLabelElm) {        
        msgLabelElm.style.display = "block";
        msgLabelElm.className ="warning"
        msgLabelElm.innerHTML = "<span style='word-space:normal'>" + message + "</span>";
        window.setTimeout("hideMessage('" + msgLabelId + "')", 5000);
    }
}

function showInfo(message, msgLabelId, timeout) {
    var msgLabelElm = document.getElementById(msgLabelId);
    if (msgLabelElm) {
        msgLabelElm.style.display = "block";
        msgLabelElm.className = "info"
        msgLabelElm.innerHTML = "<span style='word-space:normal'>" + message + "</span>";
        if (timeout > 0) {
            window.setTimeout("hideMessage('" + msgLabelId + "')", timeout);
        }
    }
}

// Pass the checkbox name to the function
function getCheckedBoxes(chkboxName) {
    var checkboxes = document.getElementsByName(chkboxName);
    var checkboxesChecked = [];
    // loop over them all
    for (var i = 0; i < checkboxes.length; i++) {
        // And stick the checked ones onto an array...
        if (checkboxes[i].checked) {
            checkboxesChecked.push(checkboxes[i]);
        }
    }
    // Return the array if it is non-empty, or null
    //return checkboxesChecked.length > 0 ? checkboxesChecked : null;
    return checkboxesChecked;
}

isTouchInside = function (e) {
    var event = e.originalEvent;
    touch_inside = false;
    if (! event.touches[0]) return touch_inside;
    touchX = event.touches[0].pageX;
    touchY = event.touches[0].pageY;
    if (
      touchX > element_coords.left &&
      touchX < element_coords.right &&
      touchY > element_coords.top &&
      touchY < element_coords.bottom
    ) {
        touch_inside = true;
    }
    return touch_inside;
};


/*************************************************POPUP***********************************************************/

var overlayElement = null;
var modalWindowElement = null;

//detect touch and then automatically assign events
var isTouchSupported = 'ontouchstart' in window.document;
var startEvent = isTouchSupported ? 'touchstart' : 'mousedown';
var moveEvent = isTouchSupported ? 'touchmove' : 'mousemove';
var endEvent = isTouchSupported ? 'touchend' : 'mouseup';

//when window is resized. resizing is useful only when popups are opened
if (!window.addEventListener) {
    window.attachEvent("onresize", handleResize);
}
else {
    window.addEventListener("resize", handleResize, false);
}
//re-position the modal pop up to the center of the page
function handleResize() {
    if (modalWindowElement) {
        modalWindowElement.style.left = (window.innerWidth - modalWindowElement.offsetWidth) / 2 + "px";
        modalWindowElement.style.top = (window.innerHeight - modalWindowElement.offsetHeight) / 2 + "px";
    }
}

/* Common header for Pop Ups */
function createPopUpHeader(title) {
    //return the header after creating

    //create header for modal window area
    modalWindowHeader = document.createElement("div");
    modalWindowHeader.className = "modalWindowHeader";
    modalWindowHeader.innerHTML = "<p>" + title + "</p>";

    return modalWindowHeader;
}
function createPopUpContent(msg) {
    //return the content after creating

    //create modal window content area
    modalWindowContent = document.createElement("div");
    modalWindowContent.className = "modalWindowContent";

    modalWindowContent.innerHTML = "<p style='text-align:center; margin-top:10px;'>" + msg + "</p>";
    //create the place order button
    okBtn = document.createElement("div");
    okBtn.className = "redBtn okBtn";
    okBtn.innerHTML = "<p>OK</p>";
    okBtn.addEventListener(endEvent, function () { hidePopUpDiv(); }, false);

    modalWindowContent.appendChild(okBtn);
    return modalWindowContent;
}

//show the modal overlay and popup window
function showPopUpDiv(modalWindowHeader, modalWindowContent, width, height)
{
    overlayElement = document.createElement("div");
    overlayElement.className = 'modalOverlay';
    modalWindowElement = document.createElement("div");
    modalWindowElement.className = 'modalWindow';

    //position modal window element
    if (width.indexOf("%") === -1) {
        modalWindowElement.style.width = width + "px";
        modalWindowElement.style.left = (window.innerWidth - width) / 2 + "px";
    } else {
        modalWindowElement.style.width = width;
        modalWindowElement.style.left = (window.innerWidth - window.innerWidth * width.replace('%', '')/100) / 2 + "px";
    }
    if (height.indexOf("%") == -1) {
        modalWindowElement.style.height = height + "px";
        modalWindowElement.style.top = (window.innerHeight - height) / 2 + "px";
    } else {
        modalWindowElement.style.height = height;
        modalWindowElement.style.top = (window.innerHeight -window.innerHeight * height.replace('%', '')/100) / 2 + "px";
    }

    //add childs
    modalWindowElement.appendChild(modalWindowHeader);
    modalWindowElement.appendChild(modalWindowContent);
    document.body.appendChild(overlayElement);
    document.body.appendChild(modalWindowElement);
    setTimeout(function () {
        modalWindowElement.style.opacity = 1;
        overlayElement.style.opacity = 0.4;
        overlayElement.addEventListener(endEvent, hidePopUpDiv, false);
    }, 300);
}

//hide the modal overlay and popup window
function hidePopUpDiv() {
    modalWindowElement.style.opacity = 0;
    overlayElement.style.opacity = 0;
    overlayElement.removeEventListener(endEvent, hidePopUpDiv, false);
    setTimeout(function () {
        document.body.removeChild(overlayElement);
        document.body.removeChild(modalWindowElement);
    }, 400);
}

function showPopUp(headerText, contentDivId, popupWidth, popupHeight) {
    var headerTag = createPopUpHeader(headerText);
    var contentDivElm = document.getElementById(contentDivId);
    if (contentDivElm) {
        var contentTag = createPopUpContent(contentDivElm.innerHTML);
        showPopUpDiv(headerTag, contentTag, popupWidth, popupHeight);        
    } else {
        alert("Content Element is Null[" + contentDivId + "]");
    }
}

function hidePopUp() {    
    hidePopUpDiv();
}

/*******************************************************************************************************************/

/*!
 * Scroll Sneak
 * http://mrcoles.com/scroll-sneak/
 *
 * Copyright 2010, Peter Coles
 * Licensed under the MIT licenses.
 * http://mrcoles.com/media/mit-license.txt
 *
 * Date: Mon Mar 8 10:00:00 2010 -0500
 */
var ScrollSneak = function (prefix, wait) {
    // clean up arguments (allows prefix to be optional - a bit of overkill)
    if (typeof (wait) == 'undefined' && prefix === true) prefix = null, wait = true;
    prefix = (typeof (prefix) == 'string' ? prefix : window.location.host).split('_').join('');
    var pre_name;

    // scroll function, if window.name matches, then scroll to that position and clean up window.name
    this.scroll = function () {
        if (window.name.search('^' + prefix + '_(\\d+)_(\\d+)_') == 0) {
            var name = window.name.split('_');
            window.scrollTo(name[1], name[2]);
            window.name = name.slice(3).join('_');
        }
    }
    // if not wait, scroll immediately
    if (!wait) this.scroll();

    this.sneak = function () {
        // prevent multiple clicks from getting stored on window.name
        if (typeof (pre_name) == 'undefined') pre_name = window.name;

        // get the scroll positions
        var top = 0, left = 0;
        if (typeof (window.pageYOffset) == 'number') { // netscape
            top = window.pageYOffset, left = window.pageXOffset;
        } else if (document.body && (document.body.scrollLeft || document.body.scrollTop)) { // dom
            top = document.body.scrollTop, left = document.body.scrollLeft;
        } else if (document.documentElement && (document.documentElement.scrollLeft || document.documentElement.scrollTop)) { // ie6
            top = document.documentElement.scrollTop, left = document.documentElement.scrollLeft;
        }
        // store the scroll
        if (top || left) window.name = prefix + '_' + left + '_' + top + '_' + pre_name;
        return true;
    }
}
