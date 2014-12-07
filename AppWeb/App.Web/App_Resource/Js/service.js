
var _cache = {};
_cache.init = function () {
    if (typeof (this.memory) == 'undefined')
        this.memory = {};
};
_cache.set = function (key, value) {
    this.memory[key] = value;
};
_cache.get = function (key) {
    //if(console)
    //{
    //    console.log("memory: ");
    //    console.log(_cache.memory);

    //    console.log("key: "+key);
    //    console.log(_cache.memory[key]);
    //}

    if (typeof (_cache.memory[key]) != 'undefined') {
        if (_cache.memory[key] != null) {
            return _cache.memory[key];
        }
    }

    return false;
};
_cache.clear = function ()
{
    this.memory = {};
}
_cache.init();

function clearCache() {
    _cache.clear();
}

var nextRequestId = 0;
var jsonDbRequestPath = "/Handler/JsonDb.ashx";
var jsonFileRequestPath = "/Handler/JsonFile.ashx";

function haveSQLiteDatabase(dbName, msgId, successCallback, cacheResult)
{
    if (typeof (cacheResult) == "undefined") cacheResult = false;
    var key = dbName;
    var result = _cache.get(key);
    if (result == false) {
        var paramValues = '{';
        paramValues += '"dbName" : "' + dbName + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "HaveSQLiteDatabase",
            params: paramValues
        };

        asyncJsonRequest(jsonDbRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('message') === true) {
                        if (cacheResult) {
                            _cache.set(key, dbName);
                        }
                        else {
                            _cache.set(key, null);
                        }
                        successCallback();
                        haveResult = true;
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        showError(retError, msgId);
                    }
                }

                if (haveResult === false) {
                    showError("No Databases Found", msgId);
                }
            });
    }
    else {
        successCallback();
    }
}

function doSQLExecute(dbName, sqlMethod, sqlStatement, msgId, successCallback, cacheResult)
{
    if (typeof (cacheResult) == "undefined") cacheResult = false;
    //if (sqlMethod == "DoUpdate") cacheResult = false;
    //if (sqlMethod == "DoInsert") cacheResult = false;
    //if (sqlMethod == "DoDelete") cacheResult = false;
    var key = dbName + sqlMethod + sqlStatement;
    var result = _cache.get(key);
    if (result == false) {
        var paramValues = '{';
        paramValues += '"dbName" : "' + dbName + '"';
        paramValues += ', "sqlStatement" : "' + sqlStatement + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: sqlMethod,
            params: paramValues
        };

        asyncJsonRequest(jsonDbRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                var haveError = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('data') === true) {
                        var retData = retResult.data;
                        if (cacheResult) {
                            _cache.set(key, retData);
                        }
                        else {
                            _cache.set(key, null);
                        }
                        successCallback(retData);
                        haveResult = true;
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        showError(retError, msgId);
                        haveError = true;
                    }
                }

                if ((haveResult === false) && (haveError == false)) {
                    showError("No data Found", msgId);
                }

            });
    } else {
        successCallback(result);
    }
}

function haveConfigFile(configFilePath, msgId, successCallback, errorCallback, cacheResult)
{

    if (typeof (cacheResult) == "undefined") cacheResult = false;
    var key = configFilePath;
    var result = _cache.get(key);
    if (result == false) {

        var paramValues = '{';
        paramValues += '"relativePath" : "' + configFilePath + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "HaveFileData",
            params: paramValues
        };

        asyncJsonRequest(jsonFileRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('message') === true) {
                        if (cacheResult) {
                            _cache.set(key, configFilePath);
                        }
                        else {
                            _cache.set(key, null);
                        }
                        successCallback();
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        errorCallback(retError);
                    }
                }
            });
    } else {
        successCallback();
    }
}

function checkConfigData(configFilePath, configName, configExpectedValue, msgId, matchCallback, unmatchCallback, cacheResult)
{
    if (typeof (cacheResult) == "undefined") cacheResult = false;
    var key = configFilePath + configName;
    var result = _cache.get(key);
    if (result == false) {

        var paramValues = '{';
        paramValues += '"relativePath" : "' + configFilePath + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "GetFileData",
            params: paramValues
        };

        asyncJsonRequest(jsonFileRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('data') === true) {
                        var retData = retResult.data;
                        if (retData.hasOwnProperty(configName) === true) {
                            if (cacheResult) {
                                _cache.set(key, retData);
                            }
                            else {
                                _cache.set(key, null);
                            }
                            if (retData[configName] === configExpectedValue) {
                                matchCallback();
                            }
                            else {
                                unmatchCallback();
                            }
                        }
                        else {
                            showError('The Config Data [' + configName + '] does not exist', msgId);
                        }
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        showError(retError, msgId);
                    }
                }
            });
    }
    else {
        if (result[configName] === configExpectedValue) {
            matchCallback();
        }
        else {
            unmatchCallback();
        }
    }
}

function getConfigData(configFilePath, msgId, successCallback, cacheResult)
{
    if (typeof (cacheResult) == "undefined") cacheResult = false;
    var key = configFilePath;
    var result = _cache.get(key);
    if (result == false) {

        var paramValues = '{';
        paramValues += '"relativePath" : "' + configFilePath + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "GetFileData",
            params: paramValues
        };

        asyncJsonRequest(jsonFileRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('data') === true) {
                        var retData = retResult.data;
                        if (cacheResult) {
                            _cache.set(key, retData);
                        }
                        else {
                            _cache.set(key, null);
                        }
                        successCallback(retData);
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        showError(retError, msgId);
                    }
                }
            });
    } else {
        successCallback(result);
    }
}

function createConfigData(configFilePath, configNameArr, configValueArr, msgId, successCallback) {
    var isValid = true;
    var jsonFileData = '{';
    if ($.isArray(configNameArr)) {
        if (configNameArr.length === configValueArr.length) {
            for (var i = 0; i < configName.length; i++) {
                if (jsonFileData.length > 1) jsonFileData += ",";
                if (($.type(configValueArr) === "number") || ($.type(configValueArr) === "boolean")) {
                    jsonFileData += '"' + configNameArr[i] + '" : ' + configValueArr[i];
                }
                else {
                    jsonFileData += '"' + configNameArr[i] + '" : "' + configValueArr[i] + '"';
                }
            }
        }
        else {
            showError('Config Name and Value Array Length does not match', msgId);
            isValid = false;
        }
    } else {
        if (($.type(configValueArr) === "number") || ($.type(configValueArr) === "boolean")) {
            jsonFileData += '"' + configNameArr + '" : ' + configValueArr;
        } else {
            jsonFileData += '"' + configNameArr + '" : "' + configValueArr + '"';
        }
    }
    jsonFileData += '}';

    if (isValid === true) {
        var paramValues = '{';
        paramValues += '"relativePath" : "' + configFilePath + '"';
        paramValues += ', "jsonFileData" : ' + jsonFileData;
        paramValues += ', "overwrite" : false';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "SaveFileData",
            params: paramValues
        };

        asyncJsonRequest(jsonFileRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('message') === true) {
                        successCallback();
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        showError(retError, msgId);
                    }
                }
            });
    }
}

function saveConfigData(configFilePath, configNameArr, configValueArr, msgId, successCallback) {
    var isValid = true;
    var jsonFileData = '{';
    if ($.isArray(configNameArr)) {
        if (configNameArr.length === configValueArr.length) {
            for (var i = 0; i < configName.length; i++) {
                if (jsonFileData.length > 1) jsonFileData += ",";
                if (($.type(configValueArr) === "number") || ($.type(configValueArr) === "boolean")) {
                    jsonFileData += '"' + configNameArr[i] + '" : ' + configValueArr[i];
                }
                else {
                    jsonFileData += '"' + configNameArr[i] + '" : "' + configValueArr[i] + '"';
                }
            }
        }
        else {
            showError('Config Name and Value Array Length does not match', msgId);
            isValid = false;
        }
    } else {
        if (($.type(configValueArr) === "number") || ($.type(configValueArr) === "boolean")) {
            jsonFileData += '"' + configNameArr + '" : ' + configValueArr;
        } else {
            jsonFileData += '"' + configNameArr + '" : "' + configValueArr + '"';
        }
    }
    jsonFileData += '}';

    if (isValid === true) {
        var paramValues = '{';
        paramValues += '"relativePath" : "' + configFilePath + '"';
        paramValues += ', "jsonFileData" : ' + jsonFileData;
        paramValues += ', "overwrite" : true';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "SaveFileData",
            params: paramValues
        };

        asyncJsonRequest(jsonFileRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('message') === true) {
                        successCallback();
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        showError(retError, msgId);
                    }
                }
            });
    }
}

function setConfigData(configFilePath, jsonFileData, msgId, successCallback) {
    var isValid = true;

    if (isValid === true) {
        var paramValues = '{';
        paramValues += '"relativePath" : "' + configFilePath + '"';
        paramValues += ', "jsonFileData" : ' + jsonFileData;
        paramValues += ', "overwrite" : true';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "SaveFileData",
            params: paramValues
        };

        asyncJsonRequest(jsonFileRequestPath, request, msgId,
            function (response) {
                var haveResult = false;
                if (response.hasOwnProperty('Result') === true) {
                    var retResult = response.Result;
                    if (retResult.hasOwnProperty('message') === true) {
                        successCallback();
                    }
                    else if (retResult.hasOwnProperty('error') === true) {
                        var retError = retResult.error;
                        showError(retError, msgId);
                    }
                }
            });
    }
}
